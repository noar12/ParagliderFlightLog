using Microsoft.AspNetCore.Components.Web;
using Radzen;
using Radzen.Blazor;
using ParaglidingFlightLogWeb.ViewModels;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Identity;
using ParaglidingFlightLogWeb.Data;
using ParaglidingFlightLogWeb.Services;
using ParagliderFlightLog.Models;
using Microsoft.JSInterop;
using System.Text.Json;


namespace ParaglidingFlightLogWeb.Components.Pages;

/// <summary>
/// Page where the flights are listed and managed
/// </summary>
public partial class FlightsList
{
    private const int MAX_FILE_COUNT = 10;
    private const long MAX_FILE_SIZE = 2 * 1024 * 1024;
    private const string ALLOWED_FILE_EXTENSION = ".igc";
    private const int MAX_PHOTO_COUNT = 5;
    private const int MAX_PHOTO_SIZE = 7 * 1024 * 1024;
    private const string PHOTO_EXTENSION = ".jpg";
    private bool _isLittleScreen = false;

    /// <summary>
    /// Flight Id reflecting what flight is currently selected or used to access a flight directly at page loading
    /// </summary>
    [Parameter]
    public string FlightId { get; set; } = "";

    [Inject] ContextMenuService ContextMenuService { get; set; } = null!;
    [Inject] DialogService DialogService { get; set; } = null!;
    [Inject] NotificationService NotificationService { get; set; } = null!;
    [Inject] IWebHostEnvironment Environment { get; set; } = null!;
    [Inject] CoreService Mvm { get; set; } = null!;
    [Inject] ILogger<FlightsList> Logger { get; set; } = null!;
    [Inject] IJSRuntime JSRuntime { get; set; } = null!;
    [CascadingParameter] private Task<AuthenticationState> AuthenticationStateTask { get; set; } = null!;
    [Inject] UserManager<ApplicationUser> UserManager { get; set; } = null!;

    private RadzenDataGrid<FlightViewModel> _dataGrid = new();

    IList<FlightViewModel> SelectedFlights = [];
    private IJSObjectReference? _module;


    FlightViewModel? LastSelectedFlight => SelectedFlights.Count > 0 ? SelectedFlights[^1] : null;

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    /// <returns></returns>
    protected override async Task OnInitializedAsync()
    {
        var userClaim = (await AuthenticationStateTask).User;
        if (userClaim.Identity is not null && userClaim.Identity.IsAuthenticated)
        {
            var currentUser = await UserManager.GetUserAsync(userClaim);
            if (currentUser == null) return;
            string userId = currentUser.Id;
            await Mvm.Init(userId);
            Logger.LogInformation("Initialized for {User}", currentUser.UserName);
        }
    }

    /// <summary>
    /// <inheritdoc />
    /// </summary>
    /// <param name="firstRender"></param>
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        _module ??= await JSRuntime.InvokeAsync<IJSObjectReference>("import", "./helperV1_0_0.js");

        if (!firstRender || string.IsNullOrEmpty(FlightId))
        {
            return;
        }

        var flight = Mvm.FlightListViewModel.FirstOrDefault(x => x.FlightID == FlightId);
        if (flight is not null)
        {
            await _dataGrid.SelectRow(flight);
        }
    }

    void ShowContextMenuWithItems(MouseEventArgs args)
    {
        if (SelectedFlights.Count == 1)
        {
            ContextMenuService.Open(args,
                [
                    new() { Text = "Edit flight", Value = EFlightAction.Edit },
                    new() { Text = "Remove flights", Value = EFlightAction.Remove },
                ],
                OnMenuItemClick);
        }
    }

    void OnMenuItemClick(MenuItemEventArgs args)
    {
        if (args.Value is EFlightAction action)
        {
            switch (action)
            {
                case EFlightAction.Edit:
                    _ = OnEditFlight();
                    break;
                case EFlightAction.Remove:
                    _ = OnRemoveFlights(SelectedFlights);
                    break;
            }
        }

        ContextMenuService.Close();
    }

    enum EFlightAction
    {
        Edit,
        Remove,
    }

    async Task OnRemoveFlights(IList<FlightViewModel> flightsToRemove)
    {
        var answer = await DialogService.Confirm($"Are you sure you want to delete {SelectedFlights.Count} flight(s)?",
            "Flight remove confirmation", new ConfirmOptions() { OkButtonText = "Yes", CancelButtonText = "No" });
        if (answer == true)
        {
            foreach (FlightViewModel fvm in flightsToRemove)
            {
                Mvm.RemoveFlight(fvm);
            }
        }

        StateHasChanged();
        await _dataGrid.Reload();
    }

    async Task OnEditFlight()
    {
        await DialogService.OpenAsync<EditFlight>($"Edit flight",
            new Dictionary<string, object>() { { "FlightToEdit", LastSelectedFlight! }, { "ViewModel", Mvm } },
            new DialogOptions() { Resizable = true, Draggable = false });
        StateHasChanged();
    }

    async Task OnAddFlights(UploadChangeEventArgs e)
    {
        Logger.LogInformation("ContentRootPath is : {ContentRootPath}", Environment.ContentRootPath);
        var igcFilePaths = new List<string>();
        var files = e.Files.ToList();
        if (files.Count > MAX_FILE_COUNT)
        {
            NotifyUser($"Cannot accept more than {MAX_FILE_COUNT} files");
            return;
        }

        if (files.Select(f => f.Name)
            .Any(n => !n.ToLower().EndsWith(ALLOWED_FILE_EXTENSION)))
        {
            NotifyUser("Only igc file");
            return;
        }

        foreach (var file in files)
        {
            try
            {
                if (file.Size > MAX_FILE_SIZE)
                {
                    NotifyUser($"Individual file has to be smaller than {MAX_FILE_SIZE / 1024} kB");
                    return;
                }

                string tmpDirectory = Path.Combine(Environment.ContentRootPath, "tmp");
                if (!Directory.Exists(tmpDirectory))
                {
                    _ = Directory.CreateDirectory(tmpDirectory);
                }

                string trustedFileNameForFileStorage = Path.GetRandomFileName();
                string path = Path.Combine(tmpDirectory, trustedFileNameForFileStorage);
                await using FileStream fs = new(path, FileMode.Create);
                await file.OpenReadStream(MAX_FILE_SIZE).CopyToAsync(fs);
                igcFilePaths.Add(fs.Name);
                Logger.LogDebug("File copied to {Name}", fs.Name);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "File: {Filename}", file.Name);
            }
        }

        await Mvm.AddFlightsFromIGCAsync([.. igcFilePaths]);
        foreach (string filepath in igcFilePaths)
        {
            try
            {
                File.Delete(filepath);
            }
            catch (IOException ex)
            {
                Logger.LogError("{Message}", ex.Message);
            }
        }

        StateHasChanged();
        await _dataGrid.Reload();
    }

    private void NotifyUser(string message, NotificationSeverity severity = NotificationSeverity.Error)
    {
        NotificationService.Notify(new NotificationMessage
        {
            Severity = severity,
            Duration = 4000,
            Summary = message,
        });
    }

    private async Task AddPhotos(UploadChangeEventArgs e, FlightViewModel flight)
    {
        var files = e.Files.ToList();
        if (files.Count > MAX_PHOTO_COUNT)
        {
            NotifyUser($"Cannot accept more than {MAX_PHOTO_COUNT} files");
            return;
        }

        if (files.Select(f => f.Name)
            .Any(n => !n.ToLower().EndsWith(PHOTO_EXTENSION)))
        {
            NotifyUser("Only jpg file");
            return;
        }

        foreach (var file in files)
        {
            NotifyUser($"Starting {file.Name} upload...", NotificationSeverity.Info);
            try
            {
                if (file.Size > MAX_PHOTO_SIZE)
                {
                    NotifyUser($"Individual file has to be smaller than {MAX_PHOTO_SIZE / 1024} kB");
                    return;
                }

                using var memory = new MemoryStream();
                await file.OpenReadStream(MAX_PHOTO_SIZE).CopyToAsync(memory);
                await Mvm.SavePhoto(flight, memory);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "File: {Filename}", file.Name);
            }
        }

        NotifyUser($"{files.Count} file(s) have been uploaded", NotificationSeverity.Success);
        await InvokeAsync(StateHasChanged);
    }

    private async Task OpenShareFlight(FlightViewModel flight)
    {
        await DialogService.OpenAsync<ShareFlightForm>("Share Flight",
            new() { { "Flight", flight } },
            new DialogOptions() { Resizable = true, Draggable = false });
    }

    private void OnSelectedFlightChanged(IList<FlightViewModel> flights)
    {
        if (flights.Count != 1)
        {
            return;
        }

        SelectedFlights = flights;
        FlightId = flights[0].FlightID;
    }

    private async Task OnEditFlightButton(FlightViewModel flightToEdit)
    {
        SelectedFlights.Clear();
        SelectedFlights.Add(flightToEdit);
        FlightId = flightToEdit.FlightID;
        await OnEditFlight();
    }

    private async Task OnDeleteFlightButton(FlightViewModel flightToDelete)
    {
        SelectedFlights.Clear();
        SelectedFlights.Add(flightToDelete);
        FlightId = flightToDelete.FlightID;
        await OnRemoveFlights(SelectedFlights);
    }

    private async Task OnAddFlightWithoutIgcFile()
    {
        _ = await DialogService.OpenAsync<CreateNewManualFlight>("Add flight manually",
            new()
            {
                { "ViewModel", Mvm }
            });
    }

    private void OnMobileChange(bool matches)
    {
        _isLittleScreen = matches;
        StateHasChanged();
        _dataGrid.Reload();
    }
    private void OnRowRender(RowRenderEventArgs<FlightViewModel> args)
    {
        args.Expandable = _isLittleScreen;
    }
    private async Task OnJsonCopyClick(FlightViewModel flight)
    {
        if (_module is null) { return; }
        var flightExport = new FlightJsonExportModel()
        {
            Comment = flight.Comment,
            FlightType = flight.XcScore?.Type,
            GliderName = flight.GliderName,
            Objective = flight.Objective,
            TakeOffSiteName = flight.TakeOffSiteName,
            FlightDuration = flight.FlightDuration,
            MaxAltitude = flight.MaxAltitude,
            MaxClimb = flight.MaxClimb,
            MaxSink = flight.MaxSink,
            TakeOffDateTime = flight.TakeOffDateTime,
            RouteLength = flight.XcScore?.RouteLength,
            XcScore = flight.XcScore?.Points,
            AverageSpeed_kmh = flight.XcScore?.AverageSpeed_kmh
        };
        var options = new JsonSerializerOptions() { WriteIndented = true };
        string flightJson = JsonSerializer.Serialize(flightExport, options);
        await _module.InvokeAsync<IJSObjectReference>("copyToClipboard", flightJson);
    }
    private sealed class FlightJsonExportModel
    {
        public DateTime TakeOffDateTime { get; set; }
        public TimeSpan FlightDuration { get; set; }
        public double? XcScore { get; set; }
        public string? FlightType { get; set; }
        public required string TakeOffSiteName { get; set; }
        public required string Objective { get; set; }
        public required string GliderName { get; set; }
        public double MaxAltitude { get; set; }
        public required string Comment { get; set; }
        public double? RouteLength { get; set; }

        public double MaxClimb { get; set; }
        public double MaxSink { get; set; }
        public double? AverageSpeed_kmh { get; set; }
    }
}
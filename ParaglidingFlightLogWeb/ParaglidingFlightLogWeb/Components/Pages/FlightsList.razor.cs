using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Web;
using Radzen;
using Radzen.Blazor;
using ParaglidingFlightLogWeb.ViewModels;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Identity;
using ParaglidingFlightLogWeb.Data;
using Microsoft.JSInterop;
using ParaglidingFlightLogWeb.Services;


namespace ParaglidingFlightLogWeb.Components.Pages;

/// <summary>
/// Page where the flights are listed and managed
/// </summary>
public partial class FlightsList
{
    private const int MAX_FILE_COUNT = 10;
    private const long MAX_FILE_SIZE = 2 * 1024 * 1024;
    private const string ALLOWED_FILE_EXTENSION = ".igc";

    /// <summary>
    /// Flight Id reflecting what flight is currently selected or used to acces a flight directly at page loading
    /// </summary>
    [Parameter] public string FlightId { get; set; } = "";

    [Inject] ContextMenuService ContextMenuService { get; set; } = null!;
    [Inject] DialogService DialogService { get; set; } = null!;
    [Inject] NotificationService NotificationService { get; set; } = null!;
    [Inject] IWebHostEnvironment Environment { get; set; } = null!;
    [Inject] CoreService Mvm { get; set; } = null!;
    [Inject] ILogger<FlightsList> Logger { get; set; } = null!;
    [CascadingParameter] private Task<AuthenticationState> AuthenticationStateTask { get; set; } = null!;
    [Inject] UserManager<ApplicationUser> UserManager { get; set; } = null!;

    private RadzenDataGrid<FlightViewModel> _dataGrid = new();

    IList<FlightViewModel> SelectedFlights = [];

    FlightViewModel? LastSelectedFlight
    {
        get
        {
            return SelectedFlights.Count > 0 ? SelectedFlights[SelectedFlights.Count - 1] : null;
        }
    }

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
        if (string.IsNullOrEmpty(FlightId))
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
        if (SelectedFlights is not null && SelectedFlights.Count == 1)
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

    async Task OnShowMapClick()
    {
        await DialogService.OpenAsync<ShowFlightOnMap>($"Flight trace on map",
            new Dictionary<string, object>() { { "FlightToShow", LastSelectedFlight! }, { "Height", 500 } },
            new DialogOptions() { Width = "900px", Resizable = true, Draggable = false });
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
            new DialogOptions() { Width = "700px", Height = "570px", Resizable = true, Draggable = false });
        StateHasChanged();
    }

    async Task OnAddFlights(InputFileChangeEventArgs e)
    {
        Logger.LogInformation("ContentRootPath is : {ContentRootPath}", Environment.ContentRootPath);
        var l_IgcFilePaths = new List<string>();

        if (e.FileCount > MAX_FILE_COUNT)
        {
            NotifyUser($"Cannot accept more than {MAX_FILE_COUNT} files");
            return;
        }

        if (e.GetMultipleFiles(MAX_FILE_COUNT).Select(f => f.Name)
            .Any(n => !n.ToLower().EndsWith(ALLOWED_FILE_EXTENSION)))
        {
            NotifyUser("Only igc file");
            return;
        }

        foreach (var file in e.GetMultipleFiles(MAX_FILE_COUNT))
        {
            try
            {
                if (file.Size > MAX_FILE_SIZE)
                {
                    NotifyUser($"Individual file has to be smaller than {MAX_FILE_SIZE / 1024} kB");
                    return;
                }

                var trustedFileNameForFileStorage = Path.GetRandomFileName();
                var path = Path.Combine(Environment.ContentRootPath, "tmp", trustedFileNameForFileStorage);
                await using FileStream fs = new(path, FileMode.Create);
                await file.OpenReadStream(MAX_FILE_SIZE).CopyToAsync(fs);
                l_IgcFilePaths.Add(fs.Name);
                Logger.LogDebug("File copied to {fs.Name}", fs.Name);
            }
            catch (Exception ex)
            {
                Logger.LogError("File: {Filename} Error: {Error}", file.Name, ex.Message);
            }
        }

        Mvm.AddFlightsFromIGC([.. l_IgcFilePaths]);
        foreach (string filepath in l_IgcFilePaths)
        {
            try
            {
                System.IO.File.Delete(filepath);
            }
            catch (System.IO.IOException ex)
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
            Severity = severity, Duration = 4000, Summary = message,
        });
    }

    private void ComputeFlightScore()
    {
        Mvm.EnqueueFlightForScore(LastSelectedFlight);
    }
}
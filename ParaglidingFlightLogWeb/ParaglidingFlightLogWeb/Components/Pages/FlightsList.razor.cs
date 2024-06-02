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
using ParagliderFlightLog.XcScoreWapper;
using ParaglidingFlightLogWeb.Services;


namespace ParaglidingFlightLogWeb.Components.Pages
{
    public partial class FlightsList
	{
		
		[Inject] ContextMenuService ContextMenuService { get; set; } = null!;
		[Inject] DialogService DialogService { get; set; } = null!;
		[Inject] IWebHostEnvironment Environment { get; set; } = null!;
		[Inject] CoreService Mvm { get; set; } = null!;
		[Inject] ILogger<Index> Logger { get; set; } = null!;
		[CascadingParameter] private Task<AuthenticationState> AuthenticationStateTask { get; set; } = null!;
		[Inject] UserManager<ApplicationUser> UserManager { get; set; } = null!;

		private RadzenDataGrid<FlightViewModel> dataGrid = new();

		IList<FlightViewModel> SelectedFlights = new List<FlightViewModel>();
		FlightViewModel? LastSelectedFlight
		{
			get
			{
				return SelectedFlights.Count > 0 ? SelectedFlights[SelectedFlights.Count - 1] : null;
			}
		}

		protected override async Task OnInitializedAsync()
		{
			var userClaim = (await AuthenticationStateTask).User;
			if (userClaim.Identity is not null && userClaim.Identity.IsAuthenticated)
			{
				var currentUser = await UserManager.GetUserAsync(userClaim);
				if (currentUser == null) return;
				string userId = currentUser.Id;
				await Mvm.Init(userId);
			}
		}

		void ShowContextMenuWithItems(MouseEventArgs args)
		{
			if (SelectedFlights is not null && SelectedFlights.Count == 1)
			{
				ContextMenuService.Open(args,
										new List<ContextMenuItem>
										{
											new() { Text = "Edit flight", Value = EFlightAction.Edit },
											new() { Text = "Remove flights", Value = EFlightAction.Remove },
										},
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
		async Task OnShowMapClick(){
			await DialogService.OpenAsync<ShowFlightOnMap>($"Flight trace on map",
			new Dictionary<string, object>() { { "FlightToShow", LastSelectedFlight! }, { "Height", 500 } },
			new DialogOptions() { Width= "900px", Resizable = true, Draggable = false });
		}

		enum EFlightAction
		{
			Edit,
			Remove,
		}
		
		async Task OnRemoveFlights(IList<FlightViewModel> flightsToRemove)
		{
			var answer = await DialogService.Confirm($"Are you sure you want to delete {SelectedFlights.Count} flight(s)?", "Flight remove confirmation", new ConfirmOptions() { OkButtonText = "Yes", CancelButtonText = "No" });
			if (answer == true)
			{
				foreach (FlightViewModel fvm in flightsToRemove)
				{
					Mvm.RemoveFlight(fvm);
				}
			}

			StateHasChanged();
			await dataGrid.Reload();
		}

		async Task OnEditFlight()
		{
			await DialogService.OpenAsync<EditFlight>($"Edit flight", new Dictionary<string, object>() { { "FlightToEdit", LastSelectedFlight! }, { "ViewModel", Mvm } }, new DialogOptions() { Width = "700px", Height = "570px", Resizable = true, Draggable = false });
			StateHasChanged();
		}

		async Task OnAddFlights(InputFileChangeEventArgs e)
		{
			Logger.LogInformation("ContentRootPath is : {ContentRootPath}", Environment.ContentRootPath);
			var l_IgcFilePaths = new List<string>();
			foreach (var file in e.GetMultipleFiles())
			{
				try
				{
					long maxFileSize = 2048000;
					var trustedFileNameForFileStorage = Path.GetRandomFileName();
					var path = Path.Combine(Environment.ContentRootPath, "tmp", trustedFileNameForFileStorage);
					await using FileStream fs = new(path, FileMode.Create);
					await file.OpenReadStream(maxFileSize).CopyToAsync(fs);
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
			await dataGrid.Reload();
		}
		private void ComputeFlightScore(){
			Mvm.EnqueueFlight(LastSelectedFlight);
		}
	}
}
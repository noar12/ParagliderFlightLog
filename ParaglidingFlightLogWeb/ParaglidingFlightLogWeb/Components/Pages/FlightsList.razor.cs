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


namespace ParaglidingFlightLogWeb.Components.Pages
{
	public partial class FlightsList
	{
		private IJSObjectReference? module;
		private object? flightMap;
		[Inject] IJSRuntime jsRuntime { get; set; } = null!;
		[Inject] ContextMenuService ContextMenuService { get; set; } = null!;
		[Inject] DialogService DialogService { get; set; } = null!;
		[Inject] IWebHostEnvironment Environment { get; set; } = null!;
		[Inject] MainViewModel mvm { get; set; } = null!;
		[Inject] ILogger<Index> _logger { get; set; } = null!;
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
				await mvm.Init(userId);
			}
		}
		protected override async Task OnAfterRenderAsync(bool firstRender)
		{
			if (firstRender)
			{
				module = await jsRuntime.InvokeAsync<IJSObjectReference>("import", "./leafletmap.js");
				if (module != null)
				{
					flightMap = await module.InvokeAsync<IJSObjectReference>("load_map", "flightMap", 0, 0, 2);
				}
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

		enum EFlightAction
		{
			Edit,
			Remove,
		}
		async Task OnFlightClick(DataGridRowMouseEventArgs<FlightViewModel> e)
		{
			var flight = e.Data;
			double[][] latlngs = flight.GetLatLngsArray();
			if (module is not null)
			{
				//flightMap = await module.InvokeAsync<IJSObjectReference>("remove_all", flightMap); //this remove even the tile. It's a bit violent...
				flightMap = await module.InvokeAsync<IJSObjectReference>("add_polyline",
															 flightMap,
															 latlngs);
			}
		}
		async Task OnRemoveFlights(IList<FlightViewModel> flightsToRemove)
		{
			var answer = await DialogService.Confirm($"Are you sure you want to delete {SelectedFlights.Count} flight(s)?", "Flight remove confirmation", new ConfirmOptions() { OkButtonText = "Yes", CancelButtonText = "No" });
			if (answer == true)
			{
				foreach (FlightViewModel fvm in flightsToRemove)
				{
					mvm.RemoveFlight(fvm);
				}
			}

			StateHasChanged();
			await dataGrid.Reload();
		}

		async Task OnEditFlight()
		{
			await DialogService.OpenAsync<EditFlight>($"Edit flight", new Dictionary<string, object>() { { "FlightToEdit", LastSelectedFlight! }, { "ViewModel", mvm } }, new DialogOptions() { Width = "700px", Height = "570px", Resizable = true, Draggable = false });
			StateHasChanged();
		}

		async Task OnAddFlights(InputFileChangeEventArgs e)
		{
			_logger.LogInformation("ContentRootPath is : {ContentRootPath}", Environment.ContentRootPath);
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
					_logger.LogDebug("File copied to {fs.Name}", fs.Name);
				}
				catch (Exception ex)
				{
					_logger.LogError("File: {Filename} Error: {Error}", file.Name, ex.Message);
				}
			}

			mvm.AddFlightsFromIGC([.. l_IgcFilePaths]);
			foreach (string filepath in l_IgcFilePaths)
			{
				try
				{
					System.IO.File.Delete(filepath);
				}
				catch (System.IO.IOException ex)
				{
					_logger.LogError("{Message}", ex.Message);
				}
			}

			StateHasChanged();
			await dataGrid.Reload();
		}
	}
}
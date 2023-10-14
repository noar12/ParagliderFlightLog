using global::System.Collections.Generic;
using global::System.Linq;
using global::System.Threading.Tasks;
using global::Microsoft.AspNetCore.Components;
using System.Net.Http;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.Web.Virtualization;
using Microsoft.JSInterop;
using BlazorUI;
using BlazorUI.Shared;
using Radzen;
using Radzen.Blazor;
using System;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using ParagliderFlightLog.ViewModels;

namespace BlazorUI.Pages
{
    public partial class Index
    {
        private RadzenDataGrid<FlightViewModel> dataGrid = new();
        protected override void OnInitialized()
        {
        }

        IList<FlightViewModel> SelectedFlights = new List<FlightViewModel>();
        FlightViewModel? LastSelectedFlight
        {
            get
            {
                return SelectedFlights.Count > 0 ? SelectedFlights[SelectedFlights.Count - 1] : null;
            }
        }

        void ShowContextMenuWithItems(MouseEventArgs args)
        {
            if (SelectedFlights is not null && SelectedFlights.Count == 1)
            {
                ContextMenuService.Open(args, new List<ContextMenuItem> { new ContextMenuItem() { Text = "Edit flight", Value = flightAction.Edit }, new ContextMenuItem() { Text = "Remove flights", Value = flightAction.Remove }, }, OnMenuItemClick);
            }
        }

        void OnMenuItemClick(MenuItemEventArgs args)
        {
            if (args.Value is flightAction action)
            {
                switch (action)
                {
                    case flightAction.Edit:
                        OnEditFlight();
                        break;
                    case flightAction.Remove:
                        OnRemoveFlights(SelectedFlights);
                        break;
                }
            }

            ContextMenuService.Close();
        }

        enum flightAction
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
                    mvm.RemoveFlight(fvm);
                }
            }

            StateHasChanged();
            await dataGrid.Reload();
        }

        async Task OnEditFlight()
        {
            await DialogService.OpenAsync<EditFlight>($"Edit flight", new Dictionary<string, object>() { { "FlightToEdit", LastSelectedFlight }, { "ViewModel", mvm } }, new DialogOptions() { Width = "700px", Height = "570px", Resizable = true, Draggable = true });
            StateHasChanged();
        }

        async Task OnAddFlights(InputFileChangeEventArgs e)
        {
            _logger.LogInformation($"ContentRootPath is : {Environment.ContentRootPath}");
            List<string> l_IgcFilePaths = new List<string>();
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

            mvm.AddFlightsFromIGC(l_IgcFilePaths.ToArray());
            foreach (string filepath in l_IgcFilePaths)
            {
                try
                {
                    System.IO.File.Delete(filepath);
                }
                catch (System.IO.IOException ex)
                {
                    _logger.LogError(ex.Message);
                }
            }

            StateHasChanged();
            await dataGrid.Reload();
        }
        private string text = "";
        void InputTextChanged(string newText)
        {
            text = newText;
        }
    }
}
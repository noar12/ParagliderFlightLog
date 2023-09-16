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
    public partial class SitesList
    {
        protected override void OnInitialized()
        {
        }

        IList<SiteViewModel> SelectedSites = new List<SiteViewModel>();
        SiteViewModel? LastSelectedSite
        {
            get
            {
                return SelectedSites.Count > 0 ? SelectedSites[SelectedSites.Count - 1] : null;
            }
        }

        void ShowContextMenuWithItems(MouseEventArgs args)
        {
            if (SelectedSites is not null && SelectedSites.Count == 1)
            {
                ContextMenuService.Open(args, new List<ContextMenuItem> { new ContextMenuItem() { Text = "Edit site", Value = siteAction.Edit }, }, OnMenuItemClick);
            }
        }

        void OnMenuItemClick(MenuItemEventArgs args)
        {
            if (args.Value is siteAction action)
            {
                switch (action)
                {
                    case siteAction.Edit:
                        OnEditSite();
                        break;
                }
            }

            ContextMenuService.Close();
        }

        enum siteAction
        {
            Edit,
        }

        async Task OnEditSite()
        {
            await DialogService.OpenAsync<EditSite>($"Edit site", new Dictionary<string, object>() { { "SiteToEdit", LastSelectedSite }, { "ViewModel", mvm } }, new DialogOptions() { Width = "700px", Height = "600px", Resizable = true, Draggable = true });
            StateHasChanged();
        }

        int SiteUseCount
        {
            get
            {
                return LastSelectedSite.SiteUseCount;
            }
        }
    }
}
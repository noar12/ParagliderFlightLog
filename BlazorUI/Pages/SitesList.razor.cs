using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using Radzen;
using ParagliderFlightLog.ViewModels;

namespace BlazorUI.Pages
{
    public partial class SitesList : IAsyncDisposable
    {
        private const int SITE_MAP_ZOOM_LEVEL = 12;
        private IJSObjectReference? module;
        private object? map;
        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                module = await jsRuntime.InvokeAsync<IJSObjectReference>("import", "./leafletmap.js");
                if (module != null)
                {
                    map = await module.InvokeAsync<IJSObjectReference>("load_map", "map", 0, 0, 2);
                }
            }
        }
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
        async Task UpdateSiteDetails(object arg)
        {
            if (arg is IList<SiteViewModel> sites)
            {
                SelectedSites = sites;
                if (module is not null)
                {
                    map = await module.InvokeAsync<IJSObjectReference>("modify_map",
                                                            map,
                                                            sites[^1].Latitude,
                                                            sites[^1].Longitude,
                                                            SITE_MAP_ZOOM_LEVEL);
                    map = await module.InvokeAsync<IJSObjectReference>("add_marker",
                                                            map,
                                                            sites[^1].Latitude,
                                                            sites[^1].Longitude,
                                                            sites[^1].Name
                    );
                }
            }

        }
        enum siteAction
        {
            Edit,
        }

        async Task OnEditSite()
        {
            await DialogService.OpenAsync<EditSite>($"Edit site", new Dictionary<string, object>() { { "SiteToEdit", LastSelectedSite! }, { "ViewModel", mvm } }, new DialogOptions() { Width = "700px", Height = "600px", Resizable = true, Draggable = true });
            StateHasChanged();
        }

        public async ValueTask DisposeAsync()
        {
            if (module != null)
            {
                try
                {
                    await module.DisposeAsync();
                }
                catch (Exception e)
                {

                    _logger.LogError(e.Message);
                }

            }
        }

    }
}
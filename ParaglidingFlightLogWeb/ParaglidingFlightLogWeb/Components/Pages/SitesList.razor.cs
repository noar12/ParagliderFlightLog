using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using Radzen;
using ParaglidingFlightLogWeb.ViewModels;
using System.Linq;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components;
using ParaglidingFlightLogWeb.Data;

namespace ParaglidingFlightLogWeb.Components.Pages
{
    public partial class SitesList : IAsyncDisposable
    {
        private const int SITE_MAP_ZOOM_LEVEL = 12;
        private IJSObjectReference? module;
        private object? map;
        private DateTime _startDate = DateTime.Today - TimeSpan.FromDays(365);
        private DateTime _endDate = DateTime.Today;

        [CascadingParameter]
        private Task<AuthenticationState> AuthenticationStateTask { get; set; } = null!;
        [Inject]
        UserManager<ApplicationUser> UserManager { get; set; } = null!;
        protected override async Task OnInitializedAsync()
        {
            var userClaim = (await AuthenticationStateTask).User;
            if (userClaim.Identity is not null && userClaim.Identity.IsAuthenticated)
            {
                var currentUser = await UserManager.GetUserAsync(userClaim);
                if (currentUser == null) return;
                string userId = currentUser.Id;
                mvm.Init(userId);
            }
        }

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
                ContextMenuService.Open(args,
                                        new List<ContextMenuItem> { new() { Text = "Edit site", Value = ESiteAction.Edit }, },
                                        OnMenuItemClick);
            }
        }

        void OnMenuItemClick(MenuItemEventArgs args)
        {
            if (args.Value is ESiteAction action)
            {
                switch (action)
                {
                    case ESiteAction.Edit:
                        _ = OnEditSite();
                        break;
                }
            }

            ContextMenuService.Close();
        }
        private async Task UpdateSiteDetails(object arg)
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

        private async Task OnShowSiteTimeRange(){
            List<SiteViewModel> siteToShow = mvm.SiteUsedInTimeRange(_startDate, _endDate);
            foreach (var site in siteToShow.Where(site => module is not null))
            {
                map = await module!.InvokeAsync<IJSObjectReference>("add_marker",
                                                                        map,
                                                                        site.Latitude,
                                                                        site.Longitude,
                                                                        site.Name
                                );
            }
        }
        enum ESiteAction
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
                    module = null;
                    GC.SuppressFinalize(this);
                }
                catch (Exception e)
                {

                    _logger.LogError("{Message}",e.Message);
                }

            }
        }

    }
}
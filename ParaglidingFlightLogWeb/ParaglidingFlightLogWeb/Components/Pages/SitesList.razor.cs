using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using Radzen;
using Radzen.Blazor;
using ParaglidingFlightLogWeb.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components;
using ParaglidingFlightLogWeb.Data;
using ParaglidingFlightLogWeb.Services;

namespace ParaglidingFlightLogWeb.Components.Pages
{
    public partial class SitesList : IAsyncDisposable
    {
        private const int SITE_MAP_ZOOM_LEVEL = 12;
        private IJSObjectReference? module;
        private object? map;
        private DateTime _startDate = DateTime.Today - TimeSpan.FromDays(365);
        private DateTime _endDate = DateTime.Today;

        /// <summary>
        /// Site Id reflecting what site is currently selected or used to access a site directly at page loading
        /// </summary>
        [Parameter]
        public string SiteId { get; set; } = "";

        [Inject] CoreService Core { get; set; } = null!;
        [Inject] ContextMenuService ContextMenuService { get; set; } = null!;
        [Inject] DialogService DialogService { get; set; } = null!;
        [Inject] IJSRuntime JsRuntime { get; set; } = null!;
        [Inject] ILogger<SitesList> Logger { get; set; } = null!;

        [CascadingParameter] private Task<AuthenticationState> AuthenticationStateTask { get; set; } = null!;
        [Inject] UserManager<ApplicationUser> UserManager { get; set; } = null!;
        private RadzenDataGrid<SiteViewModel> _dataGrid = new();
        protected override async Task OnInitializedAsync()
        {
            var userClaim = (await AuthenticationStateTask).User;
            if (userClaim.Identity is not null && userClaim.Identity.IsAuthenticated)
            {
                var currentUser = await UserManager.GetUserAsync(userClaim);
                if (currentUser == null) return;
                string userId = currentUser.Id;
                await Core.Init(userId);
                Logger.LogInformation("Initialized for {User}", currentUser.UserName);
            }
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                module = await JsRuntime.InvokeAsync<IJSObjectReference>("import", "./leafletmap.js");
                if (module != null)
                {
                    map = await module.InvokeAsync<IJSObjectReference>("load_map", "map", 0, 0, 2);
                }
            }

            if (string.IsNullOrEmpty(SiteId))
            {
                return;
            }

            var site = Core.SiteListViewModel.FirstOrDefault(x => x.Site_ID == SiteId);
            if (site is not null)
            {
                await _dataGrid.SelectRow(site);
            }
        }

        IList<SiteViewModel> SelectedSites = [];

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
                    [new() { Text = "Edit site", Value = ESiteAction.Edit }],
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
                    //map = await module.InvokeAsync<IJSObjectReference>("remove_all", map);//this remove even the tile. It's a bit violent...
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

        private async Task OnShowSiteTimeRange()
        {
            List<SiteViewModel> siteToShow = Core.SiteUsedInTimeRange(_startDate, _endDate);
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
            await DialogService.OpenAsync<EditSite>($"Edit site",
                new Dictionary<string, object>() { { "SiteToEdit", LastSelectedSite! }, { "ViewModel", Core } },
                new DialogOptions() { Width = "700px", Height = "600px", Resizable = true, Draggable = false });
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
                    Logger.LogError("{Message}", e.Message);
                }
            }
        }
    }
}
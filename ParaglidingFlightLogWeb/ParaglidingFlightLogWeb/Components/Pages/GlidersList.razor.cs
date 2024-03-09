using global::Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Radzen;
using ParaglidingFlightLogWeb.ViewModels;
using Microsoft.AspNetCore.Identity;
using ParaglidingFlightLogWeb.Data;

namespace ParaglidingFlightLogWeb.Components.Pages
{
    public partial class GlidersList
    {
        IList<GliderViewModel> SelectedGliders = new List<GliderViewModel>();
        GliderViewModel? LastSelectedGlider
        {
            get
            {
                return SelectedGliders.Count > 0 ? SelectedGliders[SelectedGliders.Count - 1] : null;
            }
        }
        [Inject] ContextMenuService ContextMenuService { get; set; } = null!;
        [Inject] DialogService DialogService { get; set; } = null!;
        [Inject] IWebHostEnvironment Environment { get; set; } = null!;
        [Inject] MainViewModel mvm { get; set; } = null!;
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
                await mvm.Init(userId);
            }
        }
        void ShowContextMenuWithItems(MouseEventArgs args)
        {
            ContextMenuService.Open(args, [new() { Text = "Edit glider", Value = EGliderAction.Edit },], OnMenuItemClick);
        }

        void OnMenuItemClick(MenuItemEventArgs args)
        {
            if (args.Value is EGliderAction action)
            {
                switch (action)
                {
                    case EGliderAction.Edit:
                        _ = OnEditGlider();
                        break;
                }
            }

            ContextMenuService.Close();
        }

        enum EGliderAction
        {
            Edit,
        }

        async Task OnEditGlider()
        {
            await DialogService.OpenAsync<EditGlider>($"Edit glider", new Dictionary<string, object>() { { "GliderToEdit", LastSelectedGlider! } }, new DialogOptions() { Width = "700px", Height = "600px", Resizable = true, Draggable = false });
            mvm.EditGlider(LastSelectedGlider!);
            StateHasChanged();
        }

        void OnAddGlider()
        {
            mvm.AddGlider();
        }
    }
}
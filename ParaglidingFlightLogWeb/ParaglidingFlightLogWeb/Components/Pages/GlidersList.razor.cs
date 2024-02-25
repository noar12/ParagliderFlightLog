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
using ParaglidingFlightLogWeb;
using ParaglidingFlightLogWeb.Components.Layout;
using Radzen;
using Radzen.Blazor;
using System;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
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
            await DialogService.OpenAsync<EditGlider>($"Edit glider", new Dictionary<string, object>() { { "GliderToEdit", LastSelectedGlider! } }, new DialogOptions() { Width = "700px", Height = "600px", Resizable = true, Draggable = true });
            mvm.EditGlider(LastSelectedGlider!);
            StateHasChanged();
        }

        void OnAddGlider()
        {
            mvm.AddGlider();
        }
    }
}
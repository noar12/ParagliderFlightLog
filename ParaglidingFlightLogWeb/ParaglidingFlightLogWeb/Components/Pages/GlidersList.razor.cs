using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Radzen;
using ParaglidingFlightLogWeb.ViewModels;
using Microsoft.AspNetCore.Identity;
using ParaglidingFlightLogWeb.Data;
using ParaglidingFlightLogWeb.Services;
using Radzen.Blazor;

namespace ParaglidingFlightLogWeb.Components.Pages;

public partial class GlidersList
{
    private IList<GliderViewModel> SelectedGliders = [];
    private RadzenDataGrid<GliderViewModel>? _grid;
    GliderViewModel? LastSelectedGlider => SelectedGliders.Count > 0 ? SelectedGliders[^1] : null;
    [Inject] private ContextMenuService ContextMenuService { get; set; } = null!;
    [Inject] private DialogService DialogService { get; set; } = null!;
    [Inject] private NotificationService NotifServ { get; set; } = null!;
    [Inject] private CoreService Core { get; set; } = null!;
    [Inject] private ILogger<GlidersList> Logger { get; set; } = null!;
    [CascadingParameter] private Task<AuthenticationState> AuthenticationStateTask { get; set; } = null!;
    [Inject] UserManager<ApplicationUser> UserManager { get; set; } = null!;

    /// <inheritdoc />
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

    private async Task OnEditGlider(GliderViewModel? glider)
    {
        if (glider == null) return;
        await DialogService.OpenAsync<EditGlider>($"Edit glider",
            new Dictionary<string, object>() { { "GliderToEdit", glider } },
            new DialogOptions() { Width = "700px", Height = "600px", Resizable = true, Draggable = false });
        Core.EditGlider(glider);
        await InvokeAsync(StateHasChanged);
    }

    private async Task OnAddGlider()
    {
        var glider = await Core.AddGliderAsync();
        SelectedGliders.Clear();
        SelectedGliders.Add(glider);
        if (_grid is not null)
        {
            await _grid.Reload();
        }
        await InvokeAsync(StateHasChanged);
    }

    private async Task OnDeleteGlider(GliderViewModel? glider)
    {
        if (glider is null) { return; }

        if (Core.FlightListViewModel.Any(x => x.Glider?.GliderId == glider.GliderId))
        {
            NotifServ.Notify(NotificationSeverity.Error, "Cannot delete glider",
                "This glider is used in one or more flights. Please remove it from the flights before deleting it.",
                5000);
            return;
        }

        await Core.DeleteGliderAsync(glider);
        SelectedGliders.Remove(glider);
        if (_grid is not null)
        {
            await _grid.Reload();
        }
        await InvokeAsync(StateHasChanged);
        
    }
}
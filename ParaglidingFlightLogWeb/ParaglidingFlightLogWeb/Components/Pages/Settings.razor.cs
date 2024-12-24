using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Identity;
using ParagliderFlightLog.Services;
using ParaglidingFlightLogWeb.Data;
using ParaglidingFlightLogWeb.Services;
using System.Security.Claims;

namespace ParaglidingFlightLogWeb.Components.Pages;

public sealed partial class Settings : IDisposable
{
    private List<string>? _adminsId = [];
    private bool _isAdmin;
    private IDisposable? _disposable;
    private int _xcScoreQueueCount;
    private ClaimsPrincipal? _userClaim;
    [Inject] private IConfiguration Config { get; set; } = null!;
    [Inject] private CoreService CoreService { get; set; } = null!;
    [Inject] private XcScoreManagerData XcScoreManagerData { get; set; } = null!;
    [Inject] UserManager<ApplicationUser> UserManager { get; set; } = null!;
    [CascadingParameter] private Task<AuthenticationState> AuthenticationStateTask { get; set; } = null!;
    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    /// <returns></returns>
    protected override async Task OnInitializedAsync()
    {
        _adminsId = Config.GetSection("Administration:Admins").Get<List<string>>();
        _userClaim = (await AuthenticationStateTask).User;
        if (_userClaim.Identity?.Name is null)
        {
            _isAdmin = false;
        }
        else
        {
            _isAdmin = _adminsId?.Contains(_userClaim.Identity.Name) ?? false;
        }

        if (_isAdmin && _disposable is null)
        {
            _disposable = XcScoreManagerData.FlightToProcess.Subscribe(async count => await XcScoreQueueHasChanged(count));
        }
    }

    private async Task XcScoreQueueHasChanged(int xcScoreQueueCount)
    {
        _xcScoreQueueCount = xcScoreQueueCount;
        await InvokeAsync(StateHasChanged);
    }

    private bool HasUserRequestEnqueued()
    {
        if (_userClaim is null) { return false; }

        string? userId = UserManager.GetUserId(_userClaim);
        if (userId is null) { return false; }
        return XcScoreManagerData.IsThereUserFlightEnqueue(userId);
    }

    /// <summary>
    /// Dispose the page
    /// </summary>
    public void Dispose() {
        _disposable?.Dispose();
    }

    private void OnCalculateMissingFileClick(MouseEventArgs obj)
    {
        CoreService.EnqueueAllUserFlightForScore();
        StateHasChanged();
    }
}

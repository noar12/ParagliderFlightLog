using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using ParagliderFlightLog.Services;
using ParaglidingFlightLogWeb.Services;

namespace ParaglidingFlightLogWeb.Components.Pages;

public sealed partial class Settings : IDisposable
{
    private List<string>? _adminsId = [];
    private bool _isAdmin;
    private IDisposable? _disposable;
    private int _xcScoreQueueCount;
    [Inject] private IConfiguration Config { get; set; } = null!;
    [Inject] private CoreService CoreService { get; set; } = null!;
    [Inject] private XcScoreManagerData XcScoreManagerData { get; set; } = null!;
    [CascadingParameter] private Task<AuthenticationState> AuthenticationStateTask { get; set; } = null!;
    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    /// <returns></returns>
    protected override async Task OnInitializedAsync()
    {
        _adminsId = Config.GetSection("Administration:Admins").Get<List<string>>();
        var userClaim = (await AuthenticationStateTask).User;
        if (userClaim.Identity?.Name is null)
        {
            _isAdmin = false;
        }
        else
        {
            _isAdmin = _adminsId?.Contains(userClaim.Identity.Name) ?? false;
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

    /// <summary>
    /// Dispose the page
    /// </summary>
    public void Dispose() {
        _disposable?.Dispose();
    }
    
}

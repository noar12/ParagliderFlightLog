using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Web;
using Radzen;
using Radzen.Blazor;
using ParaglidingFlightLogWeb.ViewModels;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using ParaglidingFlightLogWeb.Data;
using System.Diagnostics;
using ParaglidingFlightLogWeb.Services;
using Microsoft.JSInterop;
using Microsoft.Extensions.Caching.Memory;

namespace ParaglidingFlightLogWeb.Components.Pages;

public partial class Index
{
    [CascadingParameter]
    private Task<AuthenticationState> AuthenticationStateTask { get; set; } = null!;
    [Inject]
    UserManager<ApplicationUser> UserManager { get; set; } = null!;
    [Inject] FlightStatisticService FlightStatisticService { get; set; } = null!;
    [Inject] CoreService CoreService { get; set; } = null!;
    string? userId;


    FlightViewModel? _flightToRemember = null;
    private List<FlightViewModel>? _thisYearTopScorers = null;
    private List<FlightViewModel>? _thisYearLongestFlights = null;
    private List<FlightViewModel>? _topScorers = null;
    private List<FlightViewModel>? _longestFlights = null;
    private List<SiteViewModel>? _sitesToReturnTo = null;
    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    /// <returns></returns>
    protected override async Task OnInitializedAsync()
    {
        var sw = Stopwatch.StartNew();
        _logger.LogInformation("Starting initialization of index page");

        var userClaim = (await AuthenticationStateTask).User;
        if (userClaim.Identity is not null && userClaim.Identity.IsAuthenticated)
        {
            var currentUser = await UserManager.GetUserAsync(userClaim);
            if (currentUser == null) return;
            userId = currentUser.Id;
            await CoreService.Init(userId);


            _logger.LogInformation("Index page intialzed for {user} in {time_ms} ms", currentUser.UserName, sw.ElapsedMilliseconds);
        }
    }
    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    /// <param name="firstRender"></param>
    /// <returns></returns>
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        _logger.LogDebug("Rendering index page");
        if (firstRender)
        {
            await Task.Run(() => _flightToRemember = CoreService.GetFlightToRemember());
            await Task.Run(() => _thisYearTopScorers = FlightStatisticService.TopScorer(DateTime.Now.Year).ToList());
            await Task.Run(() => _thisYearLongestFlights = FlightStatisticService.TopLongestFlight(DateTime.Now.Year).ToList());
            //await Task.Run(() => _thisYearHighestFlights = FlightStatisticService.TopHighestFlight(DateTime.Now.Year).ToList());//
            await Task.Run(() => _topScorers = FlightStatisticService.TopScorer().ToList());
            await Task.Run(() => _longestFlights = FlightStatisticService.TopLongestFlight().ToList());
            //await Task.Run(() => _highestFlights = FlightStatisticService.TopHighestFlight().ToList());// this take too much time because height is not store in the db
            await Task.Run(() => _sitesToReturnTo = CoreService.GetRandomSitesToReturnTo(TimeSpan.FromDays(365.25)));
            StateHasChanged(); // that's ugly but it is the only way I found to display something to the user before every thing is set
            // OnInitialized is execute twice (once before connecting the SignalR and the page is not rendering at this point) and another time after.
        }

    }
}
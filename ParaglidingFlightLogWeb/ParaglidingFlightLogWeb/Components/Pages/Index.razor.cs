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

namespace ParaglidingFlightLogWeb.Components.Pages;

public partial class Index
{
    [CascadingParameter]
    private Task<AuthenticationState> AuthenticationStateTask { get; set; } = null!;
    [Inject]
    UserManager<ApplicationUser> UserManager { get; set; } = null!;
    [Inject] FlightStatisticService FlightStatisticService { get; set; } = null!;
    [Inject] CoreService CoreService { get; set; } = null!;
    string? userName;
    string? userId;
    string? userEmail;


    FlightViewModel? _flightToRemember;
    protected override async Task OnInitializedAsync()
    {
        var sw = Stopwatch.StartNew();
        _logger.LogInformation("Starting initialization of index page");

        var userClaim = (await AuthenticationStateTask).User;
        if (userClaim.Identity is not null && userClaim.Identity.IsAuthenticated)
        {
            var currentUser = await UserManager.GetUserAsync(userClaim);
            if (currentUser == null) return;
            userName = currentUser.UserName;
            userId = currentUser.Id;
            userEmail = currentUser.Email;
            _logger.LogInformation("Index page intialzed for {user} in {time_ms} ms", currentUser.UserName, sw.ElapsedMilliseconds);
            await CoreService.Init(userId);
            _flightToRemember = CoreService.GetFlightToRemember();
        }
    }
}
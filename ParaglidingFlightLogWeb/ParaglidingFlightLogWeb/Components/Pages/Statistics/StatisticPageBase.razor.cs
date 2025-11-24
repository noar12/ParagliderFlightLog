using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using ParaglidingFlightLogWeb.Data;
using ParaglidingFlightLogWeb.Services;

namespace ParaglidingFlightLogWeb.Components.Pages.Statistics;

public abstract partial class StatisticPageBase : ComponentBase
{
    /// <summary>
    /// Core service to access flight data.
    /// </summary>
    [Inject] protected CoreService Core { get; set; } = null!;
    /// <summary>
    /// Flight statistic service.
    /// </summary>
    [Inject] protected FlightStatisticService FlightStatistic { get; set; } = null!;
    /// <summary>
    /// Authentication state task.
    /// </summary>
    [CascadingParameter] protected Task<AuthenticationState> AuthenticationStateTask { get; set; } = null!;
    /// <summary>
    /// User manager service.
    /// </summary>
    [Inject] protected UserManager<ApplicationUser> UserManager { get; set; } = null!;
    /// <summary>
    /// Logger
    /// </summary>
    [Inject] protected ILogger<StatisticPageBase> Logger { get; set; } = null!;
    /// <summary>
    /// Prepare the statistic results.
    /// </summary>
    /// <returns></returns>
    protected abstract Task Analyze(); 
    
    /// <summary>
    /// <inheritdoc/>
    /// </summary>
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
            if (Core.FlightListViewModel.Count == 0)
                return;
            await Analyze();
        }
        
    }
    /// <summary>
    /// Class to display data
    /// </summary>
    protected class YearMonthlyStatistic
    {
        /// <summary>
        /// 
        /// </summary>
        public MonthlyItem[] MonthlyItems { get; init; } = [];
    }
    /// <summary>
    /// Class to display data
    /// </summary>
    protected class MonthlyItem
    {
        /// <summary>
        /// 
        /// </summary>
        public string Month { get; set; } = string.Empty;
        /// <summary>
        /// 
        /// </summary>
        public double Value { get; set; }
    }
    /// <summary>
    /// Class to represent a duration item for histogram display.
    /// </summary>
    /// <param name="histData"></param>
    /// <returns></returns>
    protected static DurationItem[] HistDataToDurationItem(HistData histData)
    {
        var durationItems = new List<DurationItem>();
        for (int i = 0; i < histData.Counts.Length; ++i)
        {
            durationItems.Add(new DurationItem() { BarValue = histData.Counts[i], BarLocation = histData.BinEdges[i], });
        }

        return [.. durationItems];
    }
    /// <summary>
    /// Class to represent a duration item for histogram display.
    /// </summary>
    protected class DurationItem
    {
        /// <summary>
        /// 
        /// </summary>
        public double BarLocation { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public double BarValue { get; set; }
    }
}
using Radzen;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Identity;
using ParaglidingFlightLogWeb.Data;
using ParaglidingFlightLogWeb.Services;

namespace ParaglidingFlightLogWeb.Components.Pages;

public partial class FlightsStatistic
{
    [Inject] IWebHostEnvironment Environment { get; set; } = null!;
    [Inject] CoreService Core { get; set; } = null!;
    [Inject] FlightStatisticService FlightStatistic { get; set; } = null!;
    [CascadingParameter] private Task<AuthenticationState> AuthenticationStateTask { get; set; } = null!;
    [Inject] UserManager<ApplicationUser> UserManager { get; set; } = null!;
    [Inject] ILogger<FlightsStatistic> Logger { get; set; } = null!;
    protected override async Task OnInitializedAsync()
    {
        var userClaim = (await AuthenticationStateTask).User;
        if (userClaim.Identity is not null && userClaim.Identity.IsAuthenticated)
        {
            var currentUser = await UserManager.GetUserAsync(userClaim);
            if (currentUser == null) return;
            string userId = currentUser.Id;
            await Core.Init(userId);
            _yearToAnalyse = DateTime.Now.Year;
            Logger.LogInformation("Initialized for {User}", currentUser.UserName);
            if (FlightStatistic.FlightsCount == 0)
                return;
            OnAnalyze();
            
        }
        
    }

    private bool _allYearAnalysis = false;
    private readonly Variant variant = Variant.Outlined;
    StatisticalFlightsAnalysis AnalysisToDo = StatisticalFlightsAnalysis.DurationDistribution;
    int _yearToAnalyse;
    void OnAnalyze()
    {
        string[] l_MonthList =
        [
            "January",
            "February",
            "March",
            "April",
            "Mai",
            "June",
            "July",
            "August",
            "Septempber",
            "October",
            "November",
            "December"
        ];
        string[] l_YearsText = new string[Core.YearsOfFlying.Count];
        int i = 0;
        switch (AnalysisToDo)
        {
            case StatisticalFlightsAnalysis.MontlyMedian:
                MonthlyMedianAnalysisResult = new YearMonthlyStatistic[Core.YearsOfFlying.Count];
                foreach (int l_FlightYear in Core.YearsOfFlying)
                {
                    double[] l_MonthlyMedians = FlightStatistic.GetMonthlyMedian(l_FlightYear);
                    MonthlyItem[] currentYearMonthlyMedian = new MonthlyItem[l_MonthList.Length];
                    int j = 0;
                    foreach (double monthMedian in l_MonthlyMedians)
                    {
                        currentYearMonthlyMedian[j] = new MonthlyItem()
                        {
                            Value = monthMedian,
                            Month = l_MonthList[j],
                        };
                        j++;
                    }

                    MonthlyMedianAnalysisResult[i] = new YearMonthlyStatistic()
                    {
                        MonthlyItems = currentYearMonthlyMedian,
                    };
                    l_YearsText[i] = l_FlightYear.ToString();
                    i++;
                }

                break;
            case StatisticalFlightsAnalysis.MonthlyFlightDuration:
                MonthlyDurationAnalysisResult = new YearMonthlyStatistic[Core.YearsOfFlying.Count];
                foreach (int l_FlightYear in Core.YearsOfFlying)
                {
                    double[] l_MonthlyDuration = FlightStatistic.GetMonthlyFlightHours(l_FlightYear);
                    MonthlyItem[] currentYearMonthlyDuration = new MonthlyItem[l_MonthList.Length];
                    int j = 0;
                    foreach (double monthDuration in l_MonthlyDuration)
                    {
                        currentYearMonthlyDuration[j] = new MonthlyItem()
                        {
                            Value = monthDuration,
                            Month = l_MonthList[j],
                        };
                        j++;
                    }

                    MonthlyDurationAnalysisResult[i] = new YearMonthlyStatistic()
                    {
                        MonthlyItems = currentYearMonthlyDuration,
                    };
                    l_YearsText[i] = l_FlightYear.ToString();
                    ++i;
                }

                break;
            case StatisticalFlightsAnalysis.DurationDistribution:
                HistData? histData;
                if (_allYearAnalysis)
                {
                    histData = FlightStatistic.GetFlightDurationRepartition(DateTime.MinValue, DateTime.MaxValue);
                }
                else
                {
                    histData = FlightStatistic.GetFlightDurationRepartition(new DateTime(_yearToAnalyse, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                                                                     new DateTime(_yearToAnalyse, 12, 31, 0, 0, 0, DateTimeKind.Utc));
                }
                if (histData is null) { return; }
                DurationAnalysisResult = HistDataToDurationItem(histData);
                break;
            case StatisticalFlightsAnalysis.MonthlyCumulatedFlightDuration:
                MonthlyCumulatedDurationAnalysisResult = new YearMonthlyStatistic[Core.YearsOfFlying.Count];
                foreach (int year in Core.YearsOfFlying)
                {
                    double[] monthlyCumulatedDuration = FlightStatistic.GetCumulatedFlightHoursPerMonth(year);
                    MonthlyCumulatedDurationAnalysisResult[i] = new() { MonthlyItems = new MonthlyItem[l_MonthList.Length] };
                    for (int j = 0; j < monthlyCumulatedDuration.Length; j++)
                    {
                        MonthlyCumulatedDurationAnalysisResult[i].MonthlyItems[j] = new()
                        {
                            Month = l_MonthList[j],
                            Value = monthlyCumulatedDuration[j]
                        };
                    }
                    ++i;
                }
                break;
            default:
                break;
        }
    }

    static string GetAnalyseName(StatisticalFlightsAnalysis analyse)
    {
        return analyse switch
        {
            StatisticalFlightsAnalysis.MontlyMedian => "Monthly median",
            StatisticalFlightsAnalysis.DurationDistribution => "Duration Distribution",
            StatisticalFlightsAnalysis.MonthlyFlightDuration => "Monthly flight duration",
            StatisticalFlightsAnalysis.MonthlyCumulatedFlightDuration => "Monthly cumulated flight duration",
            _ => "",
        };
    }

    class DurationItem
    {
        public double BarLocation { get; set; }
        public double BarValue { get; set; }
    }

    class MonthlyItem
    {
        public string Month { get; set; } = string.Empty;
        public double Value { get; set; }
    }

    class YearMonthlyStatistic
    {
        public MonthlyItem[] MonthlyItems { get; set; } = [];
    }

    DurationItem[] DurationAnalysisResult = [];
    YearMonthlyStatistic[] MonthlyMedianAnalysisResult = [];
    YearMonthlyStatistic[] MonthlyDurationAnalysisResult = [];
    YearMonthlyStatistic[] MonthlyCumulatedDurationAnalysisResult = [];
    static DurationItem[] HistDataToDurationItem(HistData histData)
    {
        var durationItems = new List<DurationItem>();
        for (int i = 0; i < histData.Counts.Length; ++i)
        {
            durationItems.Add(new DurationItem() { BarValue = histData.Counts[i], BarLocation = histData.BinEdges[i], });
        }

        return [.. durationItems];
    }
}
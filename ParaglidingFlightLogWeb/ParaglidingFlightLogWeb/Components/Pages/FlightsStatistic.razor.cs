using Radzen;
using ParaglidingFlightLogWeb.ViewModels;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Identity;
using ParaglidingFlightLogWeb.Data;

namespace ParaglidingFlightLogWeb.Components.Pages;

public partial class FlightsStatistic
{

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
            YearToAnalyse = DateTime.Now.Year;
            if (fsvm.FlightsCount == 0)
                return;
            DurationAnalysisResult = HistDataToDurationItem(fsvm.FlightsDurationHistData);
        }
    }
    private readonly Variant variant = Variant.Outlined;
#pragma warning disable IDE0044 // Add readonly modifier. this wrong because we change it in the interface
    StatisticalFlightsAnalysis AnalysisToDo= StatisticalFlightsAnalysis.DurationDistribution;
#pragma warning restore IDE0044 // Add readonly modifier
    int YearToAnalyse;
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
        string[] l_YearsText = new string[mvm.YearsOfFlying.Count];
        int i = 0;
        switch (AnalysisToDo)
        {
            case StatisticalFlightsAnalysis.MontlyMedian:
                MonthlyMedianAnalysisResult = new YearMonthlyStatistic[mvm.YearsOfFlying.Count];
                foreach (int l_FlightYear in mvm.YearsOfFlying)
                {
                    double[] l_MonthlyMedians = fsvm.GetMonthlyMedian(l_FlightYear);
                    MonthlyItem[] currentYearMonthlyMedian = new MonthlyItem[l_MonthList.Length];
                    int j = 0;
                    foreach (double monthMedian in l_MonthlyMedians)
                    {
                        currentYearMonthlyMedian[j] = new MonthlyItem()
                        {
                            BarValue = monthMedian,
                            Month = l_MonthList[j],
                        };
                        j++;
                    }

                    MonthlyMedianAnalysisResult[i] = new YearMonthlyStatistic()
                    {
                        MonthlyMedianItems = currentYearMonthlyMedian,
                    };
                    l_YearsText[i] = l_FlightYear.ToString();
                    i++;
                }

                break;
            case StatisticalFlightsAnalysis.MonthlyFlightDuration:
                MonthlyDurationAnalysisResult = new YearMonthlyStatistic[mvm.YearsOfFlying.Count];
                foreach (int l_FlightYear in mvm.YearsOfFlying)
                {
                    double[] l_MonthlyDuration = fsvm.GetMonthlyFlightHours(l_FlightYear);
                    MonthlyItem[] currentYearMonthlyDuration = new MonthlyItem[l_MonthList.Length];
                    int j = 0;
                    foreach (double monthDuration in l_MonthlyDuration)
                    {
                        currentYearMonthlyDuration[j] = new MonthlyItem()
                        {
                            BarValue = monthDuration,
                            Month = l_MonthList[j],
                        };
                        j++;
                    }

                    MonthlyDurationAnalysisResult[i] = new YearMonthlyStatistic()
                    {
                        MonthlyMedianItems = currentYearMonthlyDuration,
                    };
                    l_YearsText[i] = l_FlightYear.ToString();
                    ++i;
                }

                break;
            case StatisticalFlightsAnalysis.DurationDistribution:
                fsvm = new FlightsStatisticsViewModel(mvm, new DateTime(YearToAnalyse, 1, 1,0,0,0,DateTimeKind.Utc), new DateTime(YearToAnalyse, 12, 31, 0, 0, 0, DateTimeKind.Utc));
                if (fsvm.FlightsCount == 0)
                    return;
                DurationAnalysisResult = HistDataToDurationItem(fsvm.FlightsDurationHistData);
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
        public double BarValue { get; set; }
    }

    class YearMonthlyStatistic
    {
        public MonthlyItem[] MonthlyMedianItems { get; set; } = [];
    }

    DurationItem[] DurationAnalysisResult = [];
    YearMonthlyStatistic[] MonthlyMedianAnalysisResult = [];
    YearMonthlyStatistic[] MonthlyDurationAnalysisResult = [];
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
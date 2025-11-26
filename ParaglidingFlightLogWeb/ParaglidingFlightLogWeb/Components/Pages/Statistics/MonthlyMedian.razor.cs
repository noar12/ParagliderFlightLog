namespace ParaglidingFlightLogWeb.Components.Pages.Statistics;

public partial class MonthlyMedian : StatisticPageBase
{
    private YearMonthlyStatistic[] _monthlyMedianAnalysisResult = [];
    private string[] _yearsText = [];
    private bool _analyzed;

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    /// <returns></returns>
    protected override Task Analyze()
    {
        _monthlyMedianAnalysisResult = new YearMonthlyStatistic[Core.YearsOfFlying.Count];
        _yearsText = new string[Core.YearsOfFlying.Count];

        int i = 0;
        foreach (int flightYear in Core.YearsOfFlying)
        {
            double[] monthlyMedians = FlightStatistic.GetMonthlyMedian(flightYear);
            MonthlyItem[] currentYearMonthlyMedian = new MonthlyItem[FlightStatistic.MonthList.Length];
            int j = 0;
            foreach (double monthMedian in monthlyMedians)
            {
                currentYearMonthlyMedian[j] = new MonthlyItem()
                {
                    Value = monthMedian, Month = FlightStatistic.MonthList[j],
                };
                j++;
            }

            _monthlyMedianAnalysisResult[i] = new YearMonthlyStatistic() { MonthlyItems = currentYearMonthlyMedian, };
            _yearsText[i] = flightYear.ToString();
            i++;
        }

        _analyzed = true;
        return Task.CompletedTask;
    }
}
namespace ParaglidingFlightLogWeb.Components.Pages.Statistics;

public partial class MonthlyDuration : StatisticPageBase
{
    private YearMonthlyStatistic[] _monthlyDurationAnalysisResult = [];
    private string[] _yearsText = [];
    private bool _analyzed;
    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    /// <returns></returns>
    protected override Task Analyze()
    {
        _yearsText = new string[Core.YearsOfFlying.Count];
        _monthlyDurationAnalysisResult = new YearMonthlyStatistic[Core.YearsOfFlying.Count];
        int i = 0;
        foreach (int flightYear in Core.YearsOfFlying)
        {
            double[] monthlyDuration = FlightStatistic.GetMonthlyFlightHours(flightYear);
            MonthlyItem[] currentYearMonthlyDuration = new MonthlyItem[FlightStatistic.MonthList.Length];
            int j = 0;
            foreach (double monthDuration in monthlyDuration)
            {
                currentYearMonthlyDuration[j] = new MonthlyItem()
                {
                    Value = monthDuration,
                    Month = FlightStatistic.MonthList[j],
                };
                j++;
            }

            _monthlyDurationAnalysisResult[i] = new YearMonthlyStatistic()
            {
                MonthlyItems = currentYearMonthlyDuration,
            };
            _yearsText[i] = flightYear.ToString();
            ++i;
        }

        _analyzed = true;
        return Task.CompletedTask;
    }
}
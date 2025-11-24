namespace ParaglidingFlightLogWeb.Components.Pages.Statistics;

public partial class MonthlyCumulatedDuration : StatisticPageBase
{
    private YearMonthlyStatistic[] MonthlyCumulatedDurationAnalysisResult = [];
    private bool _analyzed;


    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    protected override async Task Analyze()
    {
        MonthlyCumulatedDurationAnalysisResult = new YearMonthlyStatistic[Core.YearsOfFlying.Count];
        int i = 0;
        foreach (int year in Core.YearsOfFlying)
        {
            double[] monthlyCumulatedDuration = FlightStatistic.GetCumulatedFlightHoursPerMonth(year);
            MonthlyCumulatedDurationAnalysisResult[i] =
                new() { MonthlyItems = new MonthlyItem[FlightStatistic.MonthList.Length] };
            for (int j = 0; j < monthlyCumulatedDuration.Length; j++)
            {
                MonthlyCumulatedDurationAnalysisResult[i].MonthlyItems[j] = new()
                {
                    Month = FlightStatistic.MonthList[j], Value = monthlyCumulatedDuration[j]
                };
            }

            ++i;
        }

        _analyzed = true;
        await InvokeAsync(StateHasChanged);
    }
}
namespace ParaglidingFlightLogWeb.Components.Pages.Statistics;

public partial class ObjectiveSyntesis : StatisticPageBase
{
    private bool _allYearAnalysis;
    private int _yearToAnalyse = DateTime.UtcNow.Year;
    private ObjectiveItem[] _pieChartData = [];
    private bool _analyzed;

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    protected override Task Analyze()
    {
        string[] objectivesTxt = Core.GetAvailableFlightObjectives();
        _pieChartData = new ObjectiveItem[objectivesTxt.Length];
        int i = 0;

        DateTime analysisStart = _allYearAnalysis ? new DateTime(1900, 1, 1) : new DateTime(_yearToAnalyse, 1, 1);
        DateTime analysisEnd = _allYearAnalysis ? DateTime.Now : new DateTime(_yearToAnalyse, 12, 31, 23, 59, 59);

        foreach (string o in objectivesTxt)
        {
            _pieChartData[i] = new ObjectiveItem()
            {
                ObjectiveType = o,
                Hours = FlightStatistic.GetHoursFlownForObjective(o,analysisStart, analysisEnd).TotalHours
            };
            i++;
        }
        _analyzed = true;
        return Task.CompletedTask;
    }
}

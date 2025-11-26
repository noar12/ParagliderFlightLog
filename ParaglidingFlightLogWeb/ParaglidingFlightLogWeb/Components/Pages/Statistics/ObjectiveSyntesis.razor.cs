namespace ParaglidingFlightLogWeb.Components.Pages.Statistics;

public partial class ObjectiveSyntesis : StatisticPageBase
{
    private bool _allYearAnalysis;
    private int _yearToAnalyse = DateTime.UtcNow.Year;
    private ObjectiveItem[] _pieChartData = [];
    private bool _analyzed;

    private string _localHoursTxt = "";
    private string _xcHoursTxt = "";
    private string _glideHoursTxt = "";

    private double _xcTotalDistance = 0.0;
    private double _xcAverageSpeed = 0.0;

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
            var hoursFlownOnObjective = FlightStatistic.GetHoursFlownForObjective(o, analysisStart, analysisEnd);
            _pieChartData[i] = new ObjectiveItem()
            {
                ObjectiveType = o,
                Hours = hoursFlownOnObjective.TotalHours
            };

            if (o == "Local")
            {
                _localHoursTxt = $"{(int)hoursFlownOnObjective.TotalHours:D2}:{hoursFlownOnObjective.Minutes:D2}";
            }
            else if (o == "XC")
            {
                _xcHoursTxt = $"{(int)hoursFlownOnObjective.TotalHours:D2}:{hoursFlownOnObjective.Minutes:D2}";
                _xcTotalDistance = FlightStatistic.GetTotalXcDistance(analysisStart, analysisEnd);
                _xcAverageSpeed = FlightStatistic.GetAverageXcSpeed(analysisStart, analysisEnd);
            }
            else if (o == "Glide")
            {
                _glideHoursTxt = $"{(int)hoursFlownOnObjective.TotalHours:D2}:{hoursFlownOnObjective.Minutes:D2}";
            }

            i++;

        }



        _analyzed = true;
        return Task.CompletedTask;
    }
    private void OnNextClick()
    {
        _yearToAnalyse++;
        Analyze();
    }
    private void OnPreviousClick()
    {
        _yearToAnalyse--;
        Analyze();
    }
    private void OnAllYearCheckBoxChange(bool newValue)
    {
        _allYearAnalysis = newValue;
        Analyze();
    }
}

using ParaglidingFlightLogWeb.Services;

namespace ParaglidingFlightLogWeb.Components.Pages.Statistics;

public partial class DurationDistribution : StatisticPageBase
{
    private bool _allYearAnalysis;
    private int _yearToAnalyse;

    private DurationItem[] DurationAnalysisResult = [];
    private bool _isLittleScreen;
    private string _chartSytle;



    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    /// <returns></returns>
    protected override Task OnInitializedAsync()
    {
        _yearToAnalyse = DateTime.Today.Year;
        return base.OnInitializedAsync();
    }
    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    /// <returns></returns>
    protected override Task Analyze()
    {
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
        if (histData is null) { return Task.CompletedTask; }
        DurationAnalysisResult = HistDataToDurationItem(histData);
        return Task.CompletedTask;
    }
    private void OnMobileChange(bool matches)
    {
        _isLittleScreen = matches;
        _chartSytle = _isLittleScreen ? "height: calc(100vh - 350px);min-width: 1200px;" : "height: calc(100vh - 180px);min-width: 1200px;";
        StateHasChanged();
    }
}

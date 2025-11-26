using ParaglidingFlightLogWeb.Services;

namespace ParaglidingFlightLogWeb.Components.Pages.Statistics
{
    public partial class XcScoreOverTheYears : StatisticPageBase
    {
        private XcScoreOverTheYearsData[] _xcScoresOverTheYears = [];
        private bool _analyzed;

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <returns></returns>
        protected override Task Analyze()
        {
            _xcScoresOverTheYears = FlightStatistic.GetXcScoresOverTheYears();
            _analyzed = true;
            return Task.CompletedTask;
        }
    }
}
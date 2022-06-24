using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfUI.ViewModels
{
    public class FlightsStatisticsViewModel
    {
        public TimeSpan FlightsDuration { get; private set; }
        public TimeSpan MeanFlightsDuration { get; private set; }
        public TimeSpan MedianFlightsDuration { get; private set; }
        public List<TimeSpan> FlightsDurationsList { get; private set; }
        public int FlightsCount { get; private set; }

        public FlightsStatisticsViewModel(MainViewModel mainViewModel, DateTime AnalyzeStart, DateTime AnalyzeEnd)
        {
            List<FlightViewModel>l_AnalyzeFlights = mainViewModel.FlightsInPeriod(AnalyzeStart, AnalyzeEnd);
            FlightsDuration = mainViewModel.FlightDurationInPeriod(AnalyzeStart, AnalyzeEnd);
            MeanFlightsDuration = FlightsDuration / l_AnalyzeFlights.Count;
            MedianFlightsDuration = l_AnalyzeFlights.OrderBy(flight => flight.FlightDuration)
                .ToList()[l_AnalyzeFlights.Count / 2].FlightDuration;
            FlightsCount = l_AnalyzeFlights.Count;
            FlightsDurationsList = new List<TimeSpan>();
        }
    }
}

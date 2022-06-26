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
        public int FlightsCount { get; private set; }
        public HistData FlightsDurationHistData { get; private set; }

        public FlightsStatisticsViewModel(MainViewModel mainViewModel, DateTime AnalyzeStart, DateTime AnalyzeEnd)
        {
            // get the flights we have to put in the statistical analysis
            List<FlightViewModel> l_AnalyzeFlights = mainViewModel.FlightsInPeriod(AnalyzeStart, AnalyzeEnd);
            
            FlightsDuration = mainViewModel.FlightDurationInPeriod(AnalyzeStart, AnalyzeEnd);
            MeanFlightsDuration = FlightsDuration / l_AnalyzeFlights.Count;
            MedianFlightsDuration = l_AnalyzeFlights.OrderBy(flight => flight.FlightDuration)
                .ToList()[l_AnalyzeFlights.Count / 2].FlightDuration;
            FlightsCount = l_AnalyzeFlights.Count;
            List<double> l_flightsDurationsList = l_AnalyzeFlights.Select(f => f.FlightDuration.TotalHours).ToList();

            (double[] counts, double[] binEdges) = ScottPlot.Statistics.Common.Histogram(l_flightsDurationsList.ToArray(), 20);
            FlightsDurationHistData = new HistData(counts, binEdges.Take(binEdges.Length - 1).ToArray());

            


        }

    }

    public class HistData
    {
        public double[] Counts { get; private set; }
        public double[] BinEdges { get; private set; }
        public HistData(double[] counts, double[] binEdges)
        {
            Counts = counts;
            BinEdges = binEdges;
        }
    }
}

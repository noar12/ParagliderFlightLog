using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ParagliderFlightLog.DataModels;


namespace WpfUI.ViewModels
{
    public class FlightsStatisticsViewModel
    {
        private MainViewModel m_MainViewModel;
        public TimeSpan FlightsDuration { get; private set; }
        public string FlightsDurationText { get { return $"{(int)FlightsDuration.TotalHours}:{FlightsDuration.Minutes}"; } }
        public TimeSpan MeanFlightsDuration { get; private set; }
        public string MeanFlightsDurationText { get { return $"{(int)MeanFlightsDuration.TotalHours}:{MeanFlightsDuration.Minutes}"; } }
        public TimeSpan MedianFlightsDuration { get; private set; }
        public string MedianFlightDurationText { get { return $"{(int)MedianFlightsDuration.TotalHours}:{MedianFlightsDuration.Minutes}"; } }
        public int FlightsCount { get; private set; }
        public HistData FlightsDurationHistData { get; private set; }

        public FlightsStatisticsViewModel(MainViewModel mainViewModel, DateTime AnalyzeStart, DateTime AnalyzeEnd)
        {
            m_MainViewModel = mainViewModel;
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

        public double[] GetMonthlyMedian(int l_FlightYear)
        {
            double[] l_MonthMedian = new double[12];
            for (int month = 1; month <= 12; month++)
            {
                FlightViewModel[]? l_MonthFlights = m_MainViewModel.FlightListViewModel
                .Where(f => f.TakeOffDateTime.Year == l_FlightYear && f.TakeOffDateTime.Month == month).OrderBy(f => f.FlightDuration).ToArray();
                if (l_MonthFlights.Length != 0)
                {
                    int i = month -1;
                    l_MonthMedian[i] = l_MonthFlights[l_MonthFlights.Count() / 2].FlightDuration.TotalHours;
                }

            }
            return l_MonthMedian;
            
           
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

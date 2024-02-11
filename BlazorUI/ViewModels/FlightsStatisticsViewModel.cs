using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ParagliderFlightLog.Models;


namespace ParagliderFlightLog.ViewModels
{
    public class FlightsStatisticsViewModel
    {
        private readonly MainViewModel m_MainViewModel;
        public TimeSpan FlightsDuration { get; private set; }
        public string FlightsDurationText { get { return $"{(int)FlightsDuration.TotalHours}:{FlightsDuration.Minutes}"; } }
        public TimeSpan MeanFlightsDuration { get; private set; }
        public string MeanFlightsDurationText { get { return $"{(int)MeanFlightsDuration.TotalHours}:{MeanFlightsDuration.Minutes}"; } }
        public TimeSpan MedianFlightsDuration { get; private set; }
        public string MedianFlightDurationText { get { return $"{(int)MedianFlightsDuration.TotalHours}:{MedianFlightsDuration.Minutes}"; } }
        public int FlightsCount { get; private set; }
        public HistData FlightsDurationHistData { get; private set; } = new(Array.Empty<double>(),Array.Empty<double>());

        public FlightsStatisticsViewModel(MainViewModel mvm) {
        m_MainViewModel = mvm;
        }
        public FlightsStatisticsViewModel(MainViewModel mainViewModel, DateTime AnalyzeStart, DateTime AnalyzeEnd)
        {
            m_MainViewModel = mainViewModel;
            // get the flights we have to put in the statistical analysis
            List<FlightViewModel> l_AnalyzeFlights = mainViewModel.FlightsInPeriod(AnalyzeStart, AnalyzeEnd);
            if (l_AnalyzeFlights.Count == 0)
                return;
            FlightsDuration = mainViewModel.FlightDurationInPeriod(AnalyzeStart, AnalyzeEnd);
            MeanFlightsDuration = FlightsDuration / l_AnalyzeFlights.Count;
            MedianFlightsDuration = l_AnalyzeFlights.OrderBy(flight => flight.FlightDuration)
                .ToList()[l_AnalyzeFlights.Count / 2].FlightDuration;
            FlightsCount = l_AnalyzeFlights.Count;
            List<double> l_flightsDurationsList = l_AnalyzeFlights.Select(f => f.FlightDuration.TotalHours).ToList();

            (double[] counts, double[] binEdges) = ComputeHistData(l_flightsDurationsList.ToArray(), 20);
            FlightsDurationHistData = new HistData(counts, binEdges);

            


        }

        static private (double[] counts, double[] binEdges) ComputeHistData(double[] sample, int groupCount)
        {
            double[] l_counts = new double[groupCount];
            double[] l_binEdges = new double[groupCount];
            double l_BinWidth = Math.Ceiling(sample.Max()) / groupCount;

            double l_PreviousBin = 0.0;
            for (int i = 0; i < groupCount; i++)
            {
                double l_CurrentBin = l_PreviousBin + l_BinWidth;
                l_binEdges[i] = l_CurrentBin;
                l_counts[i] = sample.Count(s => (s > l_PreviousBin) && (s <= l_CurrentBin));
                l_PreviousBin = l_CurrentBin;
            }
            return (l_counts, l_binEdges);
        }

        public double[] GetMonthlyMedian(int flightYear)
        {
            double[] l_MonthMedian = new double[12];
            for (int month = 1; month <= 12; month++)
            {
                FlightViewModel[]? l_MonthFlights = m_MainViewModel.FlightListViewModel
                .Where(f => f.TakeOffDateTime.Year == flightYear && f.TakeOffDateTime.Month == month).OrderBy(f => f.FlightDuration).ToArray();
                if (l_MonthFlights.Length != 0)
                {
                    int i = month -1;
                    l_MonthMedian[i] = l_MonthFlights[l_MonthFlights.Length / 2].FlightDuration.TotalHours;
                }

            }
            return l_MonthMedian;
        }
        public double[] GetMonthlyFlightHours(int flightYear)
        {
            double[] l_MonthFlightHour = new double[12];
            for (int month = 1; month <= 12; month++)
            {
                double l_MonthFlights = m_MainViewModel.FlightListViewModel
                .Where(f => f.TakeOffDateTime.Year == flightYear && f.TakeOffDateTime.Month == month)
                .Aggregate(TimeSpan.Zero,(sub, f)=> sub + f.FlightDuration).TotalHours;
                int i = month -1;
                l_MonthFlightHour[i] = l_MonthFlights;
            }
            return l_MonthFlightHour;
        }
        public string[] AvailableAnalysis
        {
            get
            {
                var availableAnalysis = new List<string>();
                foreach( StatisticalFlightsAnalysis analysis in Enum.GetValues(typeof(StatisticalFlightsAnalysis)))
                {
                    switch (analysis)
                    {
                        case StatisticalFlightsAnalysis.MontlyMedian:
                            availableAnalysis.Add("Monthly median");
                            break;
                        case StatisticalFlightsAnalysis.DurationDistribution:
                            availableAnalysis.Add("Duration Distribution");
                            break;
                        default:
                            break;
                    }
                }
                return availableAnalysis.ToArray();
            }
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

    public enum StatisticalFlightsAnalysis
    {
        DurationDistribution,
        MontlyMedian,
        MonthlyFlightDuration,
    }
}

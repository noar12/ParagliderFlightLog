using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ParagliderFlightLog.Models;
using ParaglidingFlightLogWeb.Components.Pages;
using ParaglidingFlightLogWeb.ViewModels;
using Radzen;


namespace ParaglidingFlightLogWeb.Services
{
    public class FlightStatisticService
    {
        private readonly CoreService _mainViewModel;
        public TimeSpan FlightsDuration { get; private set; }
        public string FlightsDurationText { get { return $"{(int)FlightsDuration.TotalHours:D2}:{FlightsDuration.Minutes:D2}"; } }
        public TimeSpan MeanFlightsDuration { get; private set; }
        public string MeanFlightsDurationText { get { return $"{(int)MeanFlightsDuration.TotalHours:D2}:{MeanFlightsDuration.Minutes:D2}"; } }
        public TimeSpan MedianFlightsDuration { get; private set; }
        public string MedianFlightDurationText { get { return $"{(int)MedianFlightsDuration.TotalHours:D2}:{MedianFlightsDuration.Minutes:D2}"; } }
        public int FlightsCount { get; private set; }
        public IEnumerable<FlightViewModel> TopScorer(int year = 1, int listSize = 3)
        {
            return _mainViewModel.FlightListViewModel
            .Where(f => f.TakeOffDateTime > new DateTime(year, 1, 1, 0, 0, 0, DateTimeKind.Utc) &&
                                f.XcScore is not null)
            .OrderByDescending(f => f.XcScore!.Points)
            .Take(listSize);
        }
        public IEnumerable<FlightViewModel> TopLongestFlight(int year = 1, int listSize = 3)
        {
            return _mainViewModel.FlightListViewModel
            .Where(f => f.TakeOffDateTime > new DateTime(year, 1, 1, 0, 0, 0, DateTimeKind.Utc))
            .OrderByDescending(f => f.FlightDuration)
            .Take(listSize);
        }
        public IEnumerable<FlightViewModel> TopHighestFlight(int year = 1, int listSize = 3)
        {
            return _mainViewModel.FlightListViewModel
            .Where(f => f.TakeOffDateTime > new DateTime(year, 1, 1, 0, 0, 0, DateTimeKind.Utc))
            .OrderByDescending(f => f.MaxAltitude)
            .Take(listSize);
        }
        public int LastYearSiteCount
        {
            get
            {
                return _mainViewModel.FlightListViewModel
                    .Where(f => f.TakeOffDateTime >= DateTime.Now - TimeSpan.FromDays(365.25) && f.TakeOffSite is not null)
                    .Select(f => f.TakeOffSite)
                    .DistinctBy(s => s!.Site_ID).Count();
            }
        }
        public GliderViewModel? OldestCheckUsedGlider
        {
            get
            {
                return _mainViewModel.FlightListViewModel.Where(f => f.TakeOffDateTime >= DateTime.Now - TimeSpan.FromDays(365.25) && f.TakeOffSite is not null && f.Glider is not null)
                .Select(f => f.Glider)
                .DistinctBy(g => g?.GliderId)
                .MinBy(g => g?.LastCheckDateTime);
            }
        }
        public HistData FlightsDurationHistData { get; private set; } = new([], []);

        public FlightStatisticService(CoreService mvm)
        {
            _mainViewModel = mvm;
        }

        /// <summary>
        /// List of month names
        /// </summary>
        public string[] MonthList { get; } =
        [
            "January",
            "February",
            "March",
            "April",
            "Mai",
            "June",
            "July",
            "August",
            "September",
            "October",
            "November",
            "December"
        ];

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
                l_counts[i] = sample.Count(s => s > l_PreviousBin && s <= l_CurrentBin);
                l_PreviousBin = l_CurrentBin;
            }
            return (l_counts, l_binEdges);
        }
        public HistData? GetFlightDurationRepartition(DateTime AnalyzeStart, DateTime AnalyzeEnd)
        {
            // get the flights we have to put in the statistical analysis
            List<FlightViewModel> l_AnalyzeFlights = _mainViewModel.FlightsInPeriod(AnalyzeStart, AnalyzeEnd);
            if (l_AnalyzeFlights.Count == 0)
                return null;
            FlightsDuration = _mainViewModel.FlightDurationInPeriod(AnalyzeStart, AnalyzeEnd);
            MeanFlightsDuration = FlightsDuration / l_AnalyzeFlights.Count;
            MedianFlightsDuration = l_AnalyzeFlights.OrderBy(flight => flight.FlightDuration)
                .ToList()[l_AnalyzeFlights.Count / 2].FlightDuration;
            FlightsCount = l_AnalyzeFlights.Count;
            List<double> l_flightsDurationsList = l_AnalyzeFlights.Select(f => f.FlightDuration.TotalHours).ToList();

            (double[] counts, double[] binEdges) = ComputeHistData([.. l_flightsDurationsList], 20);
            return new HistData(counts, binEdges);
        }

        public double[] GetMonthlyMedian(int flightYear)
        {
            double[] l_MonthMedian = new double[12];
            for (int month = 1; month <= 12; month++)
            {
                FlightViewModel[]? l_MonthFlights =
                [
                    .. _mainViewModel.FlightListViewModel
                                    .Where(f => f.TakeOffDateTime.Year == flightYear && f.TakeOffDateTime.Month == month).OrderBy(f => f.FlightDuration),
                ];
                if (l_MonthFlights.Length != 0)
                {
                    int i = month - 1;
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
                double l_MonthFlights = _mainViewModel.FlightListViewModel
                .Where(f => f.TakeOffDateTime.Year == flightYear && f.TakeOffDateTime.Month == month)
                .Aggregate(TimeSpan.Zero, (sub, f) => sub + f.FlightDuration).TotalHours;
                int i = month - 1;
                l_MonthFlightHour[i] = l_MonthFlights;
            }
            return l_MonthFlightHour;
        }
        public double[] GetCumulatedFlightHoursPerMonth(int flightYear)
        {
            double[] output = new double[12];
            double[] MonthFlightHours = GetMonthlyFlightHours(flightYear);
            for (int i = 0; i < output.Length - 1; i++)
            {
                output[i] = MonthFlightHours[..(i + 1)].Sum();
            }
            output[^1] = MonthFlightHours.Sum();
            return output;
        }
        

        public XcScoreOverTheYears[] GetXcScoresOverTheYears()
        {
            XcScoreOverTheYears[] output = new XcScoreOverTheYears[4];
            output[0] = new XcScoreOverTheYears(){FlightCount = 4, XcScores = new XcScoreItem[_mainViewModel.YearsOfFlying.Count]};
            output[1] = new XcScoreOverTheYears(){FlightCount = 6, XcScores = new XcScoreItem[_mainViewModel.YearsOfFlying.Count]};
            output[2] = new XcScoreOverTheYears(){FlightCount = 10, XcScores = new XcScoreItem[_mainViewModel.YearsOfFlying.Count]};
            output[3] = new XcScoreOverTheYears(){FlightCount = 20, XcScores = new XcScoreItem[_mainViewModel.YearsOfFlying.Count]};
            int i = 0;
            foreach (int year in _mainViewModel.YearsOfFlying)
            {
                var bestFlights = _mainViewModel.FlightListViewModel
                    .Where(f => f.TakeOffDateTime.Year == year && f.XcScore is not null)
                    .OrderByDescending(f => f.XcScore!.Points).ToArray();
                var bestofFour = bestFlights.Take(4).ToArray();
                var bestofSix = bestFlights.Take(6).ToArray();
                var bestofTen = bestFlights.Take(10).ToArray();
                var bestofTwenty = bestFlights.Take(20).ToArray();

                output[0].XcScores[i] = new XcScoreItem(){
                    Year = year,
                    XcScore = bestofFour.Sum(f => f.XcScore!.Points),
                };
                output[1].XcScores[i] = new XcScoreItem(){
                    Year = year,
                    XcScore = bestofSix.Sum(f => f.XcScore!.Points),
                };
                output[2].XcScores[i] = new XcScoreItem(){
                    Year = year,
                    XcScore = bestofTen.Sum(f => f.XcScore!.Points),
                };
                output[3].XcScores[i] = new XcScoreItem(){
                    Year = year,
                    XcScore = bestofTwenty.Sum(f => f.XcScore!.Points),
                };
                ++i;
            };
            return output;
        }
    }

    public class XcScoreOverTheYears
    {
        public int? FlightCount { get; init; }
        public XcScoreItem[] XcScores { get; init; }
        public string Name => $"{FlightCount?.ToString() ?? "all"} flights";
    }
    public class XcScoreItem
    {
        public int Year { get; set; }
        public string YearText => Year.ToString();
        public double XcScore { get; set; }
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
        MonthlyCumulatedFlightDuration,
        XcoreOverTheYears,
    }
}

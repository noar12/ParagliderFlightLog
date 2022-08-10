using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using WpfUI.ViewModels;
namespace WpfUI.Controls
{
    /// <summary>
    /// Interaction logic for FlightsStatistic.xaml
    /// </summary>
    public partial class FlightsStatisticControl : UserControl
    {
        public static readonly DependencyProperty SourceProperty = DependencyProperty.Register("Source", typeof(MainViewModel), typeof(FlightsStatisticControl), new PropertyMetadata(null));

        public MainViewModel Source
        {
            get { return (MainViewModel)GetValue(SourceProperty); }
            set { SetValue(SourceProperty, value); }
        }

        

        public FlightsStatisticControl()
        {
            InitializeComponent();
            CommandManager.RegisterClassCommandBinding(typeof(FlightsStatisticControl),
                new CommandBinding(LogBookCommand.StatisticAnalyzeCommand, OnAnalyze,
                (s, e) => { e.CanExecute = AnalyzeType.SelectedItem != null; }));
        }

        //private void CanAnalyze(object sender, CanExecuteRoutedEventArgs e)
        //{
        //    e.CanExecute = AnalyzeType.SelectedIndex >= 0;
        //}

        private void OnAnalyze(object sender, ExecutedRoutedEventArgs e)
        {
            FlightsStatisticsViewModel flightsStatisticsViewModel;
            //build Flight statistic view model for the selected year
            if (AnalyzeYear.SelectedItem != null)
            {
                flightsStatisticsViewModel =
    new FlightsStatisticsViewModel(Source, new DateTime((int)AnalyzeYear.SelectedItem, 1, 1), new DateTime((int)AnalyzeYear.SelectedItem, 12, 31, 23, 59, 59));
            }
            else
            {
                flightsStatisticsViewModel = new FlightsStatisticsViewModel(Source, DateTime.MinValue, DateTime.Now);
            }


            //Write general statistic about the selected year
            FlightsDurationText.Text = flightsStatisticsViewModel.FlightsDurationText;
            FlightsMeanDurationText.Text = flightsStatisticsViewModel.MeanFlightsDurationText;
            FlightsMedianDurationText.Text = flightsStatisticsViewModel.MedianFlightDurationText;
            FlightsCountText.Text = flightsStatisticsViewModel.FlightsCount.ToString();

            switch (AnalyzeType.SelectedIndex)
            {
                case 0:
                    FlightStatisticPlot.Plot.Clear();
                    var bar = FlightStatisticPlot.Plot.AddBar(flightsStatisticsViewModel.FlightsDurationHistData.Counts, flightsStatisticsViewModel.FlightsDurationHistData.BinEdges);
                    bar.BarWidth = 0.05;

                    // customize the plot style
                    FlightStatisticPlot.Plot.YAxis.Label("Flight count (#)");
                    FlightStatisticPlot.Plot.XAxis.Label("Duration (Hours)");
                    FlightStatisticPlot.Plot.XAxis.AutomaticTickPositions();
                    FlightStatisticPlot.Plot.Title("Flight duration distribution");
                    FlightStatisticPlot.Plot.SetAxisLimits(yMin: 0);


                    FlightStatisticPlot.Refresh();
                    break;
                case 1:
                    FlightStatisticPlot.Plot.Clear();

                    string[] l_MonthList = { "January", "February", "March", "April", "Mai", "June", "July", "August", "Septempber", "October", "November", "December" };
                    string[] l_YearsText = new string[Source.YearsOfFlying.Count];
                    double[][] l_MedianValues = new double[Source.YearsOfFlying.Count][];
                    int i = 0;
                    if (AnalyzeYear.SelectedIndex == -1)
                    {

                        foreach (int l_FlightYear in Source.YearsOfFlying)
                        {
                            double[] l_MonthData = flightsStatisticsViewModel.GetMonthlyMedian(l_FlightYear);
                            l_YearsText[i] = l_FlightYear.ToString();
                            l_MedianValues[i] = l_MonthData;
                            i++;
                        }
                        FlightStatisticPlot.Plot.AddBarGroups(l_MonthList, l_YearsText, l_MedianValues, new double[l_MedianValues.Length][]);
                        FlightStatisticPlot.Plot.Title("Month median flight duration for each year");
                    }
                    else
                    {
                        if (AnalyzeYear.SelectedItem is int year)
                        {
                            double[] l_MonthNumber = { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11};
                            double[] l_MonthData = flightsStatisticsViewModel.GetMonthlyMedian(year);
                            l_YearsText[i] = year.ToString();
                            l_MedianValues[i] = l_MonthData;
                            FlightStatisticPlot.Plot.AddBar(l_MonthData);
                            FlightStatisticPlot.Plot.XAxis.ManualTickPositions(l_MonthNumber, l_MonthList);
                        }
   
                    }

                    
                    FlightStatisticPlot.Plot.YAxis.Label("Median duration (Hours)");
                    FlightStatisticPlot.Plot.XAxis.Label("Month (-)");
                    
                    FlightStatisticPlot.Plot.SetAxisLimits(yMin: 0);
                    FlightStatisticPlot.Plot.Legend(true, location: ScottPlot.Alignment.UpperRight);
                    FlightStatisticPlot.Refresh();
                    break;
                default:
                    break;
            }

        }

        private void AnalyzeType_DropDownClosed(object sender, EventArgs e)
        {

            //this has to be move in the view model
            var l_FlightYearsData = Source.FlightListViewModel.GroupBy(f => f.TakeOffDateTime.Year).OrderBy(group => group.Key);

            foreach (var flightYearData in l_FlightYearsData)
            {

                var l_FlightMonthsDataInYear = flightYearData.GroupBy(f => f.TakeOffDateTime.Month).OrderBy(group => group.Key);
                foreach (var flightMonthData in l_FlightMonthsDataInYear)
                {
                    var l_OrderedFlightmonthData = flightMonthData.OrderBy(f => f.FlightDuration).ToArray();
                    // the middle of the ordered sequence is the median (at least of sample count is even)
                    var l_MonthDurationMedian = l_OrderedFlightmonthData[l_OrderedFlightmonthData.Length / 2].FlightDuration;

                }
                //
                //
                //FlightDurationDistPlot.Plot.AddScatter();

            }


        }

        private void Analyze_Click(object sender, RoutedEventArgs e)
        {
            
        }

        private void ClearYear_Click(object sender, RoutedEventArgs e)
        {
            AnalyzeYear.SelectedIndex = -1;
        }
    }
}

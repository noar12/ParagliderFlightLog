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
            
        }

        private void AnalyzeYear_DropDownClosed(object sender, EventArgs e)
        {
            //build Flight statistic view model for the selected year
            FlightsStatisticsViewModel flightsStatisticsViewModel = 
                new FlightsStatisticsViewModel(Source,new DateTime((int)AnalyzeYear.SelectedItem,1,1), new DateTime((int)AnalyzeYear.SelectedItem,12,31,23,59,59) );
            FlightsDurationText.Text = $"{(int)flightsStatisticsViewModel.FlightsDuration.TotalHours}:{flightsStatisticsViewModel.FlightsDuration.Minutes}";
            FlightsMeanDurationText.Text = $"{(int)flightsStatisticsViewModel.MeanFlightsDuration.TotalHours}:{flightsStatisticsViewModel.MeanFlightsDuration.Minutes}";
            FlightsMedianDurationText.Text = $"{(int)flightsStatisticsViewModel.MedianFlightsDuration.TotalHours}:{flightsStatisticsViewModel.MedianFlightsDuration.Minutes}";
            FlightsCountText.Text = flightsStatisticsViewModel.FlightsCount.ToString();
            FlightDurationDistPlot.Plot.Clear();
            var bar = FlightDurationDistPlot.Plot.AddBar(flightsStatisticsViewModel.FlightsDurationHistData.Counts, flightsStatisticsViewModel.FlightsDurationHistData.BinEdges);
            bar.BarWidth = 0.05;

            // customize the plot style
            FlightDurationDistPlot.Plot.YAxis.Label("Flight count (#)");
            FlightDurationDistPlot.Plot.XAxis.Label("Duration (Hours)");
            FlightDurationDistPlot.Plot.Title("Flight duration distribution");
            FlightDurationDistPlot.Plot.SetAxisLimits(yMin: 0);

            
            FlightDurationDistPlot.Refresh();
        }
    }
}

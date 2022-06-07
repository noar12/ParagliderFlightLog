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
using ParagliderFlightLog.DataModels;
using WpfUI.Controls;

namespace WpfUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        FlightListControl flc = new FlightListControl();
        SiteListControl slc = new SiteListControl();

        public MainWindow()
        {
            InitializeComponent();

            LogFlyDB l_logFlyDB = new LogFlyDB();
            l_logFlyDB.LoadLogFlyDB("Logfly.db");
            FlightLogDB = l_logFlyDB.BuildFlightLogDB();
            flc.DataContext = x_MainWindow;
            flc.Source = FlightLogDB;
            slc.DataContext = x_MainWindow;
            slc.Source = FlightLogDB;


        }

        public static readonly DependencyProperty FlightLogDBProperty = DependencyProperty.Register("FlightLogDB", typeof(FlightLogDB), typeof(MainWindow), new PropertyMetadata(null));
        public FlightLogDB FlightLogDB
        {
            get { return (FlightLogDB)GetValue(FlightLogDBProperty); }
            set { SetValue(FlightLogDBProperty, value); } 
        }

        private void FlightListButton_Click(object sender, RoutedEventArgs e)
        {
            MainContent.Content = flc;
        }

        private void SiteListButton_Click(object sender, RoutedEventArgs e)
        {
            MainContent.Content = slc;
        }
    }
}

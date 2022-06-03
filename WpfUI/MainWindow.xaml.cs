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

namespace WpfUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private FlightLogDB m_flightLogDB = new FlightLogDB();
        public MainWindow()
        {
            InitializeComponent();

            LogFlyDB l_logFlyDB = new LogFlyDB();
            l_logFlyDB.LoadLogFlyDB("Logfly.db");
            FlightLogDB = l_logFlyDB.BuildFlightLogDB();
            
        }

        public FlightLogDB FlightLogDB { get => m_flightLogDB; set => m_flightLogDB = value; }

        private void FlightListButton_MouseDown(object sender, MouseButtonEventArgs e)
        {

        }
    }
}

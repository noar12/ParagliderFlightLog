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
using WpfUI.Controls;
using WpfUI.ViewModels;
namespace WpfUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        FlightListControl flc = new FlightListControl();
        SiteListControl slc = new SiteListControl();
        FlightsStatisticControl fsc = new FlightsStatisticControl();

        public MainWindow()
        {
            InitializeComponent();
            MainViewModel mainViewModel = new MainViewModel();

            
            flc.Source = mainViewModel.FlightListViewModel;
            
            slc.Source = mainViewModel.SiteListViewModel;
            Status.DataContext = mainViewModel;

            fsc.Source = mainViewModel;

        }



        private void FlightListButton_Click(object sender, RoutedEventArgs e)
        {
            MainContent.Content = flc;
        }

        private void SiteListButton_Click(object sender, RoutedEventArgs e)
        {
            MainContent.Content = slc;
        }
        private void FlightStatButton_Click(object sender, RoutedEventArgs e)
        {
            MainContent.Content = fsc;
        }
    }
}

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
//using ParagliderFlightLog.DataModels;
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

        public MainWindow()
        {
            InitializeComponent();
            MainViewModel mainViewModel = new MainViewModel();

            flc.DataContext = x_MainWindow;
            flc.Source = mainViewModel.FlightListViewModel;
            slc.DataContext = x_MainWindow;
            slc.Source = mainViewModel.SiteListViewModel;
            Status.DataContext = mainViewModel;


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

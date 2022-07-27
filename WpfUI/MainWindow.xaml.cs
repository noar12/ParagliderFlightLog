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

        MainViewModel mainViewModel = new MainViewModel();



        public MainWindow()
        {
            CommandManager.RegisterClassCommandBinding(typeof(MainWindow),
                new CommandBinding(LogBookCommand.ImportIGC_Command, OnImportIGC,
                (s, e) => { e.CanExecute = true; }));
            CommandManager.RegisterClassCommandBinding(typeof(MainWindow),
                new CommandBinding(LogBookCommand.ImportLogFlyDbCommand, OnImportLogFlyDB,
                (s, e) => { e.CanExecute = true; }));
            InitializeComponent();



           
            slc.Source = mainViewModel;
            
            flc.Source = mainViewModel.FlightListViewModel;

            
            Status.DataContext = mainViewModel;

            fsc.Source = mainViewModel;


        }

        private void OnAddSite(object sender, ExecutedRoutedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void OnImportLogFlyDB(object sender, ExecutedRoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog openFileDlg = new Microsoft.Win32.OpenFileDialog();
            if (openFileDlg.ShowDialog() == true) {
                mainViewModel.ImportLogFlyDB(openFileDlg.FileName);
            }
            
        }

        private void OnImportIGC(object sender, ExecutedRoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog openFileDlg = new Microsoft.Win32.OpenFileDialog();
            if (openFileDlg.ShowDialog() == true)
            {
                mainViewModel.AddFlightFromIGC(openFileDlg.FileName);
            }

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
            //fsc.AnalyzeYear.SelectedItem = fsc.AnalyzeYear.Items[fsc.AnalyzeYear.Items.Count - 1];
            // find a solution to fire dropdown event to trigger the statistic calculation of the last item

        }
    }
}

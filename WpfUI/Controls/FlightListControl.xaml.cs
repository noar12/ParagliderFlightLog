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
using ParagliderFlightLog.ViewModels;
using WpfUI.Forms;

namespace WpfUI.Controls
{
    /// <summary>
    /// Interaction logic for FlightListControl.xaml
    /// </summary>
    public partial class FlightListControl : UserControl
    {
        public FlightListControl()
        {
            InitializeComponent();
            CommandManager.RegisterClassCommandBinding(typeof(FlightListControl),
                new CommandBinding(LogBookCommand.RemoveFlightCommand, OnRemoveFlights,
                (s, e) => { e.CanExecute = FlightGrid.SelectedItem != null; }));
            CommandManager.RegisterClassCommandBinding(typeof(FlightListControl),
                new CommandBinding(LogBookCommand.EditFlightCommand, OnEditFlights,
                (s, e) => { e.CanExecute = FlightGrid.SelectedItems.Count == 1; }));
        }

        private void OnEditFlights(object sender, ExecutedRoutedEventArgs e)
        {
            if (FlightGrid.SelectedItem is FlightViewModel fvm)
            {
                FlightForm flightForm = new FlightForm();
                flightForm.Source = Source;
                flightForm.EditableFlight = SelectedItem;
                if (flightForm.ShowDialog() == true)
                {
                    Source.EditFlight(SelectedItem);
                }

            }
        }

        private void OnRemoveFlights(object sender, ExecutedRoutedEventArgs e)
        {
            List<FlightViewModel> l_FlightToRemove = new List<FlightViewModel>();
            foreach (object? item in FlightGrid.SelectedItems)
            {
                if (item is FlightViewModel fvm)
                {
                    l_FlightToRemove.Add(fvm);
                }
            }
            foreach (FlightViewModel fvm in l_FlightToRemove)
            {
                fvm.RemoveFlight();
            }
        }

        public static readonly DependencyProperty SourceProperty = DependencyProperty.Register("Source", typeof(MainViewModel), typeof(FlightListControl), new PropertyMetadata(null));
        public static readonly DependencyProperty SelectedItemProperty = DependencyProperty.Register("SelectedItem", typeof(FlightViewModel), typeof(FlightListControl), new PropertyMetadata(null));

        //private static void OnChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        //{

        //}
        public MainViewModel Source
        {
            get { return (MainViewModel)GetValue(SourceProperty); }
            set { SetValue(SourceProperty, value); }
        }
        
        public FlightViewModel SelectedItem
        {
            get { return (FlightViewModel)GetValue(SelectedItemProperty); }
            set { SetValue(SelectedItemProperty, value); }
        }
    }
}

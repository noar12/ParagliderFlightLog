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
    /// Interaction logic for FlightListControl.xaml
    /// </summary>
    public partial class FlightListControl : UserControl
    {
        public FlightListControl()
        {
            InitializeComponent();
            CommandManager.RegisterClassCommandBinding(typeof(FlightListControl),
                new CommandBinding(LogBookCommand.RemoveFlightCommand, OnRemoveFlight,
                (s, e) => { e.CanExecute = FlightGrid.SelectedItem != null; }));
        }

        private void OnRemoveFlight(object sender, ExecutedRoutedEventArgs e)
        {
            if (FlightGrid.SelectedItem is FlightViewModel fwm)
            {
                Source.Remove(fwm);
            }
        }

        public static readonly DependencyProperty SourceProperty = DependencyProperty.Register("Source", typeof(ICollection<FlightViewModel>), typeof(FlightListControl), new PropertyMetadata(null));
        public static readonly DependencyProperty SelectedItemProperty = DependencyProperty.Register("SelectedItem", typeof(FlightViewModel), typeof(FlightListControl), new PropertyMetadata(null));

        //private static void OnChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        //{

        //}
        public ICollection<FlightViewModel> Source
        {
            get { return (ICollection<FlightViewModel>)GetValue(SourceProperty); }
            set { SetValue(SourceProperty, value); }
        }
        
        public FlightViewModel SelectedItem
        {
            get { return (FlightViewModel)GetValue(SelectedItemProperty); }
            set { SetValue(SelectedItemProperty, value); }
        }
    }
}

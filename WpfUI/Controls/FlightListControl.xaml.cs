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
            
        }


        public static readonly DependencyProperty SourceProperty = DependencyProperty.Register("Source", typeof(FlightLogDB), typeof(FlightListControl), new PropertyMetadata(null));

        public FlightLogDB Source
        {
            get { return (FlightLogDB)GetValue(SourceProperty); }
            set { SetValue(SourceProperty, value); }
        }
        
        public Flight SelectedItem { get; set; }
    }
}

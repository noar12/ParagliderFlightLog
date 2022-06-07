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
    /// Interaction logic for SiteListControl.xaml
    /// </summary>
    public partial class SiteListControl : UserControl
    {
        public SiteListControl()
        {
            InitializeComponent();
        }
        public static readonly DependencyProperty SourceProperty = DependencyProperty.Register("Source", typeof(FlightLogDB), typeof(SiteListControl), new PropertyMetadata(null));
        public static readonly DependencyProperty SelectedItemProperty = DependencyProperty.Register("SelectedItem", typeof(Site), typeof(SiteListControl), new PropertyMetadata(null));

        //private static void OnChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        //{

        //}
        public FlightLogDB Source
        {
            get { return (FlightLogDB)GetValue(SourceProperty); }
            set { SetValue(SourceProperty, value); }
        }

        public Site SelectedItem
        {
            get { return (Site)GetValue(SelectedItemProperty); }
            set { SetValue(SelectedItemProperty, value); }
        }
    }
}

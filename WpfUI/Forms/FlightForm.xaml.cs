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
using System.Windows.Shapes;
using ParagliderFlightLog.ViewModels;

namespace WpfUI.Forms
{
    /// <summary>
    /// Interaction logic for FlightForm.xaml
    /// </summary>
    public partial class FlightForm : Window
    {
        public FlightForm()
        {
            InitializeComponent();
        }
        public FlightViewModel EditableFlight
        {
            get { return (FlightViewModel)GetValue(EditableFlightProperty); }
            set { SetValue(EditableFlightProperty, value); }
        }

        // Using a DependencyProperty as the backing store for EditableFlight.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty EditableFlightProperty =
            DependencyProperty.Register("EditableFlight", typeof(FlightViewModel), typeof(FlightForm), new PropertyMetadata(null));

        public MainViewModel Source
        {
            get { return (MainViewModel)GetValue(SourceProperty); }
            set { SetValue(SourceProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Source.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SourceProperty =
            DependencyProperty.Register("Source", typeof(MainViewModel), typeof(FlightForm), new PropertyMetadata(null));

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            EditableFlight.Comment = CommentTextBlock.Text;
            EditableFlight.TakeOffSiteName = SiteComboBox.Text;
            EditableFlight.GliderName = GliderComboBox.Text;
            this.DialogResult = true;
            this.Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }
    }
}

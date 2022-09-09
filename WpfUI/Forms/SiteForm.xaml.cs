using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
    /// Interaction logic for AddSiteForm.xaml
    /// </summary>
    public partial class SiteForm : Window
    {

        public SiteViewModel SelectedSite
        {
            get { return (SiteViewModel)GetValue(SelectedSiteProperty); }
            set { SetValue(SelectedSiteProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SelectedSite.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SelectedSiteProperty =
            DependencyProperty.Register("SelectedSite", typeof(SiteViewModel), typeof(SiteForm));



        //public ICollection<SiteViewModel> SiteCollection
        //{
        //    get { return (ICollection<SiteViewModel>)GetValue(SiteCollectionProperty); }
        //    set { SetValue(SiteCollectionProperty, value); }
        //}

        //// Using a DependencyProperty as the backing store for SiteCollection.  This enables animation, styling, binding, etc...
        //public static readonly DependencyProperty SiteCollectionProperty =
        //    DependencyProperty.Register("SiteCollection", typeof(ICollection<SiteViewModel>), typeof(AddSiteForm));



        public SiteForm(SiteViewModel site)
        {
            InitializeComponent();
            this.SizeToContent = SizeToContent.WidthAndHeight;
            DataContext = this;
            SelectedSite = site;

            SiteName.Text = SelectedSite.Name;
            Town.Text = SelectedSite.Town;
            Country.SelectedItem = SelectedSite.Country;
            WindOrientationStart.SelectedItem = SelectedSite.WindOrientationBegin;
            WindOrientationEnd.SelectedItem = SelectedSite.WindOrientationEnd;
            Latitude.Text = SelectedSite.Latitude.ToString();
            Longitude.Text = SelectedSite.Longitude.ToString();
            Altitude.Text = SelectedSite.Altitude.ToString();
        }

        private void OK_Click(object sender, RoutedEventArgs e)
        {
            
            SelectedSite.Name = SiteName.Text;
            SelectedSite.Town = Town.Text;
            SelectedSite.Country = Country.Text;
            SelectedSite.WindOrientationBegin = WindOrientationStart.Text;
            SelectedSite.WindOrientationEnd = WindOrientationEnd.Text;
            
            double.TryParse(Latitude.Text, out double l_Latitude);
            double.TryParse(Longitude.Text, out double l_Longitude);
            double.TryParse(Altitude.Text, out double l_Altitude);
            SelectedSite.Latitude = l_Latitude;
            SelectedSite.Longitude = l_Longitude;
            SelectedSite.Altitude = l_Altitude;

            this.DialogResult = true;
            this.Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }
    }
}

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
using WpfUI.ViewModels;

namespace WpfUI.Forms
{
    /// <summary>
    /// Interaction logic for AddSiteForm.xaml
    /// </summary>
    public partial class AddSiteForm : Window
    {



        public bool EditMode
        {
            get { return (bool)GetValue(EditModeProperty); }
            set { SetValue(EditModeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for EditMode.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty EditModeProperty =
            DependencyProperty.Register("EditMode", typeof(bool), typeof(SiteViewModel));




        public SiteViewModel SelectedSite
        {
            get { return (SiteViewModel)GetValue(SelectedSiteProperty); }
            set { SetValue(SelectedSiteProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SelectedSite.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SelectedSiteProperty =
            DependencyProperty.Register("SelectedSite", typeof(SiteViewModel), typeof(AddSiteForm));



        public ICollection<SiteViewModel> SiteCollection
        {
            get { return (ICollection<SiteViewModel>)GetValue(SiteCollectionProperty); }
            set { SetValue(SiteCollectionProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SiteCollection.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SiteCollectionProperty =
            DependencyProperty.Register("SiteCollection", typeof(ICollection<SiteViewModel>), typeof(AddSiteForm));



        public AddSiteForm()
        {
            InitializeComponent();
            this.SizeToContent = SizeToContent.WidthAndHeight;
            if (EditMode)
            {
                Name.Text = SelectedSite.Name;
                Town.Text = SelectedSite.Town;
                Country.Text = SelectedSite.Country;
                WindOrientationStart.Text = SelectedSite.WindOrientationBegin;
                WindOrientationEnd.Text = SelectedSite.WindOrientationEnd;
                Latitude.Text = SelectedSite.Latitude.ToString();
                Longitude.Text = SelectedSite.Longitude.ToString();
                Altitude.Text = SelectedSite.Altitude.ToString();
            }
        }

        private void OK_Click(object sender, RoutedEventArgs e)
        {
            
            string l_Name = Name.Text;
            string l_Town = Town.Text;
            string l_Country = Country.Text;
            string l_WindOrientationBegin = WindOrientationStart.Text;
            string l_WindOrientationEnd = WindOrientationEnd.Text;
            double.TryParse(Latitude.Text, out double l_Latitude);
            double.TryParse(Longitude.Text, out double l_Longitude);
            double.TryParse(Altitude.Text, out double l_Altitude);
            if (!EditMode)
            {
                SiteViewModel siteViewModel = new SiteViewModel()
                {
                    Site_ID = new Guid().ToString(),
                    Name = l_Name,
                    Town = l_Town,
                    Country = l_Country,
                    WindOrientationBegin = l_WindOrientationBegin,
                    WindOrientationEnd = l_WindOrientationEnd,
                    Latitude = l_Latitude,
                    Longitude = l_Longitude,
                    Altitude = l_Altitude,

                };

                SiteCollection.Add(siteViewModel);
            }
            
            

            this.Close();
        }
    //    public void AddSiteFromForm(string name, string town, ECountry country,
    //EWindOrientation windOrientationStart, EWindOrientation windOrientationEnd,
    //double latitude, double longitude, double altitude)
    //    {
    //        Site l_site = new Site()
    //        {
    //            Name = name,
    //            Town = town,
    //            Country = country,
    //            WindOrientationBegin = windOrientationStart,
    //            WindOrientationEnd = windOrientationEnd,
    //            Latitude = latitude,
    //            Longitude = longitude,
    //            Altitude = altitude,
    //            WindOrientationSpecified = true
    //        };
    //        SiteViewModel fvm = new SiteViewModel()
    //        {
    //            Altitude = altitude,
    //            Name = name,
    //            WindOrientation = l_site.WindOrientationText,
    //        };
    //        m_flightLog.Sites.Add(l_site);
    //        SiteListViewModel.Add(fvm);
    //    }
        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}

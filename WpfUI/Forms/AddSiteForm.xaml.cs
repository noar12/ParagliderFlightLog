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
        public AddSiteForm()
        {
            InitializeComponent();
            this.SizeToContent = SizeToContent.WidthAndHeight;
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

            SiteViewModel siteViewModel = new SiteViewModel()
            {
                Name = l_Name,
                Town = l_Town,
                Country = l_Country,
                WindOrientationBegin = l_WindOrientationBegin,
                WindOrientationEnd = l_WindOrientationEnd,
                Latitude = l_Latitude,
                Longitude = l_Longitude,
                Altitude = l_Altitude,
                WindOrientation = $"{ l_WindOrientationBegin } - {l_WindOrientationEnd}"
            };
            if (DataContext is ICollection<SiteViewModel> l_SiteViewModel) 
            {
                l_SiteViewModel.Add(siteViewModel);
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

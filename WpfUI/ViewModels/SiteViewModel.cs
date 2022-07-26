using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ParagliderFlightLog.DataModels;

namespace WpfUI.ViewModels
{
    public class SiteViewModel
    {
        public string Site_ID { get; set; }
        public string Name { get; set; }
        public double Altitude { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string Country { get; set; }
        public string Town { get; set; }
        public string WindOrientation { get { return $"{WindOrientationBegin} - {WindOrientationEnd}"; } }
        public string WindOrientationBegin { get; set; }
        public string WindOrientationEnd { get; set; }
        public string[] AvailableWindOrientation { get => Enum.GetNames(typeof(EWindOrientation)); }
        public string[] AvailableCountry { get => Enum.GetNames(typeof(ECountry)); }


    }
}

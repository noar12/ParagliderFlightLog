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
        public string Name { get; set; }
        public double Altitude { get; set; }
        public string WindOrientation { get; set; }
        public string[] AvailableWindOrientation { get => Enum.GetNames(typeof(EWindOrientation)); }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ParagliderFlightLog.DataModels;

namespace WpfUI.ViewModels
{
    public class FlightViewModel
    {
        public DateTime TakeOffDateTime { get; set; }
        public TimeSpan FlightDuration { get; set; }
        public string TakeOffSiteName { get; set; }
        public string GliderName { get; set; }
        public double MaxHeight { get => FlightPoints.Select(fp => fp.Height).ToList().Max(); }
        public List<FlightPoint> FlightPoints { get; set; }
        public string Comment { get; set; }
    }


}

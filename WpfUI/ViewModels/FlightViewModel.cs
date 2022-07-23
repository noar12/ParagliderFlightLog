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
        public FlightViewModel()
        {
        }

        public FlightViewModel(string flightID, DateTime takeOffDateTime, TimeSpan flightDuration, string takeOffSiteName, string gliderName, List<FlightPoint> flightPoints, string comment)
        {
            FlightID = flightID;
            TakeOffDateTime = takeOffDateTime;
            FlightDuration = flightDuration;
            TakeOffSiteName = takeOffSiteName;
            GliderName = gliderName;
            FlightPoints = flightPoints;
            Comment = comment;
        }

        public string FlightID { get; set; } = "";
        public DateTime TakeOffDateTime { get; set; } = DateTime.MinValue;
        public TimeSpan FlightDuration { get; set; } = TimeSpan.Zero;
        public string TakeOffSiteName { get; set; } = "Unknown site";
        public string GliderName { get; set; } = "Unknown glider";
        public double MaxHeight { get => FlightPoints.Select(fp => fp.Height).ToList().Max(); }
        public List<FlightPoint> FlightPoints { get; set; } = new List<FlightPoint>();
        public string Comment { get; set; } = "";
    }


}

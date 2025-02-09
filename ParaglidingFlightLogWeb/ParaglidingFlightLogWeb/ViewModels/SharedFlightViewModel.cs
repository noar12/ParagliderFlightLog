using ParagliderFlightLog.Models;

namespace ParaglidingFlightLogWeb.ViewModels
{
    public class SharedFlightViewModel
    {
        private readonly SharedFlight _flight;
        public SharedFlightViewModel(SharedFlight flight)
        {
            _flight = flight;
        }

        public string TakeOffDateTime => _flight.TakeOffDateTime.ToString("g");
        public string SiteName => _flight.SiteName;
        public string GliderName => _flight.GliderName;
        public double MaxAltitude => _flight.FlightPoints.Max(p => p.Altitude);
        
    }
}
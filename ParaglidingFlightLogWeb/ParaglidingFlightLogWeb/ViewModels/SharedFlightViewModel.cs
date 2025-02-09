using ParagliderFlightLog.Models;

namespace ParaglidingFlightLogWeb.ViewModels
{
    public class SharedFlightViewModel
    {
        private const int INTEGRATION_STEP_S = 8;
        private readonly SharedFlight _flight;
        public SharedFlightViewModel(SharedFlight flight)
        {
            _flight = flight;
        }

        public string TakeOffDateTime => _flight.TakeOffDateTime.ToString("g");
        public string SiteName => _flight.SiteName;
        public string GliderName => _flight.GliderName;
        public double MaxAltitude => _flight.FlightPoints.Max(p => p.Altitude);
        /// <summary>
        /// Get the trace length in km
        /// </summary>
        public double TraceLength => ViewModelHelpers.GetTraceLength(_flight.FlightPoints);

        public double MaxClimb => ViewModelHelpers.GetVerticalRate(_flight.FlightPoints, INTEGRATION_STEP_S).Max();
        public double MaxSink => ViewModelHelpers.GetVerticalRate(_flight.FlightPoints, INTEGRATION_STEP_S).Min();
        private XcScoreViewModel? _xcScoreVm;
        public XcScoreViewModel? XcScore {
            get
            {
                if (_xcScoreVm is not null)
                {
                    return _xcScoreVm;
                }

                XcScore? xcScore = ParagliderFlightLog.Models.XcScore.FromJson(_flight.GeoJsonScore);
                if (xcScore is null) { return null;}
                _xcScoreVm = new XcScoreViewModel(xcScore);

                return _xcScoreVm;

            }
        }

        public List<FlightPoint> FlightPoints => _flight.FlightPoints;
        public string Comment => _flight.Comment ?? "";
        public DateTime TakeOffDateTimeData => _flight.TakeOffDateTime;
        public TimeSpan FlightDuration => _flight.FlightDuration;

        /// <summary>
        /// Return the photo of the flights for the UI
        /// </summary>
        public List<FlightPhotoViewModel> GetFlightPhotos() => _flight.Photos.Select(x => new FlightPhotoViewModel(x)).ToList();
    }
}
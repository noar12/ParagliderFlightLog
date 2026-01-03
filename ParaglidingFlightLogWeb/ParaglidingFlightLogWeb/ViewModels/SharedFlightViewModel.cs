using ParagliderFlightLog.Models;

namespace ParaglidingFlightLogWeb.ViewModels;

/// <summary>
/// Flight model ready to be used by the views
/// </summary>
public class SharedFlightViewModel
{
    private const int INTEGRATION_STEP_S = 8;
    private readonly SharedFlight _flight;
    /// <summary>
    /// ctor
    /// </summary>
    /// <param name="flight"></param>
    public SharedFlightViewModel(SharedFlight flight)
    {
        _flight = flight;
    }
    /// <summary>
    /// Flight Id
    /// </summary>
    public string Id => _flight.Id;
    /// <summary>
    /// Take off time formatted for display
    /// </summary>
    public string TakeOffDateTime => _flight.TakeOffDateTime.ToString("g");
    /// <summary>
    /// Take off site name
    /// </summary>
    public string SiteName => _flight.SiteName;
    /// <summary>
    /// Glider name
    /// </summary>
    public string GliderName => _flight.GliderName;
    /// <summary>
    /// Get the max altitude in meters
    /// </summary>
    public double MaxAltitude
    {
        get
        {
            if (_flight.FlightPoints.Count == 0)
            {
                return 0;
            }

            return _flight.FlightPoints.Max(p => p.Altitude);
        }
    }
    
    /// <summary>
    /// Get the trace length in km
    /// </summary>
    public double TraceLength => ViewModelHelpers.GetTraceLength(_flight.FlightPoints);
    /// <summary>
    /// Get the maximum climb rate in m/s
    /// </summary>
    public double MaxClimb
    {
        get
        {
            double[] verticalRates = ViewModelHelpers.GetVerticalRate(_flight.FlightPoints, INTEGRATION_STEP_S);
            if (verticalRates.Length == 0)
            {
                return 0;
            }

            return verticalRates.Max();
        }
    }
    /// <summary>
    /// Get the maximum sink rate in m/s
    /// </summary>
    public double MaxSink
    {
        get
        {
            double[] verticalRates = ViewModelHelpers.GetVerticalRate(_flight.FlightPoints, INTEGRATION_STEP_S);
            if (verticalRates.Length == 0)
            {
                return 0;
            }

            return verticalRates.Min();
        }
    }

    private XcScoreViewModel? _xcScoreVm;
    /// <summary>
    /// Get the XC score view model
    /// </summary>
    public XcScoreViewModel? XcScore
    {
        get
        {
            if (_xcScoreVm is not null)
            {
                return _xcScoreVm;
            }

            XcScore? xcScore = ParagliderFlightLog.Models.XcScore.FromJson(_flight.GeoJsonScore);
            if (xcScore is null) { return null; }

            _xcScoreVm = new XcScoreViewModel(xcScore);

            return _xcScoreVm;
        }
    }
    /// <summary>
    /// Get the flight points
    /// </summary>
    public List<FlightPoint> FlightPoints => _flight.FlightPoints;
    /// <summary>
    /// Get the comment
    /// </summary>
    public string Comment => _flight.Comment ?? "";
    /// <summary>
    /// Get the take off date time data
    /// </summary>
    public DateTime TakeOffDateTimeData => _flight.TakeOffDateTime;
    /// <summary>
    /// Get the flight duration
    /// </summary>
    public TimeSpan FlightDuration => _flight.FlightDuration;

    /// <summary>
    /// Return the photo of the flights for the UI
    /// </summary>
    public List<FlightPhotoViewModel> GetFlightPhotos() =>
        _flight.Photos.Select(x => new FlightPhotoViewModel(x)).ToList();
}
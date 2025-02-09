namespace ParagliderFlightLog.Models;

public class SharedFlight
{
    /// <summary>
    /// Id
    /// </summary>
    public string Id { get; set; } = Guid.NewGuid().ToString();
    /// <summary>
    /// Comment
    /// </summary>
    public string? Comment { get; set; }

    /// <summary>
    /// Site Name
    /// </summary>
    public string SiteName { get; set; } = "Unknown";

    public string GliderName { get; set; } = "Unknown";
    /// <summary>
    /// Flight duration in seconds
    /// </summary>
    public int FlightDuration_s { get; set; }
    /// <summary>
    /// Flight duration
    /// </summary>
    public TimeSpan FlightDuration => TimeSpan.FromSeconds(FlightDuration_s);
    /// <summary>
    /// Content of the IGC file
    /// </summary>
    public string IgcFileContent { get; set; } = "";
    /// <summary>
    /// Score of the flight
    /// </summary>
    public string GeoJsonScore { get; set; } = "";
    /// <summary>
    /// End of share date time
    /// </summary>
    public DateTime EndOfShareDateTime { get; set; }
    /// <summary>
    /// Flight points
    /// </summary>
    public List<FlightPoint> FlightPoints { get; set; } = [];
    /// <summary>
    /// Time of take off
    /// </summary>
    public DateTime TakeOffDateTime { get; set; }
    /// <summary>
    /// Photos of the flight
    /// </summary>
    public List<FlightPhoto> Photos { get; set; } = [];
}
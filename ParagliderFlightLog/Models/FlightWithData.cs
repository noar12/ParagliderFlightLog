namespace ParagliderFlightLog.Models;
/// <summary>
/// Complete model for a flight
/// </summary>
public class FlightWithData
{
    /// <summary>
    /// Content of the IGC file as raw string
    /// </summary>
    public string IgcFileContent { get; set; } = "";
    /// <summary>
    ///  Comment
    /// </summary>
    public string Comment { get; set; } = "";
    /// <summary>
    /// ID
    /// </summary>
    public string Flight_ID { get; set; } = Guid.NewGuid().ToString();
    /// <summary>
    /// ID of the site where the flight takes off
    /// </summary>
    public string REF_TakeOffSite_ID { get; set; } = "";
    /// <summary>
    /// ID of the glider used for this flight
    /// </summary>
    public string REF_Glider_ID { get; set; } = "";
    /// <summary>
    /// Take off time
    /// </summary>
    public DateTime TakeOffDateTime { get; set; } = DateTime.MinValue;
    /// <summary>
    /// The flight duration as a TimeSpan based on the number of sample in the IGC File (1 sample per seconds) 
    /// or on the content of a backing field if no igc content is available
    /// </summary>
    public TimeSpan FlightDuration { get; set; }

    /// <summary>
    /// The flight duration as a number of second. This is mainly to store it easil in the sqlite db since it doesn't manage timespan
    /// </summary>
    public int FlightDuration_s { get; set; }
    /// <summary>
    /// The altitude of the take off if an igc content is available. NaN otherwise
    /// </summary>
    public double TakeOffAltitude { get; set; }

    /// <summary>
    /// The flightPoint of the take off if an igc content is available. NaN otherwise
    /// </summary>
    public FlightPoint TakeOffPoint { get; set; } = new();
    /// <summary>
    /// Flight points coordinates form the IGC file
    /// </summary>
    public List<FlightPoint> FlightPoints { get; set; } = [];
    /// <summary>
    /// Glider name as specified in the IGC file
    /// </summary>
    public string IGC_GliderName { get; set; } = "Unknown glider";
    /// <summary>
    /// Score of the flight
    /// </summary>
    public XcScore? XcScore { get; set; }
    /// <summary>
    /// Returns a Flight without some heavy data like the IGC content
    /// </summary>
    /// <returns></returns>
    public Flight ToFlight(){
        return new()
        {
            Flight_ID = Flight_ID,
            REF_TakeOffSite_ID = REF_TakeOffSite_ID,
            REF_Glider_ID = REF_Glider_ID,
            TakeOffDateTime = TakeOffDateTime,
            FlightDuration_s = (int)FlightDuration.TotalSeconds,
            Comment = Comment,
            XcScore = XcScore
        };
    }
}

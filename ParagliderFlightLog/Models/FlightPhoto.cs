namespace ParagliderFlightLog.Models;
/// <summary>
/// Represents a photo linked to a flight
/// </summary>
public class FlightPhoto
{
    /// <summary>
    /// ID
    /// </summary>
    public string ID { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// User Id associated with the photo
    /// </summary>
    public string REF_User_Id { get; set; } = "";

    /// <summary>
    /// Flight Id of the flight during whihc the photo has been taken
    /// </summary>
    public string REF_Flight_ID { get; set; } = "";
    /// <summary>
    /// actual photo data
    /// </summary>
    public MemoryStream? PhotoStream { get; set; }
    /// <summary>
    /// File name of the photo on the disk
    /// </summary>
    public string PhotoFileName => $"{ID}.jpg";
}
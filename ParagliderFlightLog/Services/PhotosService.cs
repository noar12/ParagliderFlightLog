using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ParagliderFlightLog.DataAccess;
using ParagliderFlightLog.Models;

namespace ParagliderFlightLog.Services;

/// <summary>
/// Manage the photos
/// </summary>
public class PhotosService
{
    private readonly ILogger<PhotosService> _logger;
    private readonly IConfiguration _config;

    /// <summary>
    /// Manage the photos
    /// </summary>
    /// <param name="logger"></param>
    /// <param name="config"></param>
    public PhotosService(ILogger<PhotosService> logger, IConfiguration config)
    {
        _logger = logger;
        _config = config;
    }

    /// <summary>
    /// Save the <paramref name="flightPhoto"/>
    /// </summary>
    /// <param name="flightPhoto"></param>
    /// <param name="db"></param>
    /// <param name="width"></param>
    /// <param name="quality"></param>
    public void SaveFlightPhoto(FlightPhoto flightPhoto, FlightLogDB db, int width = 1920, int quality = 90)
    {
        if (flightPhoto.PhotoStream == null)
        {
            _logger.LogWarning($"No flight photo available to save");
            return;
        }

        string flightPhotoPath = GetFlightPhotoPath(flightPhoto);
        Directory.CreateDirectory(Path.GetDirectoryName(flightPhotoPath)!);
        using FileStream fs = new (flightPhotoPath, FileMode.Create);
        flightPhoto.PhotoStream.Seek(0, SeekOrigin.Begin);
        flightPhoto.PhotoStream.CopyTo(fs);
        db.WriteFlightPhoto(flightPhoto);
        _logger.LogInformation("Saving photo to {PhotoPath} for {User}", flightPhotoPath, flightPhoto.REF_User_Id);
        
    }
    
    public string GetFlightPhotoPath(FlightPhoto flightPhoto)
    {
        string output = _config.GetValue<string>("UserDirectory:Root") ?? "";
        string userFlightPhotoDirName = _config.GetValue<string>("UserDirectory:RelativeFlightPhotos") ?? "";
        output = output.Replace("{UserId}", flightPhoto.REF_User_Id);
        output = Path.Combine(output, userFlightPhotoDirName, flightPhoto.REF_Flight_ID, flightPhoto.PhotoFileName);
        return output;
    }
}
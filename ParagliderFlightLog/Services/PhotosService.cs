using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ParagliderFlightLog.DataAccess;
using ParagliderFlightLog.Models;
using CliWrap;
using CliWrap.Buffered;

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
    /// Save and resize the <paramref name="flightPhoto"/>
    /// </summary>
    /// <param name="flightPhoto"></param>
    /// <param name="db"></param>
    /// <param name="width"></param>
    /// <param name="height"></param>
    public async Task SaveFlightPhoto(FlightPhoto flightPhoto, FlightLogDB db, int width = 1920, int height = 1080)
    {
        if (flightPhoto.PhotoStream == null)
        {
            _logger.LogWarning($"No flight photo available to save");
            return;
        }

        string? flightPhotoPath = GetFlightPhotoPath(flightPhoto);
        
        Directory.CreateDirectory(Path.GetDirectoryName(flightPhotoPath)!);
        await using FileStream fs = new (flightPhotoPath, FileMode.Create);
        flightPhoto.PhotoStream.Seek(0, SeekOrigin.Begin);
        await flightPhoto.PhotoStream.CopyToAsync(fs);
        
        if (!await ResizePhoto(width, height, flightPhotoPath))
        {
            return;
        }

        db.WriteFlightPhoto(flightPhoto);
        _logger.LogInformation("Saving photo to {PhotoPath} for {User}", flightPhotoPath, flightPhoto.REF_User_Id);
    }

    private async Task<bool> ResizePhoto(int width, int height, string flightPhotoPath)
    {
        if (!File.Exists(flightPhotoPath))
        {
            _logger.LogError("Flight photo at {Path} does not exist, cannot resize", flightPhotoPath);
            return false;
        }
        var result = await Cli.Wrap("gm")
            .WithArguments($"convert -size {width}x{height} {flightPhotoPath} -resize {width}x{height} {flightPhotoPath}")
            .WithValidation(CommandResultValidation.None)
            .ExecuteBufferedAsync();
        if (result.ExitCode != 0)
        {
            _logger.LogError("Failed to convert flight photo to {Path} with exit code {Code}. Error was : {Error}",
                flightPhotoPath,result.ExitCode, result.StandardError);
            return false;
        }

        return true;
    }

    /// <summary>
    /// Return the path to the photo using the config info the <paramref name="flightPhoto"/>'s info or null if not found
    /// </summary>
    /// <param name="flightPhoto"></param>
    /// <returns></returns>
    public string GetFlightPhotoPath(FlightPhoto flightPhoto)
    {
        string output = _config.GetValue<string>("UserDirectory:Root") ?? "";
        string userFlightPhotoDirName = _config.GetValue<string>("UserDirectory:RelativeFlightPhotos") ?? "";
        output = output.Replace("{UserId}", flightPhoto.REF_User_Id);
        output = Path.Combine(output, userFlightPhotoDirName, flightPhoto.REF_Flight_ID, flightPhoto.PhotoFileName);

        return output;
    }
}
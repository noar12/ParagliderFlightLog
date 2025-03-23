using ParagliderFlightLog.DataAccess;
using ParagliderFlightLog.Models;
using ParagliderFlightLog.Services;
using ParaglidingFlightLogWeb.ViewModels;

namespace ParaglidingFlightLogWeb.Services;
/// <summary>
/// Service to allow sharing aspect of an account (flights, sites,...) to the web
/// </summary>
public class SharingService
{
    private readonly ILogger<SharingService> _logger;
    private readonly SharedDb _db;
    private readonly PhotosService _photosService;

    /// <summary>
    /// ctor
    /// </summary>
    /// <param name="logger"></param>
    /// <param name="db"></param>
    /// <param name="photosService"></param>
    public SharingService(ILogger<SharingService> logger, SharedDb db, PhotosService photosService)
    {
        _logger = logger;
        _db = db;
        _photosService = photosService;
    }

    /// <summary>
    /// Create a new flight based on <paramref name="flightVm" /> that will be accessible to other to share. Returns the URI to the share flight
    /// </summary>
    /// <param name="flightVm"></param>
    /// <param name="userId"></param>
    /// <param name="comment"></param>
    /// <param name="validity"></param>
    /// <param name="baseUrl"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public async Task<Uri> ShareFlightAsync(FlightViewModel flightVm, string userId, string comment, TimeSpan validity, Uri baseUrl)
    {
        _logger.LogInformation("Sharing flight {FlightId} for {Validity}", flightVm.FlightID, validity);
        FlightWithData flight = flightVm.FlightWithData;
        string siteName = flightVm.TakeOffSiteName;
        string gliderName = flightVm.GliderName;
        string id = await _db.CreateSharedFlightAsync(flight, userId, comment, siteName, gliderName, flight.FlightPhotos, validity);
        var uri = new Uri(baseUrl, $"sharedflight/{id}");
        return uri;
    }
    /// <summary>
    /// Get the shared flight based on the <paramref name="flightId"/>
    /// </summary>
    /// <param name="flightId"></param>
    /// <returns></returns>
    public async Task<SharedFlightViewModel?> GetSharedFlightAsync(string flightId)
    {
        SharedFlight? flight = await _db.GetSharedFlightAsync(flightId);
        return flight is null ? null : new SharedFlightViewModel(flight);
    }
    /// <summary>
    /// Return the base 64 encoded string of the image
    /// </summary>
    /// <param name="photo"></param>
    /// <returns></returns>
    public string GetBase64StringPhotoData(FlightPhotoViewModel photo)
    {
        return photo.GetBase64PhotoData(_photosService);
    }
}
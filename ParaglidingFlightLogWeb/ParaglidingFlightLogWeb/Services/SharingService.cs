using ParagliderFlightLog.DataAccess;
using ParagliderFlightLog.Models;
using ParaglidingFlightLogWeb.ViewModels;

namespace ParaglidingFlightLogWeb.Services;
/// <summary>
/// Service to allow sharing aspect of an account (flights, sites,...) to the web
/// </summary>
public class SharingService
{
    private readonly ILogger<SharingService> _logger;
    private readonly SharedDb _db;

    /// <summary>
    /// ctor
    /// </summary>
    /// <param name="logger"></param>
    /// <param name="db"></param>
    public SharingService(ILogger<SharingService> logger, SharedDb db)
    {
        _logger = logger;
        _db = db;
    }

    /// <summary>
    /// Create a new flight based on <param name="flightVm" /> that will be accessible to other to share. Returns the URI to the share flight
    /// </summary>
    /// <param name="flightVm"></param>
    /// <param name="validity"></param>
    /// <param name="baseUrl"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public async Task<Uri> ShareFlightAsync(FlightViewModel flightVm, TimeSpan validity, Uri baseUrl)
    {
        _logger.LogInformation("Sharing flight {FlightId} for {Validity}", flightVm.FlightID, validity);
        FlightWithData flight = flightVm.FlightWithData;
        string siteName = flightVm.TakeOffSiteName;
        string gliderName = flightVm.GliderName;
        string id = await _db.CreateSharedFlightAsync(flight, siteName, gliderName, validity);
        var uri = new Uri(baseUrl, $"shared/{id}");
        return uri;
    }
}
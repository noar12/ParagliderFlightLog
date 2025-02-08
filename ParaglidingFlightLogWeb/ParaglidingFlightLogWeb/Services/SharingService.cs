using ParagliderFlightLog.DataAccess;
using ParagliderFlightLog.Models;
using ParaglidingFlightLogWeb.ViewModels;

namespace ParaglidingFlightLogWeb.Services;
/// <summary>
/// Service to allow sharing aspect of an account (flights, sites,...) to the web
/// </summary>
public class SharingService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<SharingService> _logger;
    private readonly SharedDb _db;
    private readonly IWebHostEnvironment _env;

    /// <summary>
    /// ctor
    /// </summary>
    /// <param name="configuration"></param>
    /// <param name="logger"></param>
    /// <param name="db"></param>
    /// <param name="env"></param>
    public SharingService(IConfiguration configuration, ILogger<SharingService> logger, SharedDb db, IWebHostEnvironment env)
    {
        _configuration = configuration;
        _logger = logger;
        _db = db;
        _env = env;
    }
    /// <summary>
    /// Create a new flight based on <param name="flight" /> that will be accessible to other to share. Returns the URI to the share flight
    /// </summary>
    /// <param name="flight"></param>
    /// <param name="validity"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public async Task<Uri> ShareFlightAsync(FlightViewModel flightVm, TimeSpan validity, Uri baseUrl)
    {
        FlightWithData flight = flightVm.FlightWithData;
        string id = await _db.CreateSharedFlightAsync(flight, validity);
        var uri = new Uri(baseUrl, $"shared/{id}");
        return uri;
    }
}
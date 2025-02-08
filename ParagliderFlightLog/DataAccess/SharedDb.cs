using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ParagliderFlightLog.Models;

namespace ParagliderFlightLog.DataAccess;

/// <summary>
/// Manage data access for the shared item
/// </summary>
public class SharedDb
{
    private readonly SqliteDataAccess _db;
    private readonly IConfiguration _config;
    private readonly ILogger<SharedDb> _logger;
    private bool _isInitialized;

    public SharedDb(IConfiguration config, ILogger<SharedDb> logger, SqliteDataAccess dbAccess)
    {
        _db = dbAccess;
        _config = config;
        _logger = logger;
    }

    private async Task InitAsync()
    {
        throw new NotImplementedException();
        _isInitialized = true;
    }

    /// <summary>
    /// Create a flight to share with the web. Returns a guid to identify the created flight
    /// </summary>
    /// <param name="flight"></param>
    /// <returns></returns>
    public async Task<string> CreateSharedFlightAsync(FlightWithData flight, TimeSpan validity)
    {
        if (!_isInitialized) { await InitAsync(); }

        throw new NotImplementedException();
    }

    /// <summary>
    /// Get the flight based on the <paramref name="flightId"/>
    /// </summary>
    /// <param name="flightId"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public async Task<FlightWithData> GetSharedFlightAsync(string flightId)
    {
        if (!_isInitialized) { await InitAsync(); }
        throw new NotImplementedException();
    }
}
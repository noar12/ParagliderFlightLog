using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ParagliderFlightLog.Helpers;
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

    /// <summary>
    /// ctor
    /// </summary>
    /// <param name="config"></param>
    /// <param name="logger"></param>
    /// <param name="dbAccess"></param>
    public SharedDb(IConfiguration config, ILogger<SharedDb> logger, SqliteDataAccess dbAccess)
    {
        _db = dbAccess;
        _config = config;
        _logger = logger;
    }

    private async Task InitAsync()
    {
        if (!SqliteDataAccess.DbExists(LoadConnectionString()))
        {
            string? dbDir =
                Path.GetDirectoryName(SqliteDataAccess.GetDbPathFromConnectionString(LoadConnectionString()));
            if (dbDir == null)
            {
                _logger.LogError("Unable to locate the database directory.");
                return;
            }

            Directory.CreateDirectory(dbDir);
            await CreateSharedDbAsync();
        }
        else
        {
            var dbInfo = await GetDbInformationsAsync();
            if (dbInfo is null)
            {
                _logger.LogError("Unable to get the database informations.");
                return;
            }

            if (dbInfo is { VersionMajor: 1, VersionMinor: 0, VersionFix: 0 })
            {
                MigrateFromv1_0_0();
            }

            await CleanOldSharedFlightsAsync();
        }

        _isInitialized = true;
    }

    private void MigrateFromv1_0_0()
    {
        _logger.LogInformation("Migrating from v1.0.0 to v1.1.0");
        string sqlCreateSharedFlightStatistics = """
                                                 BEGIN TRANSACTION;
                                                 CREATE TABLE "SharedFlightStatistics" (
                                                     "Id" TEXT NOT NULL UNIQUE,
                                                     "REF_SharedFlight_ID" TEXT NOT NULL,
                                                     "REF_SourceFlight_ID" TEXT NOT NULL,
                                                     "REF_User_Id" TEXT NOT NULL,
                                                     "ViewCount" INTEGER NOT NULL,
                                                     "LastViewDateTime" TEXT NOT NULL,
                                                     PRIMARY KEY("Id"));
                                                 UPDATE DbInformations SET VersionMinor = 1;
                                                 COMMIT;
                                                 """;
        _db.SaveDataAsync(sqlCreateSharedFlightStatistics, new { }, LoadConnectionString()).Wait();
        _logger.LogInformation("Successfully migrated to v1.1.0");
    }

    private async Task CleanOldSharedFlightsAsync()
    {
        const string sqlGetOldFlights = "SELECT * FROM SharedFlights WHERE EndOfShareDateTime < @now";
        List<SharedFlight> oldFlights = await _db.LoadDataAsync<SharedFlight, dynamic>(sqlGetOldFlights,
            new { now = DateTime.UtcNow }, LoadConnectionString());

        foreach (SharedFlight flight in oldFlights)
        {
            _logger.LogInformation("Shared Flight {flightId} is expired", flight.Id);
            await DeleteFlightAsync(flight);
        }
    }

    private async Task CreateSharedDbAsync()
    {
        const string sqlCreateDbInfo = """
                                       CREATE TABLE "DbInformations" (
                                           "VersionMajor"	INTEGER NOT NULL,
                                           "VersionMinor"	INTEGER NOT NULL,
                                           "VersionFix"	INTEGER NOT NULL
                                       );
                                       """;

        const string sqlCreateSharedFlights = """
                                              CREATE TABLE "SharedFlights" (
                                              	"Id"	TEXT NOT NULL UNIQUE,
                                              	"SourceFlightId"	TEXT NOT NULL UNIQUE,
                                              	"Comment"	TEXT,
                                              	"SiteName"	TEXT NOT NULL,
                                              	"GliderName"	REAL NOT NULL,
                                              	"FlightDuration_s"	INTEGER NOT NULL,
                                              	"TakeOffDateTime"	TEXT NOT NULL,
                                              	"IgcFileContent"	TEXT NOT NULL,
                                              	"GeoJsonScore"	TEXT,
                                              	"EndOfShareDateTime"	TEXT NOT NULL,
                                              	PRIMARY KEY("Id")
                                              );
                                              """;
        string sqlCreatePhotosTable = """
                                      CREATE TABLE "FlightPhotos" (
                                          "Photo_ID" TEXT NOT NULL UNIQUE,
                                          "REF_Flight_ID"    TEXT NOT NULL,
                                          "REF_User_Id"    TEXT NOT NULL,
                                          "REF_SharedFlight_ID"    TEXT,
                                          PRIMARY KEY("Photo_ID"),
                                          FOREIGN KEY("REF_SharedFlight_ID") REFERENCES "SharedFlights"("Id"));
                                      """;
        string sqlCreateSharedFlightStatistics = """
                                                 CREATE TABLE "SharedFlightStatistics" (
                                                     "Id" TEXT NOT NULL UNIQUE,
                                                     "REF_SharedFlight_ID" TEXT NOT NULL,
                                                     "REF_SourceFlight_ID" TEXT NOT NULL,
                                                     "REF_User_Id" TEXT NOT NULL,
                                                     "ViewCount" INTEGER NOT NULL,
                                                     "LastViewDateTime" TEXT NOT NULL,
                                                     PRIMARY KEY("Id"));
                                                 """;
        await _db.SaveDataAsync(sqlCreateDbInfo, new { }, LoadConnectionString());
        await _db.SaveDataAsync(sqlCreateSharedFlights, new { }, LoadConnectionString());
        await _db.SaveDataAsync(sqlCreatePhotosTable, new { }, LoadConnectionString());
        await _db.SaveDataAsync(sqlCreateSharedFlightStatistics, new { }, LoadConnectionString());

        DbInformations dbInformations = new() { VersionMajor = 1, VersionMinor = 1, VersionFix = 0, };
        const string sqlInsertDbInfo = """
                                       INSERT INTO DbInformations (VersionMajor, VersionMinor, VersionFix)
                                       VALUES (@VersionMajor, @VersionMinor, @VersionFix);
                                       """;
        await _db.SaveDataAsync(sqlInsertDbInfo, dbInformations, LoadConnectionString());
        _logger.LogInformation("Shared Database created");
    }

    /// <summary>
    /// Create a flight to share with the web. Returns a guid to identify the created flight
    /// </summary>
    /// <param name="flight"></param>
    /// <param name="comment"></param>
    /// <param name="siteName"></param>
    /// <param name="gliderName"></param>
    /// <param name="validity"></param>
    /// <returns></returns>
    public async Task<string> CreateSharedFlightAsync(FlightWithData flight, string userId, string comment,
        string siteName,
        string gliderName,
        List<FlightPhoto> photos, TimeSpan validity)
    {
        if (!_isInitialized) { await InitAsync(); }

        string flightToShareId = await IsThisFlightSharedAsync(flight.Flight_ID);
        if (!string.IsNullOrEmpty(flightToShareId)) { return flightToShareId; }

        DateTime endOfShare = DateTime.UtcNow + validity;
        SharedFlight flightToShare = new()
        {
            Comment = comment,
            SourceFlightId = flight.Flight_ID,
            FlightDuration_s = flight.FlightDuration_s,
            IgcFileContent = flight.IgcFileContent,
            GeoJsonScore = flight.XcScore?.GeoJsonText ?? "",
            SiteName = siteName,
            GliderName = gliderName,
            EndOfShareDateTime = endOfShare,
            TakeOffDateTime = flight.TakeOffDateTime,
        };
        const string sqlInsertFlight = """
                                       INSERT INTO SharedFlights (Id, SourceFlightId, Comment, SiteName, GliderName, FlightDuration_s, TakeOffDateTime, IgcFileContent, GeoJsonScore, EndOfShareDateTime)
                                       VALUES (@Id, @SourceFlightId, @Comment, @SiteName, @GliderName, @FlightDuration_s, @TakeOffDateTime, @IgcFileContent, @GeoJsonScore, @EndOfShareDateTime);
                                       """;
        const string sqlInsertPhoto = """
                                      INSERT INTO FlightPhotos (Photo_ID, REF_Flight_ID, REF_User_Id, REF_SharedFlight_ID)
                                      VALUES (@Photo_ID, @REF_Flight_ID, @REF_User_Id, @REF_SharedFlight_ID);
                                      """;
        await _db.SaveDataAsync(sqlInsertFlight, flightToShare, LoadConnectionString());
        foreach (FlightPhoto photo in photos)
        {
            await _db.SaveDataAsync(sqlInsertPhoto,
                new
                {
                    photo.Photo_ID,
                    REF_Flight_ID = flightToShare.SourceFlightId,
                    photo.REF_User_Id,
                    REF_SharedFlight_ID = flightToShare.Id
                }, LoadConnectionString());
        }

        await CreateSharedFlightStatistic(flightToShare, userId);
        return flightToShare.Id;
    }

    /// <summary>
    /// Get the flight based on the <paramref name="flightId"/>
    /// </summary>
    /// <param name="flightId"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public async Task<SharedFlight?> GetSharedFlightAsync(string flightId)
    {
        if (!_isInitialized) { await InitAsync(); }

        const string sql = "SELECT * FROM SharedFlights WHERE Id=@flightId";
        List<SharedFlight> sharedFlights =
            await _db.LoadDataAsync<SharedFlight, dynamic>(sql, new { flightId }, LoadConnectionString());
        if (sharedFlights.Count > 1) { throw new InvalidOperationException("Multiple flights with the same id"); }

        if (sharedFlights.Count == 0) { return null; }

        SharedFlight flight = sharedFlights[0];
        if (flight.EndOfShareDateTime < DateTime.UtcNow)
        {
            await DeleteFlightAsync(flight);
            return null;
        }

        flight.FlightPoints = IgcHelper.GetFlightPointsFromIgcContent(sharedFlights[0].IgcFileContent);
        flight.Photos = await GetAllPhotoForFlightAsync(flight.Id);
        await IncrementGetCount(flight);
        return flight;
    }

    private async Task IncrementGetCount(SharedFlight flight)
    {
        string sqlGetFlightStatistics = """
                                        SELECT ViewCount 
                                        FROM SharedFlightStatistics
                                        WHERE REF_SharedFlight_ID=@Id;
                                        """;
        List<int> viewCount =
            await _db.LoadDataAsync<int, dynamic>(sqlGetFlightStatistics, new { flight.Id }, LoadConnectionString());
        if (viewCount.Count == 0)
        {
            _logger.LogError("No statistic available for shared flight {id}", flight.Id);
            return;
        }
        else if (viewCount.Count == 1)
        {
            const string sqlUpdateFlightStatistics = """
                                                     UPDATE SharedFlightStatistics
                                                     SET ViewCount=@ViewCount, LastViewDateTime=@LastViewDateTime
                                                     WHERE REF_SharedFlight_ID=@Id;
                                                     """;
            await _db.SaveDataAsync(sqlUpdateFlightStatistics,
                new { ViewCount = viewCount[0] + 1, LastViewDateTime = DateTime.UtcNow, flight.Id },
                LoadConnectionString());
        }
        else
        {
            _logger.LogError("Multiple statistics available for shared flight {id}", flight.Id);
        }
    }

    /// <summary>
    /// Create a statistic for the shared flight. Initialize the count value to 0
    /// </summary>
    /// <param name="flight"></param>
    /// <param name="userId"></param>
    private async Task CreateSharedFlightStatistic(SharedFlight flight, string userId)
    {
        string sqlCreateStatistic = """
                                       INSERT INTO SharedFlightStatistics (Id, REF_SharedFlight_ID, REF_SourceFlight_ID, REF_User_Id, ViewCount, LastViewDateTime)
                                       VALUES (@Id, @REF_SharedFlight_ID, @REF_SourceFlight_ID, @REF_User_Id, @ViewCount, @LastViewDateTime);
                                    """;
        await _db.SaveDataAsync(sqlCreateStatistic,
            new
            {
                Id = Guid.NewGuid().ToString(),
                REF_SharedFlight_ID = flight.Id,
                REF_SourceFlight_ID = flight.SourceFlightId,
                REF_User_Id = userId,
                ViewCount = 0,
                LastViewDateTime = DateTime.UtcNow
            }, LoadConnectionString());
    }

    private async Task DeleteFlightAsync(SharedFlight flight)
    {
        const string deleteFlight = "DELETE FROM SharedFlights WHERE Id=@Id";
        const string deletePhotos = "DELETE FROM FlightPhotos WHERE REF_SharedFlight_ID=@Id";

        await _db.SaveDataAsync(deletePhotos, new { flight.Id }, LoadConnectionString());
        await _db.SaveDataAsync(deleteFlight, new { flight.Id }, LoadConnectionString());
        _logger.LogInformation("Shared Flight {flightId} deleted", flight.Id);
    }

    private async Task<string> IsThisFlightSharedAsync(string flightId)
    {
        if (!_isInitialized) { await InitAsync(); }

        const string sql = "SELECT Id FROM SharedFlights WHERE SourceFlightId=@flightId";
        List<string> sharedFlightId =
            await _db.LoadDataAsync<string, dynamic>(sql, new { flightId }, LoadConnectionString());

        if (sharedFlightId.Count > 0)
        {
            return sharedFlightId[0];
        }

        return "";
    }

    private string LoadConnectionString(string connectionStringName = "SharedDataSqlite")
    {
        string? cs = _config.GetConnectionString(connectionStringName);
        if (cs is null) { throw new ArgumentNullException(cs, "Connection string not found"); }

        return cs;
    }

    private async Task<DbInformations?> GetDbInformationsAsync()
    {
        string dbInfoTable = "DbInformations";
        string sql = "SELECT name FROM sqlite_master WHERE type='table' AND name=@dbInfoTable";
        bool dbInfoExists = (await _db.LoadDataAsync<string, dynamic>(sql, new { dbInfoTable }, LoadConnectionString()))
            .Count == 1;
        if (!dbInfoExists) return null;
        sql = "SELECT VersionMajor,VersionMinor, VersionFix FROM DbInformations";
        return (await _db.LoadDataAsync<DbInformations, dynamic>(sql, new { }, LoadConnectionString()))[0];
    }

    /// <summary>
    /// Get the meta data of all the photo for the <paramref name="flight"/>
    /// </summary>
    /// <param name="flight"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    private async Task<List<FlightPhoto>> GetAllPhotoForFlightAsync(string flightId)
    {
        string sql = """
                     SELECT Photo_ID, REF_Flight_ID, REF_User_Id
                     FROM FlightPhotos
                     WHERE REF_SharedFlight_ID = @flightId;
                     """;
        var output = await _db.LoadDataAsync<FlightPhoto, dynamic>(sql, new { flightId }, LoadConnectionString());

        return output;
    }

    public async Task<List<SharedFlightStatistic>> GetUserSharedFlightStatisticsAsync(string userId)
    {
        string sql = """
                     SELECT Id, REF_SharedFlight_ID, REF_SourceFlight_ID, REF_User_Id, ViewCount, LastViewDateTime
                     FROM SharedFlightStatistics
                     WHERE REF_User_Id=@userId;
                     """;
        var output = await _db.LoadDataAsync<SharedFlightStatistic, dynamic>(sql, new { userId }, LoadConnectionString());
        return output;
    }
}
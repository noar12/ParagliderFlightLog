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
            DbInformations? dbInfo = GetDbInformations();
            if (dbInfo is null)
            {
                _logger.LogError("Unable to get the database informations.");
                return;
            }
        }

        _isInitialized = true;
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
                                          PRIMARY KEY("Photo_ID"),
                                          FOREIGN KEY("REF_Flight_ID") REFERENCES "SharedFlights"("Id"));
                                      """;
        await _db.SaveDataAsync(sqlCreateDbInfo, new { }, LoadConnectionString());
        await _db.SaveDataAsync(sqlCreateSharedFlights, new { }, LoadConnectionString());
        await _db.SaveDataAsync(sqlCreatePhotosTable, new { }, LoadConnectionString());

        DbInformations dbInformations = new() { VersionMajor = 1, VersionMinor = 0, VersionFix = 0, };
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
    public async Task<string> CreateSharedFlightAsync(FlightWithData flight, string comment, string siteName,
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
                                      INSERT INTO FlightPhotos (Photo_ID, REF_Flight_ID, REF_User_Id)
                                      VALUES (@Photo_ID, @REF_Flight_ID, @REF_User_Id);
                                      """;
        await _db.SaveDataAsync(sqlInsertFlight, flightToShare, LoadConnectionString());
        foreach (FlightPhoto photo in photos)
        {
            await _db.SaveDataAsync(sqlInsertPhoto,
                new { photo.Photo_ID, @REF_Flight_ID = flightToShare.Id, photo.REF_User_Id }, LoadConnectionString());
        }

        return flightToShare.Id;
    }

    /// <summary>
    /// Get the flight based on the <paramref name="flightId"/>
    /// </summary>
    /// <param name="flightId"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public async Task<SharedFlight> GetSharedFlightAsync(string flightId)
    {
        if (!_isInitialized) { await InitAsync(); }

        const string sql = "SELECT * FROM SharedFlights WHERE Id=@flightId";
        List<SharedFlight> sharedFlights =
            _db.LoadData<SharedFlight, dynamic>(sql, new { flightId }, LoadConnectionString());
        if (sharedFlights.Count > 1) { throw new InvalidOperationException("Multiple flights with the same id"); }

        sharedFlights[0].FlightPoints = IgcHelper.GetFlightPointsFromIgcContent(sharedFlights[0].IgcFileContent);
        sharedFlights[0].Photos = GetAllPhotoForFlight(flightId);
        return sharedFlights[0];
    }

    private async Task<string> IsThisFlightSharedAsync(string flightId)
    {
        if (!_isInitialized) { await InitAsync(); }

        const string sql = "SELECT Id FROM SharedFlights WHERE SourceFlightId=@flightId";
        List<string> sharedFlightId =
            _db.LoadData<string, dynamic>(sql, new { flightId }, LoadConnectionString());

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

    private DbInformations? GetDbInformations()
    {
        string dbInfoTable = "DbInformations";
        string sql = "SELECT name FROM sqlite_master WHERE type='table' AND name=@dbInfoTable";
        bool dbInfoExists = _db.LoadData<string, dynamic>(sql, new { dbInfoTable }, LoadConnectionString()).Count == 1;
        if (!dbInfoExists) return null;
        sql = "SELECT VersionMajor,VersionMinor, VersionFix FROM DbInformations";
        return _db.LoadData<DbInformations, dynamic>(sql, new { }, LoadConnectionString())[0];
    }

    /// <summary>
    /// Get the meta data of all the photo for the <paramref name="flight"/>
    /// </summary>
    /// <param name="flight"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    private List<FlightPhoto> GetAllPhotoForFlight(string flightId)
    {
        string sql = """
                     SELECT Photo_ID, REF_Flight_ID, REF_User_Id
                     FROM FlightPhotos
                     WHERE REF_FLIGHT_ID = @flightId;
                     """;
        var output = _db.LoadData<FlightPhoto, dynamic>(sql, new { flightId }, LoadConnectionString());

        return output;
    }
}
using System.Text.RegularExpressions;
using System.Collections;
using ParagliderFlightLog.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ParagliderFlightLog.Helpers;
using System.Diagnostics;

namespace ParagliderFlightLog.DataAccess;

/// <summary>
/// Data access class
/// </summary>
public class FlightLogDB
{
    private readonly SqliteDataAccess _db;
    private readonly IConfiguration _config;
    private readonly ILogger<FlightLogDB> _logger;
    /// <summary>
    /// The UserId of the user this instance is scoped for
    /// </summary>
    public string? UserId { get; private set; }

    /// <summary>
    /// Data access class
    /// </summary>
    /// <param name="config"></param>
    /// <param name="logger"></param>
    /// <param name="dbAccess"></param>
    public FlightLogDB(IConfiguration config, ILogger<FlightLogDB> logger, SqliteDataAccess dbAccess)
    {
        _db = dbAccess;
        _config = config;
        _logger = logger;
    }

    /// <summary>
    /// Init the data class for a specific <paramref name="userId"/>
    /// </summary>
    /// <param name="userId"></param>
    public void Init(string userId)
    {
        UserId = userId;
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
            CreateFlightLogDB();
        }
        else
        {
            var dbInfo = GetDbInformations();
            if (dbInfo == null)
            {
                MigrateFromBetaTable();
            }
            else if (dbInfo is { VersionMajor: 1, VersionMinor: 0, VersionFix: 0 })
            {
                MigrateFromv1_0_0();
            }
            else if (dbInfo is { VersionMajor: 1, VersionMinor: 1, VersionFix: 0 })
            {
                MigrateFromv1_1_0();
            }
            else if (dbInfo is { VersionMajor: 1, VersionMinor: 2, VersionFix: 0 })
            {
                MigrateFromv1_2_0();
            }
            else if (dbInfo is { VersionMajor: 1, VersionMinor: 3, VersionFix: 0 })
            {
                MigrateFromv1_3_0();
            }
            else if (dbInfo is { VersionMajor: 1, VersionMinor: 4, VersionFix: 0 })
            {
                _logger.LogInformation("Database is up to date.");
            }
            else
            {
                _logger.LogError("Database version {VersionMajor}.{VersionMinor}.{VersionFix} is not supported.",
                    dbInfo.VersionMajor, dbInfo.VersionMinor, dbInfo.VersionFix);
            }
        }
    }

    private void MigrateFromv1_3_0()
    {
        _logger.LogInformation("Migrating db from v1.3.0");
        const string sqlAddForeignKeyInFlights = """
                                            PRAGMA foreign_keys=off;
                                            CREATE TABLE "Flights_new" (
                                                "Flight_ID" TEXT NOT NULL UNIQUE,
                                                "Comment"   TEXT,
                                                "REF_TakeOffSite_ID"    TEXT NOT NULL,
                                                "REF_Glider_ID" TEXT NOT NULL,
                                                "FlightDuration_s"    INTEGER,
                                                "TakeOffDateTime"    TEXT,
                                                "IgcFileContent"    TEXT,
                                                "GeoJsonScore"   TEXT,
                                                PRIMARY KEY("Flight_ID"),
                                                FOREIGN KEY("REF_TakeOffSite_ID") REFERENCES "Sites"("Site_ID"),
                                                FOREIGN KEY("REF_Glider_ID") REFERENCES "Gliders"("Glider_ID"));
                                            INSERT INTO Flights_new (Flight_ID, Comment, REF_TakeOffSite_ID, REF_Glider_ID, FlightDuration_s, TakeOffDateTime, IgcFileContent, GeoJsonScore)
                                            SELECT Flight_ID, Comment, REF_TakeOffSite_ID, REF_Glider_ID, FlightDuration_s, TakeOffDateTime, IgcFileContent, GeoJsonScore
                                            FROM Flights;
                                            DROP TABLE Flights;
                                            ALTER TABLE Flights_new RENAME TO Flights;
                                            PRAGMA foreign_keys=on;
                                            UPDATE DbInformations SET VersionMinor = 4;
                                            VACUUM;
                                            """;
        const string sqlAddFlightObjectiveColumn = """
                                            ALTER TABLE Flights ADD COLUMN Objective TEXT;
                                            UPDATE DbInformations SET VersionMinor = 4;
                                            """;
        _db.SaveData(sqlAddForeignKeyInFlights, new { }, LoadConnectionString());
        _db.SaveData(sqlAddFlightObjectiveColumn, new { }, LoadConnectionString());
        _logger.LogInformation("Migrated db from v1.3.0 to v1.4.0");
    }

    private void MigrateFromv1_2_0()
    {
        _logger.LogInformation("Migrating db from v1.2.0");
        const string sqlCreatePhotosTable = """
                                            BEGIN TRANSACTION;
                                            CREATE TABLE "FlightPhotos" (
                                                "Photo_ID" TEXT NOT NULL UNIQUE,
                                                "REF_Flight_ID"    TEXT NOT NULL,
                                                PRIMARY KEY("Photo_ID"),
                                                FOREIGN KEY("REF_Flight_ID") REFERENCES "Flights"("Flight_ID"));
                                            UPDATE DbInformations SET VersionMinor = 3;
                                            COMMIT;
                                            """;
        _db.SaveData(sqlCreatePhotosTable, new { }, LoadConnectionString());
        _logger.LogInformation("Migrated db from v1.2.0 to v1.3.0");
    }

    private void MigrateFromv1_1_0()
    {
        _logger.LogInformation("Migrating db from v1.1.0");
        const string sql = """
                           BEGIN TRANSACTION;
                           ALTER TABLE Flights ADD GeoJsonScore TEXT;
                           UPDATE DbInformations SET VersionMinor = 2;
                           COMMIT;
                           """; // no where clause because there suppose to be only one row
        _db.SaveData(sql, new { _userId = UserId }, LoadConnectionString());
        MigrateFromv1_2_0();
    }

    private void MigrateFromv1_0_0()
    {
        _logger.LogInformation("Migrating db from v1.0.0");
        const string sql = """
                           BEGIN TRANSACTION;
                           ALTER TABLE DbInformations ADD UserId TEXT;
                           UPDATE DbInformations SET VersionMinor = 1, UserId = @_userId;
                           COMMIT;
                           """; // no where clause because there suppose to be only one row
        _db.SaveData(sql, new { _userId = UserId }, LoadConnectionString());
        MigrateFromv1_1_0();
    }

    private void MigrateFromBetaTable()
    {
        const string sqlCreateDbInfo = """
                                       CREATE TABLE "DbInformations" (
                                           "VersionMajor"	INTEGER NOT NULL,
                                           "VersionMinor"	INTEGER NOT NULL,
                                           "VersionFix"	INTEGER NOT NULL,
                                           "UserId"	TEXT NOT NULL
                                       );
                                       """;
        _db.SaveData(sqlCreateDbInfo, new { }, LoadConnectionString());
        var dbInfo = new DbInformations() { VersionMajor = 1, VersionMinor = 0, VersionFix = 0, UserId = UserId! };
        string sql =
            "INSERT INTO DbInformations (VersionMajor, VersionMinor, VersionFix, UserId) VALUES (@VersionMajor, @VersionMinor, @VersionFix, @UserId);";
        _db.SaveData(sql, dbInfo, LoadConnectionString());
        MigrateFromv1_0_0();
    }

    private DbInformations? GetDbInformations()
    {
        string dbInfoTable = "DbInformations";
        string sql = "SELECT name FROM sqlite_master WHERE type='table' AND name=@dbInfoTable";
        bool dbInfoExists = _db.LoadData<string, dynamic>(sql, new { dbInfoTable }, LoadConnectionString()).Count == 1;
        if (!dbInfoExists) { return null; }
        sql = "SELECT VersionMajor,VersionMinor, VersionFix FROM DbInformations";
        return _db.LoadData<DbInformations, dynamic>(sql, new { }, LoadConnectionString())[0];
    }

    private void AddFlightProperties(FlightWithData flight)
    {
        flight.FlightPoints = IgcHelper.GetFlightPointsFromIgcContent(flight.IgcFileContent);
        flight.TakeOffPoint = GetTakeOffPointFromPointList(flight.FlightPoints);
        flight.FlightDuration = GetFlightDurationFromPointList(flight.FlightPoints) ??
                                TimeSpan.FromSeconds(flight.FlightDuration_s);
        flight.IGC_GliderName = GetGliderNameFromIgcContent(flight.IgcFileContent);
        flight.FlightPhotos = GetAllPhotoForFlight(flight.ToFlight());
    }

    private void CreateFlightLogDB()
    {
        string sqlCreateSites = @"CREATE TABLE ""Sites"" (
    ""Site_ID""   TEXT NOT NULL UNIQUE,
    ""Name""  TEXT NOT NULL UNIQUE,
    ""Town""  TEXT,
    ""Country""   INTEGER NOT NULL,
    ""WindOrientationBegin""  INTEGER NOT NULL,
    ""WindOrientationEnd""    INTEGER NOT NULL,
    ""Altitude""  REAL NOT NULL,
    ""Latitude""  REAL NOT NULL,
    ""Longitude"" REAL NOT NULL,
    PRIMARY KEY(""Site_ID""));";
        string sqlCreateGliders = @"CREATE TABLE ""Gliders"" (
    ""Glider_ID"" TEXT NOT NULL UNIQUE,
    ""Manufacturer""  TEXT NOT NULL,
    ""Model"" TEXT NOT NULL,
    ""BuildYear"" INTEGER NOT NULL,
    ""LastCheckDateTime"" TEXT,
    ""HomologationCategory""  INTEGER,
    ""IGC_Name""  TEXT UNIQUE,
    PRIMARY KEY(""Glider_ID""));";
        string sqlCreateFlights = @"CREATE TABLE ""Flights"" (
    ""Flight_ID"" TEXT NOT NULL UNIQUE,
    ""Comment""   TEXT,
    ""REF_TakeOffSite_ID""    TEXT NOT NULL,
    ""REF_Glider_ID"" TEXT NOT NULL,
    ""FlightDuration_s""    INTEGER,
    ""TakeOffDateTime""    TEXT,
    ""IgcFileContent""    TEXT,
    ""GeoJsonScore""   TEXT,
    PRIMARY KEY(""Flight_ID""));";
        string sqlCreateDbInfo = @"CREATE TABLE ""DbInformations"" (
    ""VersionMajor""	INTEGER NOT NULL,
    ""VersionMinor""	INTEGER NOT NULL,
    ""VersionFix""	INTEGER NOT NULL,
    ""UserId""	TEXT NOT NULL
)";
        string sqlCreatePhotosTable = @"CREATE TABLE ""FlightPhotos"" (
    ""Photo_ID"" TEXT NOT NULL UNIQUE,
    ""REF_Flight_ID""    TEXT NOT NULL,
    PRIMARY KEY(""Photo_ID""),
    FOREIGN KEY(""REF_Flight_ID"") REFERENCES ""Flights""(""Flight_ID""));";
        _logger.LogInformation("Create new db for {UserId}", UserId);
        _db.SaveData(sqlCreateDbInfo, new { }, LoadConnectionString());
        var dbInfo = new DbInformations() { VersionMajor = 1, VersionMinor = 3, VersionFix = 0, UserId = UserId! };
        string sql =
            "INSERT INTO DbInformations (VersionMajor, VersionMinor, VersionFix, UserId) VALUES (@VersionMajor, @VersionMinor, @VersionFix, @UserId);";
        _db.SaveData(sql, dbInfo, LoadConnectionString());
        _db.SaveData(sqlCreateFlights, new { }, LoadConnectionString());
        _db.SaveData(sqlCreateGliders, new { }, LoadConnectionString());
        _db.SaveData(sqlCreateSites, new { }, LoadConnectionString());
        _db.SaveData(sqlCreatePhotosTable, new { }, LoadConnectionString());
        _logger.LogInformation("new db for {UserId} created", UserId);
    }

    internal void WriteSitesInDB(IList? newItems)
    {
        if (newItems != null)
        {
            string sqlWriteSite =
                @"INSERT INTO Sites
                (Site_ID, Name, Town, Country, WindOrientationBegin, WindOrientationEnd, Altitude, Latitude, Longitude)
                VALUES(@Site_ID, @Name, @Town, @Country, @WindOrientationBegin, @WindOrientationEnd, @Altitude, @Latitude, @Longitude)";
            foreach (Site site in newItems)
            {
                _db.SaveData(sqlWriteSite, site, LoadConnectionString());
            }
        }
    }

    internal void WriteFlightsInDB(List<FlightWithData>? newItems)
    {
        if (newItems != null)
        {
            string sqlWriteFlight = @"INSERT INTO Flights
                (Flight_ID, IgcFileContent, Comment, REF_TakeOffSite_ID, REF_Glider_ID, TakeOffDateTime, FlightDuration_s, GeoJsonScore)
                VALUES
                (@Flight_ID, @IgcFileContent, @Comment, @REF_TakeOffSite_ID, @REF_Glider_ID, @TakeOffDateTime, @FlightDuration_s, @GeoJsonText);";
            string sqlWriteFlightNoScore = @"INSERT INTO Flights
                (Flight_ID, IgcFileContent, Comment, REF_TakeOffSite_ID, REF_Glider_ID, TakeOffDateTime, FlightDuration_s)
                VALUES
                (@Flight_ID, @IgcFileContent, @Comment, @REF_TakeOffSite_ID, @REF_Glider_ID, @TakeOffDateTime, @FlightDuration_s);";
            foreach (var flight in newItems)
            {
                int FlightDuration_s = (int)flight.FlightDuration.TotalSeconds;
                if (flight.XcScore is not null)
                {
                    _db.SaveData(sqlWriteFlight,
                        new
                        {
                            flight.Flight_ID,
                            flight.IgcFileContent,
                            flight.Comment,
                            flight.REF_TakeOffSite_ID,
                            flight.REF_Glider_ID,
                            flight.TakeOffDateTime,
                            FlightDuration_s,
                            flight.XcScore.GeoJsonText
                        },
                        LoadConnectionString());
                }
                else
                {
                    _db.SaveData(sqlWriteFlightNoScore,
                        new
                        {
                            flight.Flight_ID,
                            flight.IgcFileContent,
                            flight.Comment,
                            flight.REF_TakeOffSite_ID,
                            flight.REF_Glider_ID,
                            flight.TakeOffDateTime,
                            FlightDuration_s,
                        },
                        LoadConnectionString());
                }
            }
        }
    }

    internal void WriteGlidersInDB(IList? newItems)
    {
        if (newItems != null)
        {
            string sqlWriteGlider =
                @"INSERT INTO Gliders (Glider_ID, Manufacturer, Model, BuildYear, LastCheckDateTime, HomologationCategory, IGC_Name)
                VALUES (@Glider_ID, @Manufacturer, @Model, @BuildYear, @LastCheckDateTime, @HomologationCategory, @IGC_Name)";

            foreach (Glider glider in newItems)
            {
                _db.SaveData(sqlWriteGlider, glider, LoadConnectionString());
            }
        }
    }

    /// <summary>
    /// Import a IGC file and put it as a Flight in the datamodel.
    /// </summary>
    /// <param name="IGC_FilePath"></param>
    /// <exception cref="NotImplementedException"></exception>
    public FlightWithData ImportFlightFromIgc(string IGC_FilePath)
    {
        var newFlight = new FlightWithData();

        using (var sr = new StreamReader(IGC_FilePath))
        {
            // to be done: check if it is a correct igc file before injecting
            _logger.LogDebug("Parsing {IGC_FilePath}", IGC_FilePath);
            newFlight.IgcFileContent = sr.ReadToEnd();
        }

        AddFlightProperties(newFlight);


        newFlight.TakeOffDateTime = GetTakeOffTimeFromIgcContent(newFlight.IgcFileContent);
        _logger.LogDebug("Find take off date time: {TakeOffDateTime}", newFlight.TakeOffDateTime);
        //Search for glider
        if (!string.IsNullOrEmpty(newFlight.IGC_GliderName))
        {
            Glider? glider = FindGliderFromGliderIgcName(newFlight.IGC_GliderName);
            if (glider != null)
            {
                newFlight.REF_Glider_ID = glider.Glider_ID;
            }
        }

        // search for a take off site
        Site takeOffSite = FindOrCreateTakeOffSiteByLocation(newFlight.TakeOffPoint);
        newFlight.REF_TakeOffSite_ID = takeOffSite.Site_ID;


        // insert the flight if everything is ok here
        WriteFlightsInDB([newFlight]);
        return newFlight;
    }

    /// <summary>
    /// Get the site referenced as where the <paramref name="flight"/> takes off.
    /// </summary>
    /// <param name="flight"></param>
    /// <returns></returns>
    public Site? GetFlightTakeOffSite(Flight flight)
    {
        string sqlStatement =
            "SELECT Site_ID, Name, Town, Country, WindOrientationBegin, WindOrientationEnd, Altitude, Latitude, Longitude " +
            "FROM Sites " +
            "WHERE Site_ID = @Id;";
        Site? site = _db
            .LoadData<Site, dynamic>(sqlStatement, new { Id = flight.REF_TakeOffSite_ID }, LoadConnectionString())
            .FirstOrDefault();
        return site;
    }

    private Glider? FindGliderFromGliderIgcName(string IGC_Name)
    {
        string sqlGetGlider =
            @"SELECT Glider_ID, Manufacturer, Model, BuildYear, LastCheckDateTime, HomologationCategory, IGC_Name
                                FROM Gliders
                                WHERE IGC_Name = @IGC_Name;";
        var output = _db.LoadData<Glider, dynamic>(sqlGetGlider, new { IGC_Name }, LoadConnectionString())
            .FirstOrDefault();

        return output;
    }

    private Site FindOrCreateTakeOffSiteByLocation(FlightPoint takeOffPoint)
    {
        string sqlGetAllSites =
            @"SELECT Site_ID, Name, Town, Country, WindOrientationBegin, WindOrientationEnd, Altitude, Latitude, Longitude
                                    FROM Sites";
        List<Site> sites = _db.LoadData<Site, dynamic>(sqlGetAllSites, new { }, LoadConnectionString());

        Site? output = sites.Find(s => takeOffPoint
                                           .DistanceFrom(new FlightPoint()
                                           {
                                               Longitude = s.Longitude,
                                               Latitude = s.Latitude,
                                               Altitude = s.Altitude
                                           }) <
                                       s.SiteRadius);
        if (output == null)
        {
            output = new Site()
            {
                Altitude = takeOffPoint.Altitude,
                Latitude = takeOffPoint.Latitude,
                Longitude = takeOffPoint.Longitude,
                Name = GetUnknownSiteNextName()
            };
            WriteSitesInDB(new List<Site> { output });
        }

        return output;
    }

    private string GetUnknownSiteNextName()
    {
        HashSet<string> unknownSiteNames = GetUnknownSites()
            .Select(x => x.Name)
            .ToHashSet();

        int nextUnknownSite = 0;
        while (unknownSiteNames.Contains($"Unknown site {nextUnknownSite}"))
        {
            nextUnknownSite++;
        }

        return $"Unknown site {nextUnknownSite}";
    }

    private List<Site> GetUnknownSites()
    {
        string sqlStatement =
            @"SELECT Site_ID, Name, Town, Country, WindOrientationBegin, WindOrientationEnd, Altitude, Latitude, Longitude
                                    FROM Sites
                                    WHERE Name LIKE '%Unknown site%';";
        List<Site> ouput = _db.LoadData<Site, dynamic>(sqlStatement, new { }, LoadConnectionString());
        return ouput;
    }

    private string LoadConnectionString(string connectionStringName = "Sqlite")
    {
        if (UserId is null)
        {
            _logger.LogError("Connection string cannot be build without a user id");
            return "";
        }

        string rawCs = _config.GetConnectionString(connectionStringName)!;
        return rawCs.Replace("{UserId}", UserId, StringComparison.InvariantCulture);
    }

    /// <summary>
    /// The take off time in UTC as a timestamp based on the igc data (date in meta data and time as the timestamp of the first sample)
    /// </summary>
    private static DateTime GetTakeOffTimeFromIgcContent(string igcContent)
    {
        const string FLIGHT_TIME_REGEXP = @"B(?<h>\d\d)(?<m>\d\d)(?<s>\d\d)";
        const string FLIGHT_DATE_REGEXP = @"HFDTE(DATE:)?(?<d>\d\d)(?<m>\d\d)(?<y>\d\d)";
        const int MILLENAR = 2000;

        string l_igcAllInOneLine = igcContent
            .Replace("\r", "", StringComparison.InvariantCulture)
            .Replace("\n", "", StringComparison.InvariantCulture);
        var matchTime = Regex.Match(l_igcAllInOneLine, FLIGHT_TIME_REGEXP);
        var matchDate = Regex.Match(l_igcAllInOneLine, FLIGHT_DATE_REGEXP);

        if (matchDate.Success && matchTime.Success)
        {
            var l_FlightYear = int.Parse(matchDate.Groups["y"].Value) + MILLENAR;
            var l_FlightMonth = int.Parse(matchDate.Groups["m"].Value);
            var l_FlightDay = int.Parse(matchDate.Groups["d"].Value);

            var l_FlightHour = int.Parse((matchTime.Groups["h"].Value));
            var l_FlightMinute = int.Parse((matchTime.Groups["m"].Value));
            var l_FlightSecond = int.Parse((matchTime.Groups["s"].Value));

            return new DateTime(l_FlightYear, l_FlightMonth, l_FlightDay, l_FlightHour, l_FlightMinute, l_FlightSecond,
                DateTimeKind.Utc);
        }

        return DateTime.MinValue;
    }

    private static TimeSpan? GetFlightDurationFromPointList(ICollection<FlightPoint> flightPoints)
    {
        return flightPoints.Count > 0 ? new TimeSpan(0, 0, flightPoints.Count) : null;
    }

    private static FlightPoint GetTakeOffPointFromPointList(IList<FlightPoint> flightPoints)
    {
        return flightPoints.Count > 0
            ? flightPoints[0]
            : new FlightPoint() { Latitude = double.NaN, Longitude = double.NaN, Altitude = double.NaN };
    }



    private static string GetGliderNameFromIgcContent(string igcContent)
    {
        const string GLIDER_TYPE_REGEX = @"^HFGTYGLIDERTYPE:(?<value>.+)$";
        var match = Regex.Match(igcContent, GLIDER_TYPE_REGEX, RegexOptions.Multiline);
        if (match.Success)
        {
            return match.Groups["value"].Value.TrimEnd('\r', '\n');
        }

        return "";
    }

    /// <summary>
    /// Get the glider referenced as used during the <paramref name="flight"/>
    /// </summary>
    /// <param name="flight"></param>
    /// <returns></returns>
    public Glider? GetFlightGlider(Flight flight)
    {
        string sqlStatement =
            "SELECT Glider_ID, Manufacturer, Model, BuildYear, LastCheckDateTime, HomologationCategory, IGC_Name " +
            "FROM Gliders " +
            "WHERE Glider_ID = @Id;";
        Glider? output = _db
            .LoadData<Glider, dynamic>(sqlStatement, new { Id = flight.REF_Glider_ID }, LoadConnectionString())
            .FirstOrDefault();
        return output;
    }

    /// <summary>
    /// Get the number of flight done from this <paramref name="site"/>
    /// </summary>
    /// <param name="site"></param>
    /// <returns></returns>
    public int GetSiteFlightCount(Site site)
    {
        string sqlStatement = @"SELECT COUNT(1)
                                    FROM Flights f
                                    WHERE f.REF_TakeOffSite_ID = @Site_ID
                                    GROUP BY f.REF_TakeOffSite_ID;";

        int output = _db.LoadData<int, dynamic>(sqlStatement, new { site.Site_ID }, LoadConnectionString())
            .FirstOrDefault();

        return output;
    }

    /// <summary>
    /// Get the comment associated with the <paramref name="flight"/>.
    /// </summary>
    /// <param name="flight"></param>
    /// <returns></returns>
    public string? GetFlightComment(Flight flight)
    {
        string sqlStatement = "SELECT Comment FROM Flights WHERE Flight_ID = @Id;";
        return _db.LoadData<string, dynamic>(sqlStatement, new { Id = flight.Flight_ID }, LoadConnectionString())
            .FirstOrDefault();
    }

    /// <summary>
    /// Update the <paramref name="flight"/> comment with <paramref name="value"/>
    /// </summary>
    /// <param name="flight"></param>
    /// <param name="value"></param>
    public void UpdateFlightComment(Flight flight, string value)
    {
        string sqlStatement = "UPDATE Flights SET Comment = @Comment WHERE Flight_ID = @Flight_ID;";
        _db.SaveData(sqlStatement, new { Comment = value, flight.Flight_ID }, LoadConnectionString());
    }

    /// <summary>
    /// Update the <paramref name="flight"/> glider with <paramref name="glider"/>
    /// </summary>
    /// <param name="flight"></param>
    /// <param name="glider"></param>
    public void UpdateFlightGlider(Flight flight, Glider glider)
    {
        string sqlStatement = "UPDATE Flights SET REF_Glider_ID = @Glider_ID WHERE Flight_ID = @Flight_ID;";
        _db.SaveData(sqlStatement, new { glider.Glider_ID, flight.Flight_ID }, LoadConnectionString());
    }

    /// <summary>
    /// Update the <paramref name="flight"/> site with <paramref name="site"/>
    /// </summary>
    /// <param name="flight"></param>
    /// <param name="site"></param>
    public void UpdateFlightTakeOffSite(Flight flight, Site site)
    {
        string sqlStatement = "UPDATE Flights SET REF_TakeOffSite_ID = @Site_ID WHERE Flight_ID = @Flight_ID;";
        _db.SaveData(sqlStatement, new { site.Site_ID, flight.Flight_ID }, LoadConnectionString());
    }
    /// <summary>
    /// Get the flight objective for the specified <paramref name="flight"/>.
    /// </summary>
    /// <param name="flight"></param>
    /// <returns></returns>
    public EFlightObjective GetFlightObjective(Flight flight)
    {
        string sqlStatement = "SELECT Objective FROM Flights WHERE Flight_ID = @Id;";
        string? objectiveStr = _db.LoadData<string, dynamic>(sqlStatement, new { Id = flight.Flight_ID },
            LoadConnectionString()).FirstOrDefault();
        if (objectiveStr is not null && Enum.TryParse<EFlightObjective>(objectiveStr, out var objective))
        {
            return objective;
        }
        else
        {
            return EFlightObjective.Undefined;
        }
    }

    /// <summary>
    /// Update the flight objective for the specified <paramref name="flight"/> with the provided <paramref name="objective"/>.
    /// </summary>
    /// <param name="flight"></param>
    /// <param name="objective"></param>
    public void UpdateFlightObjective(Flight flight, EFlightObjective objective)
    {
        string sqlStatement = "UPDATE Flights SET Objective = @Objective WHERE Flight_ID = @Flight_ID;";
        _db.SaveData(sqlStatement, new { Objective = objective, flight.Flight_ID }, LoadConnectionString());
    }

    /// <summary>
    /// Retrieve all flight from the Db. Those flight does not contains the IGC data to lighten the ram usage a bit
    /// </summary>
    /// <returns></returns>
    public async Task<List<Flight>> GetAllFlightsAsync()
    {
        var sw = Stopwatch.StartNew();
        string sqlStatement =
            "SELECT Flight_ID, Comment, REF_TakeOffSite_ID, REF_Glider_ID, FlightDuration_s, TakeOffDateTime, Objective FROM Flights;";
        var output = await _db.LoadDataAsync<Flight, dynamic>(sqlStatement, new { }, LoadConnectionString());
        foreach (var flight in output)
        {
            flight.XcScore = await GetFlightScoreAsync(flight);
        }

        _logger.LogInformation("All flights requested and get in {GetAllFlightsDuration_ms} ms",
            sw.ElapsedMilliseconds);
        return output;
    }

    /// <summary>
    /// Get the score  associated with a <paramref name="flight"/> if any
    /// </summary>
    /// <param name="flight"></param>
    /// <returns></returns>
    private async Task<XcScore?> GetFlightScoreAsync(Flight flight)
    {
        string sql = "SELECT GeoJsonScore FROM Flights WHERE Flight_ID = @Flight_ID;";
        string? scoreJson = (await _db.LoadDataAsync<string, dynamic>(sql, flight, LoadConnectionString()))
            .FirstOrDefault();
        if (scoreJson == null) { return null; }
        else { return XcScore.FromJson(scoreJson); }
    }

    /// <summary>
    /// Get the score  associated with a <paramref name="flight"/> if any
    /// </summary>
    /// <param name="flight"></param>
    /// <returns></returns>
    private XcScore? GetFlightScore(FlightWithData flight)
    {
        string sql = "SELECT GeoJsonScore FROM Flights WHERE Flight_ID = @Flight_ID;";
        string? scoreJson = _db.LoadData<string, dynamic>(sql, flight, LoadConnectionString()).FirstOrDefault();
        if (scoreJson == null) { return null; }
        else { return XcScore.FromJson(scoreJson); }
    }

    /// <summary>
    /// Get all sites in the db
    /// </summary>
    /// <returns></returns>
    public async Task<List<Site>> GetAllSitesAsync()
    {
        var sw = Stopwatch.StartNew();

        string sqlStatement =
            @"SELECT Site_ID, Name, Town, Country, WindOrientationBegin, WindOrientationEnd, Altitude, Latitude, Longitude
                                    FROM Sites;";
        var output = await _db.LoadDataAsync<Site, dynamic>(sqlStatement, new { }, LoadConnectionString());
        _logger.LogInformation("All sites requested and get in {GetAllSitesDuration_ms} ms", sw.ElapsedMilliseconds);
        return output;
    }

    /// <summary>
    /// Get all the glider in the db
    /// </summary>
    /// <returns></returns>
    public async Task<List<Glider>> GetAllGlidersAsync()
    {
        var sw = Stopwatch.StartNew();

        string sqlStatement =
            @"SELECT Glider_ID, Manufacturer, Model, BuildYear, LastCheckDateTime, HomologationCategory, IGC_Name
                                FROM Gliders;";
        var output = await _db.LoadDataAsync<Glider, dynamic>(sqlStatement, new { }, LoadConnectionString());
        _logger.LogInformation("All glider requested and get in {GetAllGlidersDuration_ms} ms", sw.ElapsedMilliseconds);
        return output;
    }

    /// <summary>
    /// Get the number of flight done with a <paramref name="glider"/> 
    /// </summary>
    /// <param name="glider"></param>
    /// <returns></returns>
    public int GetFlightDoneCountWithGlider(Glider glider)
    {
        string sqlStatement = @"SELECT COUNT(1)
                                    FROM Flights f
                                    WHERE f.REF_Glider_ID = @Glider_ID
                                    GROUP BY f.REF_Glider_ID";
        var output = _db.LoadData<int, dynamic>(sqlStatement, glider, LoadConnectionString()).FirstOrDefault();
        return output;
    }

    /// <summary>
    /// Get the flight time done with a <paramref name="glider"/> in a specific period defined between <paramref name="start"/> and <paramref name="end"/>
    /// </summary>
    /// <param name="glider"></param>
    /// <param name="start"></param>
    /// <param name="end"></param>
    /// <returns></returns>
    public TimeSpan FlightTimeInPeriodWithGlider(Glider glider, DateTime start, DateTime end)
    {
        string sqlStatement = @"SELECT SUM(FlightDuration_s)
                                    FROM Flights f
                                    WHERE f.REF_Glider_ID = @Glider_ID AND
                                    f.TakeOffDateTime >= @start AND
                                    f.TakeOffDateTime <= @end";
        var result = _db
            .LoadData<int?, dynamic>(sqlStatement, new { glider.Glider_ID, start, end }, LoadConnectionString())
            .FirstOrDefault();
        return TimeSpan.FromSeconds(result ?? 0);
    }

    /// <summary>
    /// Update the respective db record with <paramref name="flight"/> details
    /// </summary>
    /// <param name="flight"></param>
    public async Task UpdateFlightAsync(Flight flight)
    {
        string sqlStatement = @"UPDATE Flights SET 
                                    Comment = @Comment,
                                    REF_TakeOffSite_ID = @REF_TakeOffSite_ID,
                                    REF_Glider_ID = @REF_Glider_ID,
                                    FlightDuration_s = @FlightDuration_s,
                                    TakeOffDateTime = @TakeOffDateTime,
                                    Objective = @Objective,
                                    GeoJsonScore = @GeoJsonText
                                    WHERE Flight_ID = @Flight_ID;";
        string sqlStatementNoScore = @"UPDATE Flights SET
                                    Comment = @Comment,
                                    REF_TakeOffSite_ID = @REF_TakeOffSite_ID,
                                    REF_Glider_ID = @REF_Glider_ID,
                                    FlightDuration_s = @FlightDuration_s,
                                    TakeOffDateTime = @TakeOffDateTime,
                                    Objective = @Objective,
                                    WHERE Flight_ID = @Flight_ID;";
        if (flight.XcScore is not null)
        {
            await _db.SaveDataAsync(sqlStatement,
                new
                {
                    flight.Flight_ID,
                    flight.Comment,
                    flight.REF_TakeOffSite_ID,
                    flight.REF_Glider_ID,
                    flight.FlightDuration_s,
                    flight.TakeOffDateTime,
                    flight.Objective,
                    flight.XcScore.GeoJsonText
                },
                LoadConnectionString());
        }
        else
        {
            await _db.SaveDataAsync(sqlStatementNoScore,
                new
                {
                    flight.Flight_ID,
                    flight.Comment,
                    flight.REF_TakeOffSite_ID,
                    flight.REF_Glider_ID,
                    flight.FlightDuration_s,
                    flight.TakeOffDateTime,
                    flight.Objective
                },
                LoadConnectionString());
        }
    }

    /// <summary>
    /// Update the respective db record with the <paramref name="site"/> details
    /// </summary>
    /// <param name="site"></param>
    public void UpdateSite(Site site)
    {
        string sqlStatement = @"UPDATE Sites
            SET Name = @Name,
            Town = @Town,
            Country = @Country,
            WindOrientationBegin = @WindOrientationBegin,
            WindOrientationEnd = @WindOrientationEnd,
            Altitude = @Altitude,
            Latitude = @Latitude,
            Longitude = @Longitude
            WHERE Site_ID = @Site_ID;";
        _db.SaveData(sqlStatement, site, LoadConnectionString());
    }

    /// <summary>
    /// Update the respective db record with the <paramref name="glider"/> details
    /// </summary>
    /// <param name="glider"></param>
    public void UpdateGlider(Glider glider)
    {
        string sqlStatement = @"UPDATE Gliders
                                    SET	Manufacturer = @Manufacturer,
                                    Model = @Model,
                                    BuildYear = @BuildYear,
                                    LastCheckDateTime = @LastCheckDateTime,
                                    HomologationCategory = @HomologationCategory,
                                    IGC_Name = @IGC_Name
                                    WHERE Glider_ID = @Glider_ID;";
        _db.SaveData(sqlStatement, glider, LoadConnectionString());
    }

    /// <summary>
    /// Delete the corresponding <paramref name="flight"/> from the db
    /// </summary>
    /// <param name="flight"></param>
    public void DeleteFlight(Flight flight)
    {
        string sqlStatement = "DELETE FROM Flights WHERE Flight_ID = @Flight_ID";
        _db.SaveData(sqlStatement, flight, LoadConnectionString());
    }

    /// <summary>
    /// Backup the db
    /// </summary>
    /// <returns></returns>
    public async Task<string> BackupDbAsync()
    {
        string dbPath = GetDbPath();
        string backupPath = Path.Combine(new FileInfo(dbPath).Directory!.FullName,
            $"FlightLogBackup{DateTime.UtcNow:yyyyMMdd_HHmmss}.db");
        await Task.Run(() => File.Copy(dbPath, backupPath));
        return backupPath;
    }

    private string GetDbPath()
    {
        string pattern = @"(?<=Data Source=).*?(?=;)";
        var rgx = new Regex(pattern);
        Match match = rgx.Match(LoadConnectionString());
        string dbPath = match.Value;

        return dbPath;
    }

    /// <summary>
    /// Get the list of the site used between <paramref name="start"/> and <paramref name="end"/>
    /// </summary>
    /// <param name="start"></param>
    /// <param name="end"></param>
    /// <returns></returns>
    public List<Site> GetSitesUsedInTimeRange(DateTime start, DateTime end)
    {
        string sqlStatement =
            "SELECT DISTINCT Site_ID, Name, Town, Country, WindOrientationBegin, WindOrientationEnd, Altitude, Latitude, Longitude " +
            "FROM Sites s " +
            "JOIN Flights f ON s.Site_ID = f.REF_TakeOffSite_ID " +
            "WHERE f.TakeOffDateTime < @end AND f.TakeOffDateTime > @start;";
        var output = _db.LoadData<Site, dynamic>(sqlStatement,
            new { start = start.ToString("s"), end = end.ToString("s") },
            LoadConnectionString());
        return output;
    }

    /// <summary>
    /// Get the additionnal data of the <paramref name="flight"/>. Igc content and score (if available) are added
    /// </summary>
    /// <param name="flight"></param>
    /// <returns></returns>
    public FlightWithData GetFlightWithData(Flight flight)
    {
        string sql =
            @"SELECT Flight_ID, IgcFileContent, Comment, REF_TakeOffSite_ID, REF_Glider_ID, TakeOffDateTime, FlightDuration_s
        FROM Flights
        WHERE Flight_ID=@Flight_ID;";
        var output = _db.LoadData<FlightWithData, dynamic>(sql, flight, LoadConnectionString())[0];
        AddFlightProperties(output);
        output.XcScore = GetFlightScore(output);

        return output;
    }
    /// <summary>
    /// Get the meta data of all the photo for the <paramref name="flight"/>
    /// </summary>
    /// <param name="flight"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public List<FlightPhoto> GetAllPhotoForFlight(Flight flight)
    {
        if (UserId is null) { return []; }
        string sql = """
                     SELECT Photo_ID, REF_Flight_ID
                     FROM FlightPhotos
                     WHERE REF_FLIGHT_ID = @Flight_ID;
                     """;
        var output = _db.LoadData<FlightPhoto, dynamic>(sql, flight, LoadConnectionString());
        foreach (var flightPhoto in output)
        {
            flightPhoto.REF_User_Id = UserId;
        }
        return output;
    }
    /// <summary>
    /// Write the the meta data of a photo to the Db
    /// </summary>
    /// <param name="flightPhoto"></param>
    public void WriteFlightPhoto(FlightPhoto flightPhoto)
    {
        string sql = """
                     INSERT INTO FlightPhotos
                     (Photo_ID, REF_Flight_ID)
                     VALUES (@Photo_ID, @REF_Flight_ID);
                     """;
        _db.SaveData(sql, new { flightPhoto.Photo_ID, flightPhoto.REF_Flight_ID }, LoadConnectionString());
    }
    /// <summary>
    /// Add a new glider to the db. The glider is created with default values.
    /// </summary>
    public async Task<Glider> AddGliderAsync()
    {
        string sql = """
                     INSERT INTO Gliders
                     (Glider_ID, Manufacturer, Model, BuildYear, LastCheckDateTime, HomologationCategory, IGC_Name)
                     VALUES (@Glider_ID, @Manufacturer, @Model, @BuildYear, @LastCheckDateTime, @HomologationCategory, @IGC_Name);
                     """;
        var glider = new Glider();
        await _db.SaveDataAsync(sql, glider, LoadConnectionString());
        return glider;
    }
    /// <summary>
    /// Delete the <paramref name="glider"/> from the db.
    /// </summary>
    /// <param name="glider"></param>
    public async Task DeleteGliderAsync(Glider glider)
    {
        string sql = """
                     DELETE FROM Gliders
                     WHERE Glider_ID = @Glider_ID;
                     """;
        await _db.SaveDataAsync(sql, new { glider.Glider_ID }, LoadConnectionString());
    }
    /// <summary>
    /// Deletes the specified site from the database.
    /// </summary>
    /// <remarks>This method removes the site identified by the <see cref="Site.Site_ID"/> property from the
    /// database.  Ensure that the provided site exists in the database before calling this method to avoid unintended
    /// behavior.</remarks>
    /// <param name="site">The site to delete. The <see cref="Site.Site_ID"/> property must be set to identify the site to remove.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public async Task DeleteSiteAsync(Site site)
    {
        string sql = """
                     DELETE FROM Sites
                     WHERE Site_ID = @Site_ID;
                     """;
        await _db.SaveDataAsync(sql, new { site.Site_ID }, LoadConnectionString());
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="xcDistanceLimitKm"></param>
    /// <param name="localDurationLimitMin"></param>
    public async Task ApplyFlightObjectivesToUndefinedFlightsAsync(double xcDistanceLimitKm, TimeSpan localDurationLimitMin)
    {
        var flights = await GetAllFlightsAsync();
        foreach (var f in flights.Where(x => x.Objective == EFlightObjective.Undefined))
        {
            if (f.XcScore is not null && f.XcScore.RouteLength >= xcDistanceLimitKm)
            {
                f.Objective = EFlightObjective.XC;
            }
            else if (f.FlightDuration.TotalMinutes >= localDurationLimitMin.TotalMinutes)
            {
                f.Objective = EFlightObjective.Local;
            }
            else
            {
                // if the flight is not local and not xc, we consider it as undefined.
            }
            await UpdateFlightAsync(f);
        }
    }

    public async Task AddFlightAsync(Flight newFlight)
    {
        string sql = @"INSERT INTO Flights
                (Flight_ID, Comment, REF_TakeOffSite_ID, REF_Glider_ID, TakeOffDateTime, FlightDuration_s, Objective)
                VALUES
                (@Flight_ID, @Comment, @REF_TakeOffSite_ID, @REF_Glider_ID, @TakeOffDateTime, @FlightDuration_s, @Objective);";
        await _db.SaveDataAsync(sql, newFlight, LoadConnectionString());
    }
    /// <summary>
    /// Check if the <paramref name="flight"/> exists in the db
    /// </summary>
    /// <param name="flight"></param>
    /// <returns></returns>
    public async Task<bool> FlightExistsAsync(Flight flight)
    {
        string sql = "SELECT COUNT(1) FROM Flights WHERE Flight_ID = @Flight_ID;";
        int count = await _db.LoadDataAsync<int, dynamic>(sql, new { flight.Flight_ID }, LoadConnectionString())
            .ContinueWith(t => t.Result.FirstOrDefault());
        return count > 0;
    }

    public async Task CrateSiteAsync(Site site)
    {
        string sql = """
                     INSERT INTO Sites
                     (Site_ID, Name, Town, Country, WindOrientationBegin, WindOrientationEnd, Altitude, Latitude, Longitude)
                     VALUES (@Site_ID, @Name, @Town, @Country, @WindOrientationBegin, @WindOrientationEnd, @Altitude, @Latitude, @Longitude);
                     """;
        await _db.SaveDataAsync(sql, site, LoadConnectionString());
    }
}
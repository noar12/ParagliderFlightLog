using System;
using System.IO;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Globalization;
using System.Data;
using System.Data.SQLite;
using Dapper;
using System.Collections;
using System.Collections.Specialized;
using ParagliderFlightLog.Models;
using Microsoft.Extensions.Configuration;

namespace ParagliderFlightLog.DataAccess
{
	public class FlightLogDB
	{
		private readonly SqliteDataAccess _db;
		private readonly IConfiguration _config;

		public FlightLogDB(IConfiguration config)
		{
			_db = new();
			_config = config;
			if (!_db.DbExists(LoadConnectionString()))
			{
				CreateFlightLogDB();
			}
			LoadFlightLogDB();
		}


		public void LoadFlightLogDB()
		{
			string sqlGetAllSite = "SELECT Site_ID, Name, Town, Country, WindOrientationBegin, WindOrientationEnd, Altitude, Latitude, Longitude FROM Sites";
			string sqlGetAllGlider = "SELECT Glider_ID, Manufacturer, Model, BuildYear, LastCheckDateTime, HomologationCategory, IGC_Name FROM Gliders";
			string sqlGetAllFlight = "SELECT Flight_ID, IgcFileContent, Comment, REF_TakeOffSite_ID, REF_Glider_ID, FlightDuration_s, TakeOffDateTime FROM Flights";

			Sites = _db.LoadData<Site, dynamic>(sqlGetAllSite, new { }, LoadConnectionString());
			Gliders = _db.LoadData<Glider, dynamic>(sqlGetAllGlider, new { }, LoadConnectionString());
			Flights = _db.LoadData<Flight, dynamic>(sqlGetAllFlight, new { }, LoadConnectionString());
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
	PRIMARY KEY(""Flight_ID""));";

			_db.SaveData(sqlCreateFlights, new { }, LoadConnectionString());
			_db.SaveData(sqlCreateGliders, new { }, LoadConnectionString());
			_db.SaveData(sqlCreateSites, new { }, LoadConnectionString());
		}

		private void ReplaceFlightInDB(IList? newItems)
		{
			if (newItems != null)
			{
				string sqlReplaceFlight = "UPDATE Flights SET Comment = @Comment, REF_TakeOffSite_ID = @REF_TakeOffSite_ID, REF_Glider_ID = @REF_Glider_ID, FlightDuration_s = @FlightDuration_s, TakeOffDateTime = @TakeOffDateTime, IgcFileContent = @IgcFileContent WHERE Flight_ID = @Flight_ID";
				foreach (Flight flight in newItems)
				{
					_db.SaveData(sqlReplaceFlight, flight, LoadConnectionString());
				}

			}
		}

		private void ReplaceSitesInDB(IList? newItems)
		{
			if (newItems != null)
			{
				string sqlReplaceSite = "UPDATE Sites SET Name = @Name, Town = @Town, Country = @Country, WindOrientationBegin = @WindOrientationBegin, WindOrientationEnd = @WindOrientationEnd, Altitude = @Altitude, Latitude = @Latitude, Longitude = @Longitude WHERE Site_ID = @Site_ID";

				foreach (Site site in newItems)
				{
					_db.SaveData(sqlReplaceSite, site, LoadConnectionString());
				}
			}
		}

		private void DeleteSitesInDB(IList? oldItems)
		{
			throw new NotImplementedException();
		}

		private void WriteSitesInDB(IList? newItems)
		{
			if (newItems != null)
			{
				string sqlWriteSite = "INSERT INTO Sites (Site_ID, Name, Town, Country, WindOrientationBegin, WindOrientationEnd, Altitude, Latitude, Longitude) VALUES(@Site_ID, @Name, @Town, @Country, @WindOrientationBegin, @WindOrientationEnd, @Altitude, @Latitude, @Longitude)";
				foreach (Site site in newItems)
				{
					_db.SaveData(sqlWriteSite, site, LoadConnectionString());
				}

			}
		}

		private void DeleteFlightsInDB(IList? oldItems)
		{
			if (oldItems != null)
			{
				string sqlDeleteFlight = "DELETE FROM Flights WHERE Flight_ID = @Flight_ID";

				foreach (Flight flight in oldItems)
				{
					_db.SaveData(sqlDeleteFlight, flight, LoadConnectionString());
				}

			}
		}

		private void WriteFlightsInDB(IList? newItems)
		{
			if (newItems != null)
			{
				string sqlWriteFlight = @"INSERT INTO Flights
				(Flight_ID, IgcFileContent, Comment, REF_TakeOffSite_ID, REF_Glider_ID, TakeOffDateTime, FlightDuration_s)
				VALUES
				(@Flight_ID, @IgcFileContent, @Comment, @REF_TakeOffSite_ID, @REF_Glider_ID, @TakeOffDateTime, @FlightDuration_s);";

				foreach (Flight flight in newItems)
				{
					int FlightDuration_s = (int)flight.FlightDuration.TotalSeconds;

					_db.SaveData(sqlWriteFlight,
					new
					{
						flight.Flight_ID,
						flight.IgcFileContent,
						flight.Comment,
						flight.REF_TakeOffSite_ID,
						flight.REF_Glider_ID,
						flight.TakeOffDateTime,
						FlightDuration_s
					},
					LoadConnectionString());
				}


			}
		}

		private void DeleteGlidersInDB(IList? oldItems)
		{
			throw new NotImplementedException();
		}
		private void ReplaceGlidersInDB(IList? newItems)
		{
			if (newItems != null)
			{
				string sqlReplaceGlider = "UPDATE Gliders SET " +
					"Manufacturer = @Manufacturer, " +
					"Model = @Model, " +
					"BuildYear = @BuildYear, " +
					"LastCheckDateTime = @LastCheckDateTime, " +
					"HomologationCategory = @HomologationCategory, " +
					"IGC_Name = @IGC_Name " +
					"WHERE Glider_ID = @Glider_ID";


				foreach (Glider glider in newItems)
				{
					_db.SaveData(sqlReplaceGlider, glider, LoadConnectionString());
				}


			}
		}
		private void WriteGlidersInDB(IList? newItems)
		{
			if (newItems != null)
			{
				string sqlWriteGlider = "INSERT INTO Gliders (Glider_ID, Manufacturer, Model, BuildYear, LastCheckDateTime, HomologationCategory, IGC_Name) VALUES (@Glider_ID, @Manufacturer, @Model, @BuildYear, @LastCheckDateTime, @HomologationCategory, @IGC_Name)";

				foreach (Glider glider in newItems)
				{
					_db.SaveData(sqlWriteGlider, glider, LoadConnectionString());
				}


			}
		}

		public List<Flight> Flights { get; set; } = new List<Flight>();
		public List<Site> Sites { get; set; } = new List<Site>();
		public List<Glider> Gliders { get; set; } = new List<Glider>();
		/// <summary>
		/// Get the cumulative flight duration of all the flight between analyzePeriodStart and analyszePeriodEnd
		/// </summary>
		/// <param name="analyzePeriodStart"></param>
		/// <param name="analyzePeriodEnd"></param>
		/// <returns></returns>
		public TimeSpan GetTotalFlightDuration(DateTime? analyzePeriodStart = null, DateTime? analyzePeriodEnd = null)
		{
			if (analyzePeriodStart == null)
			{
				analyzePeriodStart = DateTime.MinValue;
			}
			if (analyzePeriodEnd == null)
			{
				analyzePeriodEnd = DateTime.Now;
			}
			TimeSpan l_totalFlightDuration = new TimeSpan(0, 0, 0, 0);

			l_totalFlightDuration = Flights.Where(flight => flight.TakeOffDateTime > analyzePeriodStart && flight.TakeOffDateTime < analyzePeriodEnd)
				.Aggregate(TimeSpan.Zero, (subtotal, flight) => subtotal.Add(flight.FlightDuration));

			return l_totalFlightDuration;
		}
		/// <summary>
		/// Import a IGC file and put it as a Flight in the datamodel.
		/// </summary>
		/// <param name="IGC_FilePath"></param>
		/// <exception cref="NotImplementedException"></exception>
		public Flight ImportFlightFromIGC(string IGC_FilePath)
		{
			Flight newFlight = new Flight();

			using (var sr = new StreamReader(IGC_FilePath))
			{
				// to be done: check if it is a correct igc file before injecting
				newFlight.IgcFileContent = sr.ReadToEnd();
			}

			newFlight.FlightPoints = GetFlightPointsFromIgcContent(newFlight.IgcFileContent);
			newFlight.TakeOffPoint = GetTakeOffPointFromPointList(newFlight.FlightPoints);
			newFlight.FlightDuration = GetFlightDurationFromPointList(newFlight.FlightPoints);
			newFlight.TakeOffDateTime = GetTakeOffTimeFromIgcContent(newFlight.IgcFileContent);
			newFlight.IGC_GliderName = GetGliderNameFromIgcContent(newFlight.IgcFileContent);

			// check if we were able to parse some point before continuing the insert
			if (newFlight.FlightPoints.Any())
			{
				Flights.Add(newFlight);

			}
			else
			{
				throw new Exception();
			}

			//Search for glider
			if (!string.IsNullOrEmpty(newFlight.IGC_GliderName))
			{
				Glider? glider = FindGliderFromGliderIgcName(newFlight.IGC_GliderName);
				if (glider != null)
				{
					newFlight.REF_Glider_ID = glider.Glider_ID;
					WriteGlidersInDB(new List<Glider> { glider });
				}
			}

			// search for a take off site
			Site takeOffSite = FindOrCreateTakeOffSiteByLocation(newFlight.TakeOffPoint);
			newFlight.REF_TakeOffSite_ID = takeOffSite.Site_ID;
		   

			// insert the flight if everything is ok here
			WriteFlightsInDB(new List<Flight> { newFlight });
			return newFlight;
		}
		public Site? FlightTakeOffSite(Flight flight)
		{
			string sqlStatement = "SELECT Site_ID, Name, Town, Country, WindOrientationBegin, WindOrientationEnd, Altitude, Latitude, Longitude " +
			"FROM Sites " +
			"WHERE Site_ID = @Id;";
			Site? site = _db.LoadData<Site, dynamic>(sqlStatement, new { Id = flight.REF_TakeOffSite_ID },LoadConnectionString()).FirstOrDefault();
			return site;
		}

		private Glider? FindGliderFromGliderIgcName(string IGC_Name)
		{
			Glider? output = null;
			string sqlGetGlider = @"SELECT Glider_ID, Manufacturer, Model, BuildYear, LastCheckDateTime, HomologationCategory, IGC_Name
								FROM Gliders
								WHERE IGC_Name = @IGC_Name;";
			output = _db.LoadData<Glider, dynamic>(sqlGetGlider, new { IGC_Name }, LoadConnectionString()).FirstOrDefault();

			return output;
		}

		private Site FindOrCreateTakeOffSiteByLocation(FlightPoint takeOffPoint)
		{
			Site? output = null;
			string sqlGetAllSites = @"SELECT Site_ID, Name, Town, Country, WindOrientationBegin, WindOrientationEnd, Altitude, Latitude, Longitude
									FROM Sites";
			List<Site> sites = _db.LoadData<Site, dynamic>(sqlGetAllSites, new { }, LoadConnectionString());

			output = sites.Where(s => takeOffPoint
			.DistanceFrom(new FlightPoint()
			{
				Longitude = s.Longitude,
				Latitude = s.Latitude,
				Height = s.Altitude
			}) < s.SiteRadius).
				FirstOrDefault();
			if (output == null)
			{
				List<Site> unknownSiteNames = Sites.Where(s => s.Name.Contains("Unknown site")).ToList();
				int nextUnknownSite = unknownSiteNames.Count;
				output = new Site()
				{
					Altitude = takeOffPoint.Height,
					Latitude = takeOffPoint.Latitude,
					Longitude = takeOffPoint.Longitude,
					Name = $"Unknown site {nextUnknownSite}"
				};
				WriteSitesInDB(new List<Site> { output });
			}
			return output;
		}

		private string LoadConnectionString(string connectionStringName = "Sqlite")
		{
			return _config.GetConnectionString(connectionStringName)!;
		}
		/// <summary>
		/// The take off time in UTC as a timestamp based on the igc data (date in meta data and time as the timestamp of the first sample)
		/// </summary>
		private DateTime GetTakeOffTimeFromIgcContent(string igcContent)
		{
			const string FLIGHT_TIME_REGEXP = @"B(?<h>\d\d)(?<m>\d\d)(?<s>\d\d)";
			const string FLIGHT_DATE_REGEXP = @"HFDTE(DATE:)?(?<d>\d\d)(?<m>\d\d)(?<y>\d\d)";
			const int MILLENAR = 2000;

			string l_igcAllInOneLine = igcContent.Replace("\r", "").Replace("\n", "");
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

				return new DateTime(l_FlightYear, l_FlightMonth, l_FlightDay, l_FlightHour, l_FlightMinute, l_FlightSecond);
			}
			return DateTime.MinValue;
		}
		private TimeSpan GetFlightDurationFromPointList(List<FlightPoint> flightPoints)
		{
			return flightPoints.Count > 0 ? new TimeSpan(0, 0, flightPoints.Count) : TimeSpan.MinValue;
		}

		private double GetTakeOffAltitudeFromPointList(List<FlightPoint> flightPoints)
		{
			return flightPoints.Count > 0 ? flightPoints[0].Height : double.NaN;
		}
		private FlightPoint GetTakeOffPointFromPointList(List<FlightPoint> flightPoints)
		{
			return flightPoints.Count > 0 ? flightPoints[0] : new FlightPoint() { Latitude = double.NaN, Longitude = double.NaN, Height = double.NaN };
		}
		private List<FlightPoint> GetFlightPointsFromIgcContent(string igcContent)
		{
			List<FlightPoint> output = new();
			FlightPoint l_flightPoint;
			foreach (string line in igcContent.Split("\r\n"))
			{
				if (ParseIGCFlightData(line, out l_flightPoint))
				{
					output.Add(l_flightPoint);
				}
			}
			return output;
		}

		private string GetGliderNameFromIgcContent(string igcContent)
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
		/// Parse the IGC_Line as coordinate. Highly inspired from https://github.com/ringostarr80/RL.Geo/blob/master/RL.Geo/Gps/Serialization/IgcDeSerializer.cs
		/// return true if it succeed false otherwise
		/// </summary>
		/// <param name="IGC_Line"></param>
		/// <param name="parsedFlightPoint"></param>
		/// <returns></returns>
		private bool ParseIGCFlightData(string IGC_Line, out FlightPoint parsedFlightPoint)
		{

			const string COORDINATE_REGEX = @"^B(?<UTCTimeHour>\d\d)(?<UTCTimeMinute>\d\d)(?<UTCTimeSecond>\d\d)(?<d1>\d\d)(?<m1>\d\d\d\d\d)(?<dir1>[NnSs])(?<d2>\d\d\d)(?<m2>\d\d\d\d\d)(?<dir2>[EeWw])A(?<BaroAlt>\d\d\d\d\d)(?<GPS_Alt>\d\d\d\d\d)";
			var match = Regex.Match(IGC_Line, COORDINATE_REGEX);
			if (match.Success)
			{
				var deg1 = double.Parse(match.Groups["d1"].Value, CultureInfo.InvariantCulture) + double.Parse(match.Groups["m1"].Value, CultureInfo.InvariantCulture) / 1000 / 60;
				var dir1 = Regex.IsMatch(match.Groups["dir1"].Value, "[Ss]") ? -1d : 1d;
				var deg2 = double.Parse(match.Groups["d2"].Value, CultureInfo.InvariantCulture) + double.Parse(match.Groups["m2"].Value, CultureInfo.InvariantCulture) / 1000 / 60;
				var dir2 = Regex.IsMatch(match.Groups["dir2"].Value, "[Ww]") ? -1d : 1d;
				var height = double.Parse(match.Groups["GPS_Alt"].Value, CultureInfo.InvariantCulture);
				parsedFlightPoint = new FlightPoint() { Latitude = deg1 * dir1, Longitude = deg2 * dir2, Height = height };
				return true;
			}
			parsedFlightPoint = new FlightPoint() { Height = double.NaN, Latitude = double.NaN, Longitude = double.NaN };
			return false;

		}
	}
}



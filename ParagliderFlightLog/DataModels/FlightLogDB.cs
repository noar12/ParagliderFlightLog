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

namespace ParagliderFlightLog.DataModels
{
    public class FlightLogDB
    {
        private ObservableCollection<Flight> m_flights = new ObservableCollection<Flight>();
        private ObservableCollection<Site> m_sites = new ObservableCollection<Site>();
        private ObservableCollection<Glider> m_gliders = new ObservableCollection<Glider>();

        //private const string DB_PATH = @"C:\Users\mar\source\repos\noar12\ParagliderFlightLog\ParagliderFlightLog.db";
        //private const string DB_PATH = @"C:\Users\arnau\source\repos\ParagliderFlightLog\ParagliderFlightLog.db";
        private Settings m_Settings;

        public FlightLogDB(Settings settings)
        {

            m_flights.CollectionChanged += FlightsCollectionChangedHandler;
            m_sites.CollectionChanged += SitesCollectionChangedHandler;
            m_gliders.CollectionChanged += GliderCollectionChangedHandler;
            m_Settings = settings;
        }


        public void LoadFlightLogDB()
        {
            if (File.Exists(m_Settings.DbPath))
            {
                string sqlGetAllSite = "SELECT Site_ID, Name, Town, Country, WindOrientationBegin, WindOrientationEnd, Altitude, Latitude, Longitude FROM Sites";
                string sqlGetAllGlider = "SELECT Glider_ID, Manufacturer, Model, BuildYear, LastCheckDateTime, HomologationCategory, IGC_Name FROM Gliders";
                string sqlGetAllFlight = "SELECT Flight_ID, IgcFileContent, Comment, REF_TakeOffSite_ID, REF_Glider_ID, FlightDuration_s, TakeOffDateTime FROM Flights";

                using (SQLiteConnection conn = new SQLiteConnection(LoadConnectionString(m_Settings.DbPath)))
                {
                    m_sites = new ObservableCollection<Site>(conn.Query<Site>(sqlGetAllSite));
                    m_gliders = new ObservableCollection<Glider>(conn.Query<Glider>(sqlGetAllGlider));
                    m_flights = new ObservableCollection<Flight>(conn.Query<Flight>(sqlGetAllFlight));

                    m_flights.CollectionChanged += FlightsCollectionChangedHandler;
                    m_sites.CollectionChanged += SitesCollectionChangedHandler;
                    m_gliders.CollectionChanged += GliderCollectionChangedHandler;
                }

            }


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

            if (!File.Exists(m_Settings.DbPath))
            {
                SQLiteConnection.CreateFile(m_Settings.DbPath);
                using (SQLiteConnection conn = new SQLiteConnection(LoadConnectionString(m_Settings.DbPath)))
                {
                    conn.Open();
                    SQLiteCommand CreateSitesCommand = new SQLiteCommand(sqlCreateSites, conn);
                    SQLiteCommand CreateGlidersCommand = new SQLiteCommand(sqlCreateGliders, conn);
                    SQLiteCommand CreateFlightsCommand = new SQLiteCommand(sqlCreateFlights, conn);

                    CreateSitesCommand.ExecuteNonQuery();
                    CreateGlidersCommand.ExecuteNonQuery();
                    CreateFlightsCommand.ExecuteNonQuery();
                    conn.Close();
                }
            }
        }
        private void FlightsCollectionChangedHandler(object? sender, NotifyCollectionChangedEventArgs e)
        {
            //System.Diagnostics.Debug.WriteLine("Collection changed triggered");
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    WriteFlightsInDB(e.NewItems);
                    break;
                case NotifyCollectionChangedAction.Remove:
                    DeleteFlightsInDB(e.OldItems);
                    break;
                case NotifyCollectionChangedAction.Replace:
                    ReplaceFlightInDB(e.NewItems);
                    break;
                default:
                    throw new NotSupportedException();

            }

        }

        private void ReplaceFlightInDB(IList? newItems)
        {
            if (!File.Exists(m_Settings.DbPath))
                CreateFlightLogDB();
            if (newItems != null)
            {
                string sqlReplaceFlight = "UPDATE Flights SET Comment = @Comment, REF_TakeOffSite_ID = @REF_TakeOffSite_ID, REF_Glider_ID = @REF_Glider_ID, FlightDuration_s = @FlightDuration_s, TakeOffDateTime = @TakeOffDateTime, IgcFileContent = @IgcFileContent WHERE Flight_ID = @Flight_ID";
                using (SQLiteConnection conn = new SQLiteConnection(LoadConnectionString(m_Settings.DbPath)))
                {

                    foreach (Flight flight in newItems)
                    {
                        conn.Execute(sqlReplaceFlight, flight);
                    }

                }
            }
        }

        private void GliderCollectionChangedHandler(object? sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    WriteGlidersInDB(e.NewItems);
                    break;
                case NotifyCollectionChangedAction.Remove:
                    DeleteGlidersInDB(e.OldItems);
                    break;
                case NotifyCollectionChangedAction.Replace:
                    ReplaceGlidersInDB(e.NewItems);
                    break;
                default:
                    throw new NotSupportedException();
            }
            
        }



        private void SitesCollectionChangedHandler(object? sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    WriteSitesInDB(e.NewItems);
                    break;
                case NotifyCollectionChangedAction.Remove:
                    DeleteSitesInDB(e.OldItems);
                    break;
                case NotifyCollectionChangedAction.Replace:
                    ReplaceSitesInDB(e.NewItems);
                    break;
                default:
                    throw new NotSupportedException();
            }
        }

        private void ReplaceSitesInDB(IList? newItems)
        {
            if (!File.Exists(m_Settings.DbPath))
                CreateFlightLogDB();
            if (newItems != null)
            {
                string sqlReplaceSite = "UPDATE Sites SET Name = @Name, Town = @Town, Country = @Country, WindOrientationBegin = @WindOrientationBegin, WindOrientationEnd = @WindOrientationEnd, Altitude = @Altitude, Latitude = @Latitude, Longitude = @Longitude WHERE Site_ID = @Site_ID";
                using (SQLiteConnection conn = new SQLiteConnection(LoadConnectionString(m_Settings.DbPath)))
                {

                    foreach (Site site in newItems)
                    {
                        conn.Execute(sqlReplaceSite, site);
                    }

                }
            }
        }

        private void DeleteSitesInDB(IList? oldItems)
        {
            throw new NotImplementedException();
        }

        private void WriteSitesInDB(IList? newItems)
        {
            if (!File.Exists(m_Settings.DbPath))
                CreateFlightLogDB();
            if (newItems != null)
            {
                string sqlWriteSite = "INSERT INTO Sites (Site_ID, Name, Town, Country, WindOrientationBegin, WindOrientationEnd, Altitude, Latitude, Longitude) VALUES(@Site_ID, @Name, @Town, @Country, @WindOrientationBegin, @WindOrientationEnd, @Altitude, @Latitude, @Longitude)";
                using (SQLiteConnection conn = new SQLiteConnection(LoadConnectionString(m_Settings.DbPath)))
                {

                    foreach (Site site in newItems)
                    {
                        conn.Execute(sqlWriteSite, site);
                    }

                }
            }
        }

        private void DeleteFlightsInDB(IList? oldItems)
        {
            if (!File.Exists(m_Settings.DbPath))
                CreateFlightLogDB();
            if (oldItems != null)
            {
                string sqlDeleteFlight = "DELETE FROM Flights WHERE Flight_ID = @Flight_ID";
                using (SQLiteConnection conn = new SQLiteConnection(LoadConnectionString(m_Settings.DbPath)))
                {
                    foreach (Flight flight in oldItems)
                    {
                        conn.Execute(sqlDeleteFlight, flight);
                    }
                }

            }
        }

        private void WriteFlightsInDB(IList? newItems)
        {
            if (!File.Exists(m_Settings.DbPath))
                CreateFlightLogDB();
            if (newItems != null)
            {
                string sqlWriteFlight = "INSERT INTO Flights (Flight_ID, IgcFileContent, Comment, REF_TakeOffSite_ID, REF_Glider_ID, TakeOffDateTime, FlightDuration_s) VALUES(@Flight_ID, @IgcFileContent, @Comment, @REF_TakeOffSite_ID, @REF_Glider_ID, @TakeOffDateTime, @FlightDuration_s)";
                using (SQLiteConnection conn = new SQLiteConnection(LoadConnectionString(m_Settings.DbPath)))
                {

                    foreach (Flight flight in newItems)
                    {
                        conn.Execute(sqlWriteFlight, flight);
                    }

                }
            }
        }

        private void DeleteGlidersInDB(IList? oldItems)
        {
            throw new NotImplementedException();
        }
        private void ReplaceGlidersInDB(IList? newItems)
        {
            if (!File.Exists(m_Settings.DbPath))
                CreateFlightLogDB();
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
                using (SQLiteConnection conn = new SQLiteConnection(LoadConnectionString(m_Settings.DbPath)))
                {

                    foreach (Glider glider in newItems)
                    {
                        conn.Execute(sqlReplaceGlider, glider);
                    }

                }
            }
        }
        private void WriteGlidersInDB(IList? newItems)
        {
            if (!File.Exists(m_Settings.DbPath))
                CreateFlightLogDB();
            if (newItems != null)
            {
                string sqlWriteGlider = "INSERT INTO Gliders (Glider_ID, Manufacturer, Model, BuildYear, LastCheckDateTime, HomologationCategory, IGC_Name) VALUES (@Glider_ID, @Manufacturer, @Model, @BuildYear, @LastCheckDateTime, @HomologationCategory, @IGC_Name)";
                using (SQLiteConnection conn = new SQLiteConnection(LoadConnectionString(m_Settings.DbPath)))
                {

                    foreach (Glider glider in newItems)
                    {
                        conn.Execute(sqlWriteGlider, glider);
                    }

                }
            }
        }

        public ObservableCollection<Flight> Flights { get => m_flights; set => m_flights = value; }
        public ObservableCollection<Site> Sites { get => m_sites; set => m_sites = value; }
        public ObservableCollection<Glider> Gliders { get => m_gliders; set => m_gliders = value; }
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
        public void ImportFlightFromIGC(string IGC_FilePath)
        {
            Flight l_Newflight = new Flight();
            
            using (var sr = new StreamReader(IGC_FilePath))
            {
                // to be done: check if it is a correct igc file before injecting
                l_Newflight.IgcFileContent = sr.ReadToEnd();
            }

            if (l_Newflight.IGC_GliderName != "")
            {
                // A glider is defined in the IGC so we will try to match it with an existing one
                Glider? l_Glider = Gliders.FirstOrDefault(g => g.IGC_Name == l_Newflight.IGC_GliderName);
                if ( l_Glider != null) 
                {
                    l_Newflight.REF_Glider_ID = l_Glider.Glider_ID;
                }
            }

            // search for a take off site
            Site? l_TakeOffSite = Sites.
                Where(s => l_Newflight.TakeOffPoint.DistanceFrom(new FlightPoint() { Longitude = s.Longitude, Latitude = s.Latitude, Height = s.Altitude }) < s.SiteRadius).
                FirstOrDefault();
            if (l_TakeOffSite != null)
                l_Newflight.REF_TakeOffSite_ID = l_TakeOffSite.Site_ID;
            else
            {
                l_TakeOffSite = new Site()
                {
                    Name = "Unknown site",
                    Latitude = l_Newflight.TakeOffPoint.Latitude,
                    Longitude = l_Newflight.TakeOffPoint.Longitude,
                    Altitude = l_Newflight.TakeOffAltitude,
                };
                Sites.Add(l_TakeOffSite);
                l_Newflight.REF_TakeOffSite_ID = l_TakeOffSite.Site_ID;
            }

            

            // check if we were able to parse some point before inserting the new flight
            if (l_Newflight.FlightPoints.Any())
            { 
                m_flights.Add(l_Newflight);
                
            }
            else
            {
                throw new Exception();
            }
        }

        private static string LoadConnectionString(string DB_Path)
        {
            // "Data Source=./<relativePathToSqliteDataBase;Version=3;"
            return $"Data Source={ DB_Path };Version=3;";

        }
    }

}

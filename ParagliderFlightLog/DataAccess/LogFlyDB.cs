using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SQLite;
using Dapper;
using ParagliderFlightLog.Models;
using Microsoft.Extensions.Configuration;

namespace ParagliderFlightLog.DataAccess
{
    /// <summary>
    /// Provide a data model to read and convert SQLite database from LogFly 5.
    /// </summary>
    public class LogFlyDB
    {

        private FlightLogDB _flightLogDB;
        private List<LogFlyVol> m_LogFlyVolCollection = new List<LogFlyVol>();
        private List<LogFlySite> m_LogFlySiteCollection = new List<LogFlySite>();

        public LogFlyDB(FlightLogDB flightLogDb)
        {
            _flightLogDB = flightLogDb;
        }
        // Define arbitrary glider because there are not define in logfly db
        private List<Glider> m_LogFlyGliderCollection = new List<Glider>()
        {
            new Glider(){
                Glider_ID = Guid.NewGuid().ToString(),
                Manufacturer = "Skywalk",
                Model = "Mescal 4",
                IGC_Name = "Mescal 4",
                BuildYear = 2015,
                HomologationCategory = EHomologationCategory.ENALow,
                LastCheckDateTime = DateTime.Parse("2019-11-01"),
                LastCheckDateTimeSpecified = true // Is it realy necessary ?
            },
            new Glider(){
                Glider_ID = Guid.NewGuid().ToString(),
                Manufacturer = "Advance",
                Model = "Epsilon 9",
                IGC_Name = "Epsilon 9",
                BuildYear = 2019,
                HomologationCategory = EHomologationCategory.ENBLow,
                LastCheckDateTime = DateTime.Parse("2021-02-01"),
                LastCheckDateTimeSpecified = true // Is it realy necessary ?
            }
        };

        /// <summary>
        /// Load all the data from a LogFly DB and put them in the instance field.
        /// </summary>
        /// <param name="DB_Path"></param>
        public void LoadLogFlyDB(string DB_Path)
        {
            string SqlGetAllSite = "SELECT S_ID, S_Nom, S_Alti, S_Latitude, S_Longitude, S_Commentaire FROM Site";
            string SqlGetAllVol = "SELECT V_ID, V_Date, V_Duree, V_LatDeco, V_LongDeco, V_AltDeco, V_Site, V_Commentaire, V_IGC, UTC, V_Engin, V_League, V_Score FROM Vol";

            using (SQLiteConnection conn = new SQLiteConnection(LoadConnectionString(DB_Path)))
            {
                m_LogFlySiteCollection = conn.Query<LogFlySite>(SqlGetAllSite).ToList();
                m_LogFlyVolCollection = conn.Query<LogFlyVol>(SqlGetAllVol).ToList();

            }

        }
        /// <summary>
        /// Return a FlightLogDB based on the LogFly Site and Flight collection of the instance. Glider is a constant arbitrary collection (to be changed...).
        /// </summary>
        /// <returns></returns>
        public FlightLogDB BuildFlightLogDB()
        {
            List<Site> l_sites = new List<Site>();
            List<Flight> l_flights = new List<Flight>();


            foreach (var site in m_LogFlySiteCollection)
            {
                l_sites.Add(site.ToFlightLogSite());

            }


            foreach (var vol in m_LogFlyVolCollection)
            {
                l_flights.Add(vol.ToFlightLogDBFlight(l_sites, m_LogFlyGliderCollection));
            }

            //_flightLogDB.Flights = l_flights;
            //_flightLogDB.Sites = l_sites;
            //_flightLogDB.Gliders = new List<Glider>(m_LogFlyGliderCollection);

            return _flightLogDB;
        }

        private static string LoadConnectionString(string DB_Path)
        {
            // "Data Source=./<relativePathToSqliteDataBase;Version=3;"
            return $"Data Source={DB_Path};Version=3;";

        }
    }

    class LogFlySite
    {
        //CREATE TABLE Site(S_ID integer NOT NULL primary key,S_Nom varchar(50),S_Localite varchar(50),S_CP varchar(8),S_Pays varchar(50),S_Type varchar(1),
        //S_Orientation varchar(20),S_Alti varchar(12),S_Latitude double,S_Longitude double,S_Commentaire Long Text,S_Maj varchar(10))
        private long m_S_ID;
        private string m_S_Nom = "";
        private string m_S_Type = "";
        private int m_S_Alti;
        private double m_S_Latitude;
        private double m_S_Longitude;
        private string m_S_Commentaire = "";

        public long S_ID { get => m_S_ID; set => m_S_ID = value; }
        public string S_Nom { get => m_S_Nom; set => m_S_Nom = value; }
        public string S_Type { get => m_S_Type; set => m_S_Type = value; }
        public int S_Alti { get => m_S_Alti; set => m_S_Alti = value; }
        public double S_Latitude { get => m_S_Latitude; set => m_S_Latitude = value; }
        public double S_Longitude { get => m_S_Longitude; set => m_S_Longitude = value; }
        public string S_Commentaire { get => m_S_Commentaire; set => m_S_Commentaire = value; }

        internal Site ToFlightLogSite()
        {
            Site l_Site = new Site();
            l_Site.Site_ID = Guid.NewGuid().ToString();
            l_Site.Name = S_Nom;
            l_Site.Longitude = S_Longitude;
            l_Site.Latitude = S_Latitude;
            l_Site.Altitude = S_Alti;

            return l_Site;
        }
    }

    class LogFlyVol
    {
        //CREATE TABLE Vol(V_ID integer NOT NULL PRIMARY KEY, V_Date TimeStamp, V_Duree integer, V_sDuree varchar(20), V_LatDeco double, V_LongDeco double,
        //V_AltDeco integer, V_Site varchar(100), V_Pays varchar(50), V_Commentaire Long Text, V_IGC Long Text, V_Photos Long Text,UTC integer,
        //V_CFD integer,V_Engin Varchar(10), V_League integer, V_Score Long Text)
        private long m_V_ID;
        private DateTime m_V_Date;
        private long m_V_Duree;
        private double m_V_LatDeco;
        private double m_V_LongDeco;
        private long m_V_AltDeco;
        private string m_V_Site = "";
        private string m_V_Commentaire = "";
        private string m_V_IGC = "";
        private long m_UTC;
        private string m_V_Engin = "";
        private int m_V_League;
        private string m_V_Score = "";

        public long V_ID { get => m_V_ID; set => m_V_ID = value; }
        public DateTime V_Date { get => m_V_Date; set => m_V_Date = value; }
        public long V_Duree { get => m_V_Duree; set => m_V_Duree = value; }
        public double V_LatDeco { get => m_V_LatDeco; set => m_V_LatDeco = value; }
        public double V_LongDeco { get => m_V_LongDeco; set => m_V_LongDeco = value; }
        public long V_AltDeco { get => m_V_AltDeco; set => m_V_AltDeco = value; }
        public string V_Site { get => m_V_Site; set => m_V_Site = value; }
        public string V_Commentaire { get => m_V_Commentaire; set => m_V_Commentaire = value; }
        public string V_IGC { get => m_V_IGC; set => m_V_IGC = value; }
        public long UTC { get => m_UTC; set => m_UTC = value; }
        public string V_Engin { get => m_V_Engin; set => m_V_Engin = value; }
        public int V_League { get => m_V_League; set => m_V_League = value; }
        public string V_Score { get => m_V_Score; set => m_V_Score = value; }

        internal Flight ToFlightLogDBFlight(List<Site> sites, List<Glider> gliders)
        {
            Flight l_Flight = new Flight();
            l_Flight.Flight_ID = Guid.NewGuid().ToString();
            l_Flight.FlightDuration = new TimeSpan(0, 0, (int)V_Duree);
            l_Flight.Comment = V_Commentaire;
            if (V_IGC == "")
            {
                l_Flight.TakeOffDateTime = V_Date;
            }
            l_Flight.IgcFileContent = V_IGC;

            // find the guid ref in glider and in site corresponding to m_V_Engin and m_V_Site
            l_Flight.REF_Glider_ID = (from glider in gliders where glider.Model == V_Engin select glider.Glider_ID).FirstOrDefault("");
            l_Flight.REF_TakeOffSite_ID = (from site in sites where site.Name == V_Site select site.Site_ID).FirstOrDefault("");
            return l_Flight;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using Microsoft.Data.Sqlite;
using Dapper;
using ParagliderFlightLog.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace ParagliderFlightLog.DataAccess
{
    /// <summary>
    /// Provide a data model to read and convert SQLite database from LogFly 5.
    /// </summary>
    public class LogFlyDB
    {
        private readonly FlightLogDB _flightLogDB;
        private List<LogFlyVol> m_LogFlyVolCollection = [];
        private List<LogFlySite> m_LogFlySiteCollection = [];
        public LogFlyDB(FlightLogDB flightLogDb)
        {
            _flightLogDB = flightLogDb;
        }

        /// <summary>
        /// Load all the data from a LogFly DB and put them in the instance field.
        /// </summary>
        /// <param name="DB_Path"></param>
        public void LoadLogFlyDB(string DB_Path)
        {
            string SqlGetAllSite = "SELECT S_ID, S_Nom, S_Alti, S_Latitude, S_Longitude, S_Commentaire FROM Site";
            string SqlGetAllVol = "SELECT V_ID, V_Date, V_Duree, V_LatDeco, V_LongDeco, V_AltDeco, V_Site, V_Commentaire, V_IGC, UTC, V_Engin, V_League, V_Score FROM Vol";

            using var conn = new SqliteConnection(LoadConnectionString(DB_Path));
            m_LogFlySiteCollection = conn.Query<LogFlySite>(SqlGetAllSite).ToList();
            m_LogFlyVolCollection = conn.Query<LogFlyVol>(SqlGetAllVol).ToList();

        }
        /// <summary>
        /// Import in FlightLogDB based on the LogFly Site and Flight collection of the instance. Glider are created based on thier name in V_Engin
        /// </summary>
        /// <returns></returns>
        public (int importedSitesCount, int importedGlidersCount, int importedFlightsCount) ImportInFlightLogDB()
        {
            List<Site> l_sites = m_LogFlySiteCollection.Select(s => s.ToFlightLogSite()).ToList();
            List<Glider> l_gliders = GetAllGlidersAsFlightLogDbGlider();
            List<FlightWithData> l_flights = m_LogFlyVolCollection.Select(v => v.ToFlightLogDBFlightWithData(l_sites, l_gliders)).ToList();

            _flightLogDB.WriteSitesInDB(l_sites);
            _flightLogDB.WriteGlidersInDB(l_gliders);
            _flightLogDB.WriteFlightsInDB(l_flights);

            return (l_sites.Count, l_gliders.Count, l_flights.Count);

        }

        private List<Glider> GetAllGlidersAsFlightLogDbGlider()
        {
            List<Glider> output = [];
            List<string> gliderNames = m_LogFlyVolCollection.Select(v => v.V_Engin).Distinct().ToList();

            foreach (var gliderName in gliderNames)
            {
                Glider glider = new() { Model = gliderName, IGC_Name = gliderName };
                output.Add(glider);
            }
            return output;
        }

        private static string LoadConnectionString(string DB_Path)
        {
            // "Data Source=./<relativePathToSqliteDataBase;Version=3;"
            return $"Data Source={DB_Path};Version=3;";

        }
    }
#pragma warning disable IDE1006 // Naming Styles following legacy naming from LogFly original db
    class LogFlySite
    {
        public long S_ID { get; set; }

        public string S_Nom { get; set; } = "";
        public string S_Type { get; set; } = "";
        public int S_Alti { get; set; }
        public double S_Latitude { get; set; }
        public double S_Longitude { get; set; }
        public string S_Commentaire { get; set; } = "";

        internal Site ToFlightLogSite()
        {
            var l_Site = new Site
            {
                Site_ID = Guid.NewGuid().ToString(),
                Name = S_Nom,
                Longitude = S_Longitude,
                Latitude = S_Latitude,
                Altitude = S_Alti
            };

            return l_Site;
        }
    }

    class LogFlyVol
    {
        public long V_ID { get; set; }
        public DateTime V_Date { get; set; }
        public long V_Duree { get; set; }
        public double V_LatDeco { get; set; }
        public double V_LongDeco { get; set; }
        public long V_AltDeco { get; set; }
        public string V_Site { get; set; } = "";
        public string V_Commentaire { get; set; } = "";
        public string V_IGC { get; set; } = "";
        public long UTC { get; set; }
        public string V_Engin { get; set; } = "";
        public int V_League { get; set; }
        public string V_Score { get; set; } = "";

        internal FlightWithData ToFlightLogDBFlightWithData(List<Site> sites, List<Glider> gliders)
        {
            var l_Flight = new FlightWithData
            {
                Flight_ID = Guid.NewGuid().ToString(),
                FlightDuration = new TimeSpan(0, 0, (int)V_Duree),
                Comment = V_Commentaire,

                TakeOffDateTime = V_Date,

                IgcFileContent = V_IGC,

                // find the guid ref in glider and in site corresponding to m_V_Engin and m_V_Site
                REF_Glider_ID = (from glider in gliders where glider.Model == V_Engin select glider.Glider_ID).FirstOrDefault(""),
                REF_TakeOffSite_ID = (from site in sites where site.Name == V_Site select site.Site_ID).FirstOrDefault("")
            };
            return l_Flight;
        }
    }
}
#pragma warning restore IDE1006 // Naming Styles

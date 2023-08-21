using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BlazorUI.ViewModels;
using ParagliderFlightLog.DataAccess;
using ParagliderFlightLog.Models;

namespace ParagliderFlightLog.ViewModels
{
    public class FlightViewModel
    {
        private const int INTEGRATION_STEP = 8;
        private Flight m_Flight = new Flight();
        private readonly FlightLogDB _db;
        private ICollection<Flight> m_FlightCollection = new List<Flight>();
        private ICollection<Site> m_SiteCollection = new List<Site>();
        private ICollection<Glider> m_GliderCollection = new List<Glider>();

        public FlightViewModel(Flight flight, FlightLogDB db)
        {
            m_Flight = flight;
            _db = db;
        }
        public FlightViewModel() { }
        //public FlightViewModel(string flightID, DateTime takeOffDateTime, TimeSpan flightDuration, string takeOffSiteName, string gliderName, List<FlightPoint> flightPoints, string comment)
        //{
        //    FlightID = flightID;
        //    TakeOffDateTime = takeOffDateTime;
        //    FlightDuration = flightDuration;
        //    TakeOffSiteName = takeOffSiteName;
        //    GliderName = gliderName;
        //    FlightPoints = flightPoints;
        //    Comment = comment;
        //}


        // to do: property have to get from flight ref and set to flight ref
        public string FlightID { get { return m_Flight.Flight_ID; } }
        public Flight Flight
        {
            get { return m_Flight; }
        }
        public DateTime TakeOffDateTime { get { return m_Flight.TakeOffDateTime; } }
        public TimeSpan FlightDuration { get { return m_Flight.FlightDuration; } }
        public string TakeOffSiteID { get { return m_Flight.REF_TakeOffSite_ID; } }
        public SiteViewModel TakeOffSite { get; set; }
        public string TakeOffSiteName
        {
            get
            {
                return _db.GetFlightTakeOffSite(Flight)?.Name ?? "Site not found";
            }

        }
        public string GliderName
        {
            get
            {
                return _db.GetFlightGlider(Flight)?.FullName ?? "Glider not found";
            }
        }
        public GliderViewModel? Glider
        {
            get { return _db.GetFlightGlider(Flight)?.ToVM(_db); }
            set {
                Flight.REF_Glider_ID = value?.GliderId ?? "";
            _db.UpdateFlightGlider(Flight, value?.Glider); }
        }
        public double MaxHeight
        {
            get
            {
                if (FlightPoints.Count > 0)
                {
                    return m_Flight.FlightPoints.Select(fp => fp.Height).ToList().Max();
                }
                else
                {
                    return 0.0;
                }
            }
        }
        public List<FlightPoint> FlightPoints { get { return m_Flight.FlightPoints; } }
        public string Comment { get { return _db.GetFlightComment(Flight) ?? ""; } set { _db.UpdateFlightComment(Flight, value); } }
        /// <summary>
        /// Get the trace length in km
        /// </summary>
        public double TraceLength
        {
            get
            {
                double l_TraceLegnth = 0.0;
                for (int i = 1; i < FlightPoints.Count; i++)
                {
                    l_TraceLegnth += FlightPoints[i].DistanceFrom(FlightPoints[i - 1]) / 1000;
                }
                return l_TraceLegnth;
            }
        }
        /// <summary>
        /// Maximum climb on the flight integrated over 8 secondes.
        /// </summary>
        public double MaxClimb
        {
            get
            {
                if (FlightPoints.Count > 0)
                {
                    return GetVerticalRate(INTEGRATION_STEP).Max();
                }
                else
                {
                    return 0.0;
                }
            }
        }
        /// <summary>
        /// Maximum sink on the flight integrated over 8 secondes.
        /// </summary>
        public double MaxSink
        {
            get
            {
                if (FlightPoints.Count > 0)
                    return GetVerticalRate(INTEGRATION_STEP).Min();
                else
                    return 0.0;
            }
        }
        public void RemoveFlight()
        {
            m_FlightCollection.Remove(m_Flight);
        }
        public void AddFlight()
        {
            m_FlightCollection.Add(m_Flight);
        }

        private double[] GetVerticalRate(int integrationStep)
        {
            if (FlightPoints.Count != 0)
            {
                List<double> l_verticalRates = new List<double>();
                for (int i = integrationStep; i < m_Flight.FlightPoints.Count; i++)
                {
                    l_verticalRates.Add((FlightPoints[i].Height - FlightPoints[i - INTEGRATION_STEP].Height) / INTEGRATION_STEP);
                }
                return l_verticalRates.ToArray();
            }
            return new double[0];

        }

    }


}

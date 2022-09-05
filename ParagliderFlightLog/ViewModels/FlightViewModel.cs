using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ParagliderFlightLog.DataModels;

namespace ParagliderFlightLog.ViewModels
{
    public class FlightViewModel
    {
        private const int INTEGRATION_STEP = 8;
        private Flight m_Flight = new Flight();
        private ICollection<Flight> m_FlightCollection = new List<Flight>();
        private ICollection<Site> m_SiteCollection = new List<Site>();
        private ICollection<Glider> m_GliderCollection = new List<Glider>();
        
        public FlightViewModel(Flight flight, ICollection<Flight> flights, ICollection<Site> sites, ICollection<Glider> gliders)
        {
            m_Flight = flight;
            m_FlightCollection = flights;
            m_SiteCollection = sites;
            m_GliderCollection = gliders;

        }
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


        // to do: property have to get from flight ref et set to flight ref
        public string FlightID { get { return m_Flight.Flight_ID; } }
        public Flight Flight
        {
            get { return m_Flight; }
            set
            {
                m_Flight = value;
            }
        }
        public DateTime TakeOffDateTime { get { return m_Flight.TakeOffDateTime; } }
        public TimeSpan FlightDuration { get { return m_Flight.FlightDuration; } }
        public string TakeOffSiteID { get { return m_Flight.REF_TakeOffSite_ID; } }
        public string TakeOffSiteName
        {
            get { return m_SiteCollection.Where(s => s.Site_ID == m_Flight.REF_TakeOffSite_ID).FirstOrDefault(new Site()).Name; }
            set
            {
                Site? l_ProposedSite = m_SiteCollection.Where(s => s.Name == value).FirstOrDefault();
                if ( l_ProposedSite != null)
                {
                    m_Flight.REF_TakeOffSite_ID = l_ProposedSite.Site_ID;
                }
                else
                {
                    throw new ArgumentException("The proposed site was not found in the data");
                }
                
            }
        }
        public string GliderName
        {
            get { return m_GliderCollection.Where(g => g.Glider_ID == m_Flight.REF_Glider_ID).FirstOrDefault(new Glider()).FullName; }
            set
            {
                Glider? l_ProposedGlider = m_GliderCollection.Where(g => g.FullName == value).FirstOrDefault();
                if(l_ProposedGlider != null)
                {
                    m_Flight.REF_Glider_ID = l_ProposedGlider.Glider_ID;
                }
                else
                {
                    throw new ArgumentException("The proposed glider was not found in the data");
                }
            }
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
        public string Comment { get { return m_Flight.Comment; } set { m_Flight.Comment = value; } }
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
                    l_TraceLegnth +=FlightPoints[i].DistanceFrom(FlightPoints[i - 1])/1000;
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

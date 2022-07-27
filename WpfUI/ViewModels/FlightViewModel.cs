using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ParagliderFlightLog.DataModels;

namespace WpfUI.ViewModels
{
    public class FlightViewModel
    {
        private Flight m_Flight = new Flight();
        private ICollection<Site> m_SiteCollection = new List<Site>();
        private ICollection<Glider> m_GliderCollection = new List<Glider>();
        // to do: Constructor FlightViewModel(Flight flight);
        public FlightViewModel(Flight flight, ICollection<Site> sites, ICollection<Glider> gliders)
        {
            m_Flight = flight;
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
        public DateTime TakeOffDateTime { get { return m_Flight.TakeOffDateTime; } }
        public TimeSpan FlightDuration { get { return m_Flight.FlightDuration; } }
        public string TakeOffSiteName { get { return m_SiteCollection.Where(s => s.Site_ID == m_Flight.REF_TakeOffSite_ID).FirstOrDefault(new Site()).Name; } }
        public string GliderName { get { return m_GliderCollection.Where(g => g.Glider_ID == m_Flight.REF_Glider_ID).FirstOrDefault(new Glider()).Model; } }
        public double MaxHeight { get => m_Flight.FlightPoints.Select(fp => fp.Height).ToList().Max(); }
        public List<FlightPoint> FlightPoints { get { return m_Flight.FlightPoints; } }
        public string Comment { get { return m_Flight.Comment; } }
    }


}

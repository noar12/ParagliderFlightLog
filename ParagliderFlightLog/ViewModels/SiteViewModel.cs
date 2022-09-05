using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ParagliderFlightLog.DataModels;

namespace ParagliderFlightLog.ViewModels
{
    public class SiteViewModel
    {
        private Site m_Site = new Site();
        
        public SiteViewModel(Site site)
        {
            m_Site = site;
            
        }
        public SiteViewModel() { m_Site = new Site(); }

        public Site Site { get { return m_Site; } set { m_Site = value; } }
        public string Site_ID { get { return m_Site.Site_ID; } }
        public string Name
        {
            get { return m_Site.Name; }
            set { m_Site.Name = value; }
        }
        public double Altitude
        {
            get { return m_Site.Altitude; }
            set { m_Site.Altitude = value; }
        }
        public double Latitude
        {
            get { return m_Site.Latitude; }
            set { m_Site.Latitude = value; }
        }
        public double Longitude
        {
            get { return m_Site.Longitude; }
            set { m_Site.Longitude = value; }
        }
        public string Country
        {
            get { return m_Site.Country.ToString(); }
            set
            {
                Enum.TryParse<ECountry>(value, out ECountry l_ECountry);
                m_Site.Country = l_ECountry;
            }
        }
        public string Town
        {
            get { return m_Site.Town; }
            set { m_Site.Town = value; }
        }
        public string WindOrientation { get { return $"{WindOrientationBegin} - {WindOrientationEnd}"; } }
        public string WindOrientationBegin
        {
            get { return m_Site.WindOrientationBegin.ToString(); }
            set
            {
                Enum.TryParse<EWindOrientation>(value, out EWindOrientation l_EWindOrientation);
                m_Site.WindOrientationBegin = l_EWindOrientation;
            }
        }
        public string WindOrientationEnd
        {
            get { return m_Site.WindOrientationEnd.ToString(); }
            set
            {
                Enum.TryParse<EWindOrientation>(value, out EWindOrientation l_EWindOrientation);
                m_Site.WindOrientationEnd = l_EWindOrientation;
            }
        }
        public int GetSiteUseCount(IList<FlightViewModel> flights)
        {
            return flights.Where(f => f.TakeOffSiteID == this.Site_ID).Count();
        }
        public string[] AvailableWindOrientation { get => Enum.GetNames(typeof(EWindOrientation)); }
        public string[] AvailableCountry { get => Enum.GetNames(typeof(ECountry)); }


    }
}

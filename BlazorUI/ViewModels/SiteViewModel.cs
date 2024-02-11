using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ParagliderFlightLog.DataAccess;
using ParagliderFlightLog.Models;

namespace ParagliderFlightLog.ViewModels
{
	public class SiteViewModel
	{
		private readonly Site m_Site;
		private readonly FlightLogDB _db;

		public SiteViewModel(Site site, FlightLogDB db)
		{
			m_Site = site;
			_db = db;


		}

		public Site Site
		{
			get { return m_Site; }
			//set
			//{
			//	m_Site = value;
			//}
		}
		public string Site_ID { get { return m_Site.Site_ID; } }
		public string Name
		{
			get { return m_Site.Name; }
			set
			{
				m_Site.Name = value;
				_db.UpdateSite(m_Site);
			}
		}
		public double Altitude
		{
			get { return m_Site.Altitude; }
			set
			{
				m_Site.Altitude = value;
				_db.UpdateSite(m_Site);
			}
		}
		public double Latitude
		{
			get { return m_Site.Latitude; }
			set
			{
				m_Site.Latitude = value;
				_db.UpdateSite(m_Site);
			}
		}
		public double Longitude
		{
			get { return m_Site.Longitude; }
			set
			{
				m_Site.Longitude = value;
				_db.UpdateSite(m_Site);
			}
		}
		public string Country
		{
			get { return m_Site.Country.ToString(); }
			set
			{
				bool success = Enum.TryParse<ECountry>(value, out ECountry l_ECountry);
				m_Site.Country = success ? l_ECountry : ECountry.Undefined;
				_db.UpdateSite(m_Site);
			}
		}
		public string Town
		{
			get { return m_Site.Town; }
			set
			{
				m_Site.Town = value;
				_db.UpdateSite(m_Site);
			}
		}
		public string WindOrientation { get { return $"{WindOrientationBegin} - {WindOrientationEnd}"; } }
		public string WindOrientationBegin
		{
			get { return m_Site.WindOrientationBegin.ToString(); }
			set
			{
				bool success = Enum.TryParse<EWindOrientation>(value, out EWindOrientation l_EWindOrientation);
				m_Site.WindOrientationBegin = success ? l_EWindOrientation : EWindOrientation.Undefined;
				_db.UpdateSite(m_Site);
			}
		}
		public string WindOrientationEnd
		{
			get { return m_Site.WindOrientationEnd.ToString(); }
			set
			{
				bool success = Enum.TryParse<EWindOrientation>(value, out EWindOrientation l_EWindOrientation);
				m_Site.WindOrientationEnd = success ? l_EWindOrientation : EWindOrientation.Undefined;
				_db.UpdateSite(m_Site);
			}
		}
		public int SiteUseCount
		{
			get
			{
				return _db.GetSiteFlightCount(Site);
			}
		}

		public string[] AvailableWindOrientation { get => Enum.GetNames(typeof(EWindOrientation)); }
		public string[] AvailableCountry { get => Enum.GetNames(typeof(ECountry)); }

		public override string ToString()
		{
			return Name;
		}

	}
}

﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParagliderFlightLog.DataModels
{
    public class Site
    {
        private string m_site_ID = "";

        private string m_name = "";

        private string m_town = "";

        private ECountry m_country;

        private EWindOrientation m_windOrientation;

        private bool m_windOrientationSpecified;

        private double m_altitude;

        private double m_latitude;

        private double m_longitude;

        public string Site_ID { get => m_site_ID; set => m_site_ID = value; }
        public string Name { get => m_name; set => m_name = value; }
        public string Town { get => m_town; set => m_town = value; }
        public ECountry Country { get => m_country; set => m_country = value; }
        public EWindOrientation WindOrientation { get => m_windOrientation; set => m_windOrientation = value; }
        public bool WindOrientationSpecified { get => m_windOrientationSpecified; set => m_windOrientationSpecified = value; }
        public double Altitude { get => m_altitude; set => m_altitude = value; }
        public double Latitude { get => m_latitude; set => m_latitude = value; }
        public double Longitude { get => m_longitude; set => m_longitude = value; }
    }

    public enum ECountry
    {
        /// <remarks/>
        Switzerland,

        /// <remarks/>
        France,

        /// <remarks/>
        Italy,

        /// <remarks/>
        Austria,

        /// <remarks/>
        Spain,
    }
    public enum EWindOrientation
    {
        /// <remarks/>
        North,
        NorthEast,

        /// <remarks/>
        East,
        SouthEast,

        /// <remarks/>
        South,
        SouthWest,

        /// <remarks/>
        West,
        NorthWest,
    }
}

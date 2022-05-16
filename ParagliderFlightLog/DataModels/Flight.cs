using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParagliderFlightLog.DataModels
{
    public class Flight
    {
        private string m_igcFileContent="";

        private string m_comment="";

        private string m_flight_ID="";

        private string m_REF_TakeOffSite_ID="";

        private string m_REF_Glider_ID="";

        private System.DateTime m_takeOffDateTime;

        private TimeSpan m_flightDuration;

        private double m_takeOffAltitude;

        private double m_takeOffLatitude;

        private double m_takeOffLongitude;

        public string IgcFileContent { get => m_igcFileContent; set => m_igcFileContent = value; }
        public string Comment { get => m_comment; set => m_comment = value; }
        public string Flight_ID { get => m_flight_ID; set => m_flight_ID = value; }
        public string REF_TakeOffSite_ID { get => m_REF_TakeOffSite_ID; set => m_REF_TakeOffSite_ID = value; }
        public string REF_Glider_ID { get => m_REF_Glider_ID; set => m_REF_Glider_ID = value; }
        public DateTime TakeOffDateTime { get => m_takeOffDateTime; set => m_takeOffDateTime = value; }
        public TimeSpan FlightDuration { get => m_flightDuration; set => m_flightDuration = value; }
        public double TakeOffAltitude { get => m_takeOffAltitude; set => m_takeOffAltitude = value; }
        public double TakeOffLatitude { get => m_takeOffLatitude; set => m_takeOffLatitude = value; }
        public double TakeOffLongitude { get => m_takeOffLongitude; set => m_takeOffLongitude = value; }
    }
}

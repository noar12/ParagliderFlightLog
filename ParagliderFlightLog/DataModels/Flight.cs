using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Globalization;

namespace ParagliderFlightLog.DataModels
{
    public class Flight
    {
        private string m_igcFileContent="";

        private string m_comment="";

        private string m_flight_ID="";

        private string m_REF_TakeOffSite_ID="";

        private string m_REF_Glider_ID="";

        private TimeSpan m_flightDuration;

        private double m_takeOffAltitude;

        private double m_takeOffLatitude;

        private double m_takeOffLongitude;

        private DateTime m_takeOffDateTime;

        public string IgcFileContent { get => m_igcFileContent; set => m_igcFileContent = value; }
        public string Comment { get => m_comment; set => m_comment = value; }
        public string Flight_ID { get => m_flight_ID; set => m_flight_ID = value; }
        public string REF_TakeOffSite_ID { get => m_REF_TakeOffSite_ID; set => m_REF_TakeOffSite_ID = value; }
        public string REF_Glider_ID { get => m_REF_Glider_ID; set => m_REF_Glider_ID = value; }
        public DateTime TakeOffDateTime
        {
            get
            {
                const string FLIGHT_TIME_REGEXP = @"B(?<h>\d\d)(?<m>\d\d)(?<s>\d\d)";
                const string FLIGHT_DATE_REGEXP = @"HFDTE(DATE:)?(?<d>\d\d)(?<m>\d\d)(?<y>\d\d)";
                const int MILLENAR = 2000;

                string l_igcAllInOneLine = IgcFileContent.Replace("\r", "").Replace("\n", "");
                var matchTime = Regex.Match(l_igcAllInOneLine, FLIGHT_TIME_REGEXP);
                var matchDate = Regex.Match(l_igcAllInOneLine, FLIGHT_DATE_REGEXP);

                if (matchDate.Success && matchTime.Success)
                {
                    var l_FlightYear = int.Parse(matchDate.Groups["y"].Value) + MILLENAR;
                    var l_FlightMonth = int.Parse(matchDate.Groups["m"].Value);
                    var l_FlightDay = int.Parse(matchDate.Groups["d"].Value);

                    var l_FlightHour = int.Parse((matchTime.Groups["h"].Value));
                    var l_FlightMinute = int.Parse((matchTime.Groups["m"].Value));
                    var l_FlightSecond = int.Parse((matchTime.Groups["s"].Value));

                    return new DateTime(l_FlightYear, l_FlightMonth, l_FlightDay, l_FlightHour, l_FlightMinute, l_FlightSecond);
                }
                return m_takeOffDateTime;


            }
            set
            {
                m_takeOffDateTime = value;
            }
        }
        public TimeSpan FlightDuration { get => m_flightDuration; set => m_flightDuration = value; }
        public double TakeOffAltitude { get => m_takeOffAltitude; set => m_takeOffAltitude = value; }
        public double TakeOffLatitude { get => m_takeOffLatitude; set => m_takeOffLatitude = value; }
        public double TakeOffLongitude { get => m_takeOffLongitude; set => m_takeOffLongitude = value; }
        public List<FlightPoint> FlightPoints
        {
            get
            {
                List<FlightPoint> l_flightPoints = new List<FlightPoint>();
                FlightPoint l_flightPoint = new FlightPoint();
                foreach( string line in IgcFileContent.Split("\r\n"))
                {
                    if (ParseIGCFlightData(line, out l_flightPoint))
                    {
                        l_flightPoints.Add(l_flightPoint);
                    }
                }
             

                return l_flightPoints;
            }
        }
        /// <summary>
        /// Parse the IGC_Line as coordinate. Highly inspired from https://github.com/ringostarr80/RL.Geo/blob/master/RL.Geo/Gps/Serialization/IgcDeSerializer.cs
        /// return true if it succeed false otherwise
        /// </summary>
        /// <param name="IGC_Line"></param>
        /// <param name="parsedFlightPoint"></param>
        /// <returns></returns>
        private bool ParseIGCFlightData(string IGC_Line, out FlightPoint parsedFlightPoint)
        {
            
            const string COORDINATE_REGEX = @"^B(?<UTCTimeHour>\d\d)(?<UTCTimeMinute>\d\d)(?<UTCTimeSecond>\d\d)(?<d1>\d\d)(?<m1>\d\d\d\d\d)(?<dir1>[NnSs])(?<d2>\d\d\d)(?<m2>\d\d\d\d\d)(?<dir2>[EeWw])A(?<BaroAlt>\d\d\d\d\d)(?<GPS_Alt>\d\d\d\d\d)";
            var match = Regex.Match(IGC_Line, COORDINATE_REGEX);
            if (match.Success)
            {
                var deg1 = double.Parse(match.Groups["d1"].Value, CultureInfo.InvariantCulture) + double.Parse(match.Groups["m1"].Value, CultureInfo.InvariantCulture) / 1000 / 60;
                var dir1 = Regex.IsMatch(match.Groups["dir1"].Value, "[Ss]") ? -1d : 1d;
                var deg2 = double.Parse(match.Groups["d2"].Value, CultureInfo.InvariantCulture) + double.Parse(match.Groups["m2"].Value, CultureInfo.InvariantCulture) / 1000 / 60;
                var dir2 = Regex.IsMatch(match.Groups["dir2"].Value, "[Ww]") ? -1d : 1d;
                var height = double.Parse(match.Groups["GPS_Alt"].Value, CultureInfo.InvariantCulture);
                parsedFlightPoint = new FlightPoint() { Latitude = deg1 * dir1, Longitude = deg2 * dir2, Height = height };
                return true;
            }
            parsedFlightPoint = new FlightPoint() { Height = double.NaN, Latitude = double.NaN, Longitude = double.NaN };
            return false;

        }
    }
    public class FlightPoint
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public double Height { get; set; }
    }
}

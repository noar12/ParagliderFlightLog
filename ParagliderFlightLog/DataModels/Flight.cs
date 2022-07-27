using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Globalization;
using System.Runtime.CompilerServices;

namespace ParagliderFlightLog.DataModels
{
    public class Flight : INotifyPropertyChanged
    {
        // todo: put in each setter something like an PropertyChanged
        private string m_igcFileContent = "";

        private string m_comment = "";

        private string m_flight_ID = Guid.NewGuid().ToString();

        private string m_REF_TakeOffSite_ID = "";

        private string m_REF_Glider_ID = "";

        private TimeSpan m_flightDuration;

        private DateTime m_takeOffDateTime;
        private List<FlightPoint> m_FlightPoints =  new List<FlightPoint>();

        public string IgcFileContent { get => m_igcFileContent; set => m_igcFileContent = value; }
        public string Comment { get => m_comment; set => m_comment = value; }
        public string Flight_ID { get => m_flight_ID; set => m_flight_ID = value; }
        public string REF_TakeOffSite_ID { get => m_REF_TakeOffSite_ID; set => m_REF_TakeOffSite_ID = value; }
        public string REF_Glider_ID { get => m_REF_Glider_ID; set => m_REF_Glider_ID = value; }
        /// <summary>
        /// The take off time in UTC as a timestamp based on the igc data (date in meta data and time as the timestamp of the first sample)
        /// or on a backing field if no igc content is available
        /// </summary>
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
        /// <summary>
        /// The flight duration as a TimeSpan based on the number of sample in the IGC File (1 sample per seconds) 
        /// or on the content of a backing field if no igc content is available
        /// </summary>
        public TimeSpan FlightDuration
        {
            get => FlightPoints.Count > 0 ? new TimeSpan(0, 0, FlightPoints.Count) : m_flightDuration;

            set => m_flightDuration = value;
        }
        public int FlightDuration_s
        {
            get { return (int)FlightDuration.TotalSeconds; }
            set
            {
                m_flightDuration = new TimeSpan(0, 0, value);
            }
        }
        /// <summary>
        /// The altitude of the take off if an igc content is available. NaN otherwise
        /// </summary>
        public double TakeOffAltitude { get => FlightPoints.Count > 0 ? FlightPoints[0].Height : double.NaN; }

        /// <summary>
        /// The longitude of the take off if an igc content is available. NaN otherwise
        /// </summary>
        public FlightPoint TakeOffPoint { get => FlightPoints.Count > 0 ? FlightPoints[0] : new FlightPoint() { Latitude = double.NaN, Longitude = double.NaN, Height= double.NaN}; }
        public List<FlightPoint> FlightPoints
        {
            get
            {
                if (m_FlightPoints.Count == 0)
                {
                    FlightPoint l_flightPoint = new FlightPoint();
                    foreach (string line in IgcFileContent.Split("\r\n"))
                    {
                        if (ParseIGCFlightData(line, out l_flightPoint))
                        {
                            m_FlightPoints.Add(l_flightPoint);
                        }
                    }
                }

                return m_FlightPoints;
            }
        }
        public string IGC_GliderName
        {
            get
            {
                const string GLIDER_TYPE_REGEX = @"^HFGTYGLIDERTYPE:(?<value>.+)$";
                var match = Regex.Match(IgcFileContent, GLIDER_TYPE_REGEX, RegexOptions.Multiline);
                if (match.Success)
                {
                    return match.Groups["value"].Value.TrimEnd('\r','\n');
                }
                return "";
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        // This method is called by the Set accessor of each property.  
        // The CallerMemberName attribute that is applied to the optional propertyName  
        // parameter causes the property name of the caller to be substituted as an argument.  
        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
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
    /// <summary>
    /// Flight point represented in decimal degree
    /// </summary>
    public class FlightPoint
    {
        
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public double Height { get; set; }
        public double DistanceFrom(FlightPoint OtherFlightPoint)
        {
            const double EARTH_RADIUS = 6371000;
            
            return 2 * EARTH_RADIUS
                * Math.Asin(Math.Sqrt(Math.Pow(Math.Sin((this.Latitude / 180 * Math.PI - OtherFlightPoint.Latitude / 180 * Math.PI) / 2), 2)
                + Math.Cos(this.Latitude / 180 * Math.PI) * Math.Cos(OtherFlightPoint.Latitude / 180 * Math.PI)
                * Math.Pow(Math.Sin((this.Longitude / 180 * Math.PI - OtherFlightPoint.Longitude / 180 * Math.PI) / 2), 2)));
        }
    }
}

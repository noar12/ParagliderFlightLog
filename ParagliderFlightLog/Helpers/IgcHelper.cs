using ParagliderFlightLog.Models;
using System.Globalization;
using System.Text.RegularExpressions;

namespace ParagliderFlightLog.Helpers;
/// <summary>
/// Set of method to help with IGC file
/// </summary>
public static class IgcHelper
{
    /// <summary>
    /// Parse the <paramref name="igcLine"/> as coordinate. Highly inspired from https://github.com/ringostarr80/RL.Geo/blob/master/RL.Geo/Gps/Serialization/IgcDeSerializer.cs
    /// return true if it succeed false otherwise
    /// </summary>
    /// <param name="igcLine"></param>
    /// <param name="parsedFlightPoint"></param>
    /// <returns></returns>
    private static bool ParseIGCFlightData(string igcLine, out FlightPoint parsedFlightPoint)
    {
        const string COORDINATE_REGEX =
            @"^B(?<UTCTimeHour>\d\d)(?<UTCTimeMinute>\d\d)(?<UTCTimeSecond>\d\d)(?<d1>\d\d)(?<m1>\d\d\d\d\d)(?<dir1>[NnSs])(?<d2>\d\d\d)(?<m2>\d\d\d\d\d)(?<dir2>[EeWw])A(?<BaroAlt>\d\d\d\d\d)(?<GPS_Alt>\d\d\d\d\d)";
        var match = Regex.Match(igcLine, COORDINATE_REGEX);
        if (match.Success)
        {
            var deg1 = double.Parse(match.Groups["d1"].Value, CultureInfo.InvariantCulture) +
                       double.Parse(match.Groups["m1"].Value, CultureInfo.InvariantCulture) / 1000 / 60;
            var dir1 = Regex.IsMatch(match.Groups["dir1"].Value, "[Ss]") ? -1d : 1d;
            var deg2 = double.Parse(match.Groups["d2"].Value, CultureInfo.InvariantCulture) +
                       double.Parse(match.Groups["m2"].Value, CultureInfo.InvariantCulture) / 1000 / 60;
            var dir2 = Regex.IsMatch(match.Groups["dir2"].Value, "[Ww]") ? -1d : 1d;
            var height = double.Parse(match.Groups["GPS_Alt"].Value, CultureInfo.InvariantCulture);
            parsedFlightPoint =
                new FlightPoint() { Latitude = deg1 * dir1, Longitude = deg2 * dir2, Altitude = height };
            return true;
        }
        parsedFlightPoint = new FlightPoint() { Altitude = double.NaN, Latitude = double.NaN, Longitude = double.NaN };
        return false;
    }
    /// <summary>
    /// Get a list of FlightPoint from an IGC content
    /// </summary>
    /// <param name="igcContent"></param>
    /// <returns></returns>
    public static List<FlightPoint> GetFlightPointsFromIgcContent(string igcContent)
    {
        List<FlightPoint> output = [];
        foreach (string line in igcContent.Split("\r\n"))
        {
            if (ParseIGCFlightData(line, out FlightPoint flightPoint))
            {
                output.Add(flightPoint);
            }
        }

        return output;
    }
}
using System.Text.Json;

namespace ParagliderFlightLog.Models;
/// <summary>
/// Model to store all scoring information
/// </summary>
public class XcScore
{

    /// <summary>
    /// raw data about score
    /// </summary>
    public string GeoJsonText { get; private set; } = null!;
    /// <summary>
    /// Parsed info about score
    /// </summary>
    public XcScoreGeoJson GeoJsonObject { get; private set; } = null!;
    /// <summary>
    /// Build score info from <paramref name="geoJson"/>.
    /// </summary>
    /// <param name="geoJson"></param>
    /// <param name="withFlightCoordinates"></param>
    /// <returns></returns>
    public static XcScore? FromJson(string geoJson, bool withFlightCoordinates = false)
    {
        try
        {
            var result = JsonSerializer.Deserialize<XcScoreGeoJson>(geoJson);
            if (result == null) return null;
            if (!withFlightCoordinates)
            {
                result.features = result.features.Where(f => f.id != "flight").ToArray();
                geoJson = JsonSerializer.Serialize(result);
            }
            var output = new XcScore()
            {
                GeoJsonObject = result,
                GeoJsonText = geoJson
            };
            return output;
        }
        catch
        {
            return null;
        }
    }
    /// <summary>
    /// Get the number of points of the optimezed solution
    /// </summary>
    public double Points => GeoJsonObject.properties.score;

    /// <summary>
    /// Get the type of flight found as the optimized solution
    /// </summary>
    public string Type => GeoJsonObject.properties.type;
    /// <summary>
    /// Route length in km
    /// </summary>
    public double RouteLength => Points / RouteCoefficient.GetCoefficientByName(Type).Value;
    /// <summary>
    /// The duration of the route from the first turnpoint (ep.start or cp.in) to the last turnpoint (ep.finish or cp.out)
    /// </summary>
    public TimeSpan? RouteDuration
    {
        get
        {
            string[] startEndRoutePointName = Type switch
            {
                "Free Flight" => ["ep_start", "ep_finish"],
                "Free Triangle" or "FAI Triangle" or "Closed Free Triangle" or "Closed FAI Triangle" => ["cp_in", "cp_out"],
                _ => [],
            };
            if (startEndRoutePointName.Length != 2)
            {
                return TimeSpan.Zero;
            }
            long? startTimestamp_ms = GeoJsonObject.features.SingleOrDefault(f => f.id == startEndRoutePointName[0])?.properties.timestamp;
            long? endTimestamp_ms = GeoJsonObject.features.SingleOrDefault(f => f.id == startEndRoutePointName[1])?.properties.timestamp;
            if (startTimestamp_ms is null || endTimestamp_ms is null)
            {
                return null;
            }
            return TimeSpan.FromMilliseconds((long)(endTimestamp_ms - startTimestamp_ms));
        }
    }
    /// <summary>
    /// Gets the average speed of the route in kilometers per hour (between first and last turnpoint).
    /// </summary>
    public double AverageSpeedKmh
    {
        get
        {
            double? durationHours = RouteDuration?.TotalHours;
            if (durationHours is not null && durationHours > 0)
            {
                return RouteLength / durationHours.Value;
            }
            return 0.0;
        }
    }

    /// <summary>
    /// represent the score as its json
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        return GeoJsonText;
    }
    /// <summary>
    /// This class represent the coefficient used to calculate the score. It assumed that the coefficient are according the world rules of XContest
    /// </summary>
    /// <param name="Name"></param>
    /// <param name="Value"></param>
    private record RouteCoefficient(string Name, double Value)
    {
        public static RouteCoefficient FreeFlight => new("Free Flight", 1.0);
        public static RouteCoefficient FreeTriangle => new("Free Triangle", 1.2);
        public static RouteCoefficient FaiTriangle => new("FAI Triangle", 1.4);
        public static RouteCoefficient ClosedFreeTriangle => new("Closed Free Triangle", 1.4);
        public static RouteCoefficient ClosedFaiTriangle => new("Closed FAI Triangle", 1.6);
        public static RouteCoefficient Undefined => new("Undefined", 0.0);

        public static RouteCoefficient GetCoefficientByName(string name)
        {
            return name switch
            {
                "Free Flight" => FreeFlight,
                "Free Triangle" => FreeTriangle,
                "FAI Triangle" => FaiTriangle,
                "Closed Free Triangle" => ClosedFreeTriangle,
                "Closed FAI Triangle" => ClosedFaiTriangle,
                _ => Undefined,
            };
        }
    }
}

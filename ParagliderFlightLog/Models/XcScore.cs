using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

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
    public double Points
    {
        get
        {
            return GeoJsonObject.properties.score;
        }
    }
    /// <summary>
    /// Get the type of flight found as the optimized solution
    /// </summary>
    public string Type
    {
        get
        {
            return GeoJsonObject.properties.type;
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
}

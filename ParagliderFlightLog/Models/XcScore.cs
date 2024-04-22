using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace ParagliderFlightLog.Models
{
    public class XcScore
    {
        public string GeoJsonText { get; private set; } = null!;
        public XcScoreGeoJson GeoJsonObject { get; private set; } = null!;

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
        public double Points
        {
            get
            {
                return GeoJsonObject.properties.score;
            }
        }
        public string Type
        {
            get
            {
                return GeoJsonObject.properties.type;
            }
        }
        public override string ToString()
        {
            return GeoJsonText;
        }
    }
}

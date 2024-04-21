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
        public string GeoJsonScore { get; private set; } = null!;
        private XcScoreGeoJson xcScore = null!;

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
                    xcScore = result,
                    GeoJsonScore = geoJson
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
                return xcScore.properties.score;
            }
        }
        public override string ToString()
        {
            return GeoJsonScore;
        }
    }
}

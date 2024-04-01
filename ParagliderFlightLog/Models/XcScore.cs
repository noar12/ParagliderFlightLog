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
        public string GeoJsonScore { get; private set; }
        public XcScore(string geoJson)
        {
            GeoJsonScore = geoJson;
        }
        public double? Points { get {
                var result = JsonSerializer.Deserialize<XcScoreGeoJson>(GeoJsonScore);
                return result?.properties.score;
        } }
        public override string ToString()
        {
            return GeoJsonScore;
        }
    }
}

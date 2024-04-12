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

		public static XcScore? FromJson(string geoJson)
		{
			try
			{
				var result = JsonSerializer.Deserialize<XcScoreGeoJson>(geoJson);
				if (result == null) return null;
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

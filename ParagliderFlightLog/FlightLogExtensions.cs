using ParagliderFlightLog.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParagliderFlightLog
{
	public static class FlightLogExtensions
	{
		/// <summary>
		/// Compute the distance in meter from another flight point
		/// </summary>
		/// <param name="otherFlightPoint"></param>
		/// <returns></returns>
		public static double DistanceFrom(this FlightPoint flightPoint, FlightPoint otherFlightPoint)
		{
			const double EARTH_RADIUS = 6371000;

			return 2 * EARTH_RADIUS
				* Math.Asin(Math.Sqrt(Math.Pow(Math.Sin((flightPoint.Latitude / 180 * Math.PI - otherFlightPoint.Latitude / 180 * Math.PI) / 2), 2)
				+ Math.Cos(flightPoint.Latitude / 180 * Math.PI) * Math.Cos(otherFlightPoint.Latitude / 180 * Math.PI)
				* Math.Pow(Math.Sin((flightPoint.Longitude / 180 * Math.PI - otherFlightPoint.Longitude / 180 * Math.PI) / 2), 2)));
		}
	}
}

namespace ParagliderFlightLog.Models
{
	/// <summary>
	/// Flight point represented in decimal degree
	/// </summary>
	public class FlightPoint
	{
		/// <summary>
		/// Latitude in decimal
		/// </summary>
		public double Latitude { get; set; }
		/// <summary>
		/// Longitude in decimal
		/// </summary>
		public double Longitude { get; set; }
		/// <summary>
		/// Altitude (AMSL) in [m]
		/// </summary>
		public double Altitude { get; set; }
	}
}

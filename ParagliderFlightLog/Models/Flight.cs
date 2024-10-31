namespace ParagliderFlightLog.Models
{
	public class Flight
	{
		public string Comment { get; set; } = "";
		public string Flight_ID { get; set; } = Guid.NewGuid().ToString();
		public string REF_TakeOffSite_ID { get; set; } = "";
		public string REF_Glider_ID { get; set; } = "";

		public DateTime TakeOffDateTime { get; set; } = DateTime.MinValue;
		/// <summary>
		/// The flight duration as a TimeSpan based on the number of sample in the IGC File (1 sample per seconds) 
		/// or on the content of a backing field if no igc content is available
		/// </summary>
		public TimeSpan FlightDuration => TimeSpan.FromSeconds(FlightDuration_s);

		/// <summary>
		/// The altitude of the take off if an igc content is available. NaN otherwise
		/// </summary>
		public int FlightDuration_s { get; set; }
        public XcScore? XcScore { get; set; }
    }
}

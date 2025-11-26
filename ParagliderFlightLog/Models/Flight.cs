namespace ParagliderFlightLog.Models
{
    /// <summary>
    /// Light representation of a flight. Contains only the information needed for listing flights. If you need the full flight information, use the FlightWithData class.
    /// </summary>
	public class Flight
	{
        /// <summary>
        /// Comment associated to the flight
        /// </summary>
		public string Comment { get; set; } = "";
        /// <summary>
        /// ID of the flight
        /// </summary>
		public string Flight_ID { get; set; } = Guid.NewGuid().ToString();
        /// <summary>
        /// ID of the take off site used for the flight
        /// </summary>
		public string REF_TakeOffSite_ID { get; set; } = "";
        /// <summary>
        /// ID of the glider used for the flight
        /// </summary>
		public string REF_Glider_ID { get; set; } = "";
        /// <summary>
        /// Moment of the take off
        /// </summary>
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
        /// <summary>
        /// XC Score associated to the flight, if any
        /// </summary>
        public XcScore? XcScore { get; set; }
        /// <summary>
        /// Objective of the flight
        /// </summary>
        public EFlightObjective Objective { get; set; } = EFlightObjective.Undefined;
    }
}

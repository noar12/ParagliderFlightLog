namespace ParaglidingFlightLogWeb.ViewModels
{
    public class UserSharedDataViewModel
    {
        public string LinkToOriginalFlight => $"/FlightsList/{SourceFlightId}";
        /// <summary>
        /// Id of the shared flight in the user db
        /// </summary>
        public string SourceFlightId { get; set; } = "";
        /// <summary>
        /// Id of the shared flight in the shared db
        /// </summary>
        public string SharedFlightId { get; set; } = "";
        /// <summary>
        /// Counter of the view of the shared flight. Each view no matter if it's the same user is counted
        /// </summary>
        public int ViewCounter { get; set; }
        /// <summary>
        /// Date of the last time the flight has been viewed
        /// </summary>
        public DateTime LastViewDateTime { get; set; }

        public string LinkToSharedFlight => $"/SharedFlight/{SharedFlightId}";
    }
}
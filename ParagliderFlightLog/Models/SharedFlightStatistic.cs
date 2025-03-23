namespace ParagliderFlightLog.Models
{
    public class SharedFlightStatistic
    {
        public string Id { get; set; } = "";
        public string REF_SharedFlight_ID { get; set; } = "";
        public string REF_SourceFlight_ID { get; set; } = "";
        public string REF_User_Id { get; set; } = "";
        public int ViewCount { get; set; }
        public DateTime LastViewDateTime { get; set; }
    }
}
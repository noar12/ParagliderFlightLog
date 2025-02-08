namespace ParagliderFlightLog.Models
{
    /// <summary>
    /// Db Informations model
    /// </summary>
    public class DbInformations
    {
        /// <summary>
        /// Major version of the database
        /// </summary>
        public int VersionMajor { get; set; }
        /// <summary>
        /// Minor version of the database
        /// </summary>
        public int VersionMinor { get; set; }
        /// <summary>
        /// Fix version of the database
        /// </summary>
        public int VersionFix { get; set; }
        /// <summary>
        /// User Id  whom own the database
        /// </summary>
        public string? UserId { get; set; }
    }
}

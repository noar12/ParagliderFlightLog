using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParagliderFlightLog.Models
{
    public class DbInformations
    {
        public int VersionMajor { get; set; }
        public int VersionMinor { get; set; }
        public int VersionFix { get; set; }
        public string? UserId { get; set; }
    }
}

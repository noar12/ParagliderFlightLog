using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ParagliderFlightLog.Models
{
	public class Glider
    {
		public string Glider_ID { get; set; } = Guid.NewGuid().ToString();
		public string Manufacturer { get; set; } = "Unknown manufacturer";
		public string Model { get; set; } = "Unkown model";
		public string IGC_Name { get; set; } = "UnknownGlider";
		public int BuildYear { get; set; }
		public DateTime LastCheckDateTime { get; set; }
		public bool LastCheckDateTimeSpecified { get; set; }
		public EHomologationCategory HomologationCategory { get; set; }
		public string FullName { get { return $"{Manufacturer} {Model}"; } }
    }
}



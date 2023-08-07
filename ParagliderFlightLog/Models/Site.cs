using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ParagliderFlightLog.Models
{
	public class Site
	{
		public double SiteRadius { get; } = 280; // on devrait pouvoir réduire mais certain point de site de la db sont très mal placé (ex: le CERNIL)
		public string Site_ID { get; set; } = Guid.NewGuid().ToString();
		public string Name { get; set; } = "Unknown site";
		public string Town { get; set; } = "";
		public ECountry Country { get; set; }
		public EWindOrientation WindOrientationBegin { get; set; }
		public EWindOrientation WindOrientationEnd { get; set; }
		public string WindOrientationText { get => $"{WindOrientationBegin} - {WindOrientationEnd}"; } // todo: move to the view model
		public bool WindOrientationSpecified { get; set; }
		public double Altitude { get; set; }
		public double Latitude { get; set; }
		public double Longitude { get; set; }
	}
}

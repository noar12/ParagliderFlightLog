using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfUI.ViewModels
{
    public class FlightViewModel
    {
        public DateTime TakeOffDateTime { get; set; }
        public TimeSpan FlightDuration { get; set; }
        public string TakeOffSiteName { get; set; }
        public string Comment { get; set; }
    }
}

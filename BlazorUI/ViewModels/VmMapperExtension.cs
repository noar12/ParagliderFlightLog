using ParagliderFlightLog.DataAccess;
using ParagliderFlightLog.Models;
using ParagliderFlightLog.ViewModels;
using System.Runtime.CompilerServices;

namespace BlazorUI.ViewModels
{
    public static class VmMapperExtension
    {
        public static FlightViewModel ToVM(this Flight flight, FlightLogDB db)
        {
            return new FlightViewModel(flight, db);
        }
        public static SiteViewModel ToVM(this Site site, FlightLogDB db)
        {
            return new SiteViewModel(site, db);
        }
        public static GliderViewModel ToVM(this Glider glider, FlightLogDB db)
        {
            return new GliderViewModel(glider, db);
        }
    }
}

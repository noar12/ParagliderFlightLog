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
    }
}

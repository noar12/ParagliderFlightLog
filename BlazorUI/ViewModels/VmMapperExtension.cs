using ParagliderFlightLog.Models;
using ParagliderFlightLog.ViewModels;
using System.Runtime.CompilerServices;

namespace BlazorUI.ViewModels
{
    public static class VmMapperExtension
    {
        public static FlightViewModel ToVM(this Flight flight)
        {
            return new FlightViewModel(flight);
        }
    }
}

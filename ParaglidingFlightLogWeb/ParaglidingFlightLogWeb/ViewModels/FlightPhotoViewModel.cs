using ParagliderFlightLog.Models;
using ParagliderFlightLog.Services;

namespace ParaglidingFlightLogWeb.ViewModels
{
    public class FlightPhotoViewModel(FlightPhoto flightPhoto)
    {
        public string GetBase64PhotoData(PhotosService service)
        {
            byte[] bytes = File.ReadAllBytes(service.GetFlightPhotoPath(flightPhoto));
            return Convert.ToBase64String(bytes);
        }
    }
}
using ParagliderFlightLog.Models;
using ParagliderFlightLog.Services;

namespace ParaglidingFlightLogWeb.ViewModels
{
    public class FlightPhotoViewModel(FlightPhoto flightPhoto)
    {
        /// <summary>
        /// Get the base64 encoded photo data
        /// </summary>
        /// <param name="service"></param>
        /// <returns></returns>
        public string GetBase64PhotoData(PhotosService service)
        {
            string? photoPath = service.GetFlightPhotoPath(flightPhoto);
            if (photoPath == null || !File.Exists(photoPath))
            {
                return string.Empty;
            }
            byte[] bytes = File.ReadAllBytes(photoPath);
            return Convert.ToBase64String(bytes);
        }
    }
}
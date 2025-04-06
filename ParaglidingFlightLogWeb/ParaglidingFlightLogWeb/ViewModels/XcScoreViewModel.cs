using ParagliderFlightLog.Models;

namespace ParaglidingFlightLogWeb.ViewModels
{
    /// <summary>
    /// View Model to display the XC score
    /// </summary>
    public class XcScoreViewModel
    {
        private readonly XcScore _score;
        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="score"></param>
        public XcScoreViewModel(XcScore score)
        {
            _score = score;
        }
        /// <summary>
        /// Points of the optimized solution
        /// </summary>
        public double Points => _score.Points;
        /// <summary>
        /// Type of flight found as the optimized solution
        /// </summary>
        public string Type => _score.Type;
        /// <summary>
        /// The raw data about the score. To be used to display the result on a map since it is a GeoJson
        /// </summary>
        public XcScoreGeoJson GeoJsonObject => _score.GeoJsonObject;
        /// <summary>
        /// Length of the route in km
        /// </summary>
        public double RouteLength => _score.RouteLength;
    }
}

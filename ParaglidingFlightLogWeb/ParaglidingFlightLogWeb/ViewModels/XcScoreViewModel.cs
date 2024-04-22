using ParagliderFlightLog.Models;

namespace ParaglidingFlightLogWeb.ViewModels
{
    public class XcScoreViewModel
    {
        private readonly XcScore _score;

        public XcScoreViewModel(XcScore score)
        {
            _score = score;
        }
        public double Points { get => _score.Points; }
        public string Type { get => _score.Type; }
    }
}

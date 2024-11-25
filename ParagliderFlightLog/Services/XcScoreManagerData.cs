using ParagliderFlightLog.DataAccess;
using ParagliderFlightLog.Models;

namespace ParagliderFlightLog.Services
{
    public class XcScoreManagerData
    {
        private readonly Queue<ProcessRequest> _processRequests = new();
        /// <summary>
        /// Number of flight in the queue still to process
        /// </summary>
        public int FlightToProcess => _processRequests.Count;
        
        public void QueueFlightForScoring(FlightWithData flight, FlightLogDB db)
        {
            if (string.IsNullOrEmpty(flight.IgcFileContent)) return;
            var request = new ProcessRequest()
            {
                Flight = flight,
                Db = db
            };
            _processRequests.Enqueue(request);
        }

        public ProcessRequest? GetNextProcessRequest()
        {
            return _processRequests.TryDequeue(out var request) ? request : null;
        }
        /// <summary>
        /// Indicates if a score engine is installed
        /// </summary>
        public bool ScoreEngineInstalled { get; set; } = true;
        public sealed class ProcessRequest
        {
            public FlightWithData Flight { get; set; } = null!;
            public FlightLogDB Db { get; set; } = null!;
        }
    }
}
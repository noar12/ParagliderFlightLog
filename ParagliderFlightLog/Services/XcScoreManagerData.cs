using ParagliderFlightLog.DataAccess;
using ParagliderFlightLog.Models;
using System.Reactive.Linq;
using System.Reactive.Subjects;
namespace ParagliderFlightLog.Services
{
    /// <summary>
    /// Class to share data from the XcScoreManager
    /// </summary>
    public class XcScoreManagerData
    {
        /// <summary>
        /// Ctor
        /// </summary>
        public XcScoreManagerData()
        {
            _flightToProcessSubject = new(0);
            FlightToProcess = _flightToProcessSubject.AsObservable();
        }
        private readonly Queue<ProcessRequest> _processRequests = new();

        /// <summary>
        /// Number of flight in the queue still to process. Triggered when it changed
        /// </summary>
        public  IObservable<int> FlightToProcess { get; }

        private readonly BehaviorSubject<int> _flightToProcessSubject;
        /// <summary>
        /// Add the flight to the queue for xcscore calculation
        /// </summary>
        /// <param name="flight"></param>
        /// <param name="db"></param>
        public void QueueFlightForScoring(FlightWithData flight, FlightLogDB db)
        {
            if (string.IsNullOrEmpty(flight.IgcFileContent)) return;
            var request = new ProcessRequest()
            {
                Flight = flight,
                Db = db
            };
            _processRequests.Enqueue(request);
            _flightToProcessSubject.OnNext(_processRequests.Count);
        }
        /// <summary>
        /// Return the next flight to process
        /// </summary>
        /// <returns></returns>
        public ProcessRequest? GetNextProcessRequest()
        {
            _processRequests.TryDequeue(out var request);
            _flightToProcessSubject.OnNext(_processRequests.Count);
            return request;
        }
        
        /// <summary>
        /// Indicates if a score engine is installed
        /// </summary>
        public bool ScoreEngineInstalled { get; set; } = true;
        /// <summary>
        /// A type that represent a flight to process
        /// </summary>
        public sealed class ProcessRequest
        {
            /// <summary>
            /// The flight to process
            /// </summary>
            public FlightWithData Flight { get; init; } = null!;
            /// <summary>
            /// The data access relevant for the flight
            /// </summary>
            public FlightLogDB Db { get; init; } = null!;
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ParaglidingFlightLogWeb.ViewModels;
using ParagliderFlightLog.DataAccess;
using ParagliderFlightLog.Models;
using ParagliderFlightLog;

namespace ParaglidingFlightLogWeb.ViewModels
{
    public class FlightViewModel
    {
        private const int INTEGRATION_STEP = 8;
        private readonly Flight _flight;
        private FlightWithData? _flightWithData = null;
        private readonly FlightLogDB _db;

        public FlightViewModel(Flight flight, FlightLogDB db)
        {
            _flight = flight;
            _db = db;
        }

        public string FlightID { get { return _flight.Flight_ID; } }
        public Flight Flight
        {
            get { return _flight; }
        }
        public DateTime TakeOffDateTime { get { return _flight.TakeOffDateTime; } }
        public TimeSpan FlightDuration { get { return _flight.FlightDuration; } }
        public string TakeOffSiteID { get { return _flight.REF_TakeOffSite_ID; } }
        public XcScoreViewModel? XcScore { get => _flight.XcScore != null ? new XcScoreViewModel(_flight.XcScore) : null; }
        public SiteViewModel? TakeOffSite
        {
            get { return _db.GetFlightTakeOffSite(Flight)?.ToVM(_db); }
            set
            {
                if (value != null)
                {
                    Flight.REF_TakeOffSite_ID = value.Site_ID;
                    _db.UpdateFlightTakeOffSite(Flight, value.Site);
                }
            }
        }
        public string TakeOffSiteName
        {
            get
            {
                return _db.GetFlightTakeOffSite(Flight)?.Name ?? "Site not found";
            }

        }
        public string GliderName
        {
            get
            {
                return _db.GetFlightGlider(Flight)?.FullName ?? "Glider not found";
            }
        }
        public GliderViewModel? Glider
        {
            get { return _db.GetFlightGlider(Flight)?.ToVM(_db); }
            set
            {
                if (value != null)
                {
                    Flight.REF_Glider_ID = value.GliderId;
                    _db.UpdateFlightGlider(Flight, value.Glider);
                }
            }
        }
        public double MaxHeight
        {
            get
            {
                _flightWithData ??= _db.GetFlightWithData(_flight);
                if (_flightWithData is not null && FlightPoints.Count > 0)
                {
                    return _flightWithData.FlightPoints.Select(fp => fp.Height).Max();
                }
                else
                {
                    return 0.0;
                }
            }
        }
        public List<FlightPoint> FlightPoints
        {
            get
            {
                _flightWithData ??= _db.GetFlightWithData(_flight);

                return _flightWithData?.FlightPoints ?? [];
            }
        }
        public double[][] GetLatLngsArray()
        {
            double[][] output = new double[FlightPoints.Count][];
            for (int i = 0; i < output.Length; i++)
            {
                var currentFlightPoint = FlightPoints[i];
                output[i] = [currentFlightPoint.Latitude, currentFlightPoint.Longitude];
            }
            return output;
        }
        public string Comment { get { return _db.GetFlightComment(Flight) ?? ""; } set { _db.UpdateFlightComment(Flight, value); } }
        /// <summary>
        /// Get the trace length in km
        /// </summary>
        public double TraceLength
        {
            get
            {
                double l_TraceLegnth = 0.0;
                for (int i = 1; i < FlightPoints.Count; i++)
                {
                    l_TraceLegnth += FlightPoints[i].DistanceFrom(FlightPoints[i - 1]) / 1000;
                }
                return l_TraceLegnth;
            }
        }
        /// <summary>
        /// Maximum climb on the flight integrated over 8 secondes.
        /// </summary>
        public double MaxClimb
        {
            get
            {
                if (FlightPoints.Count > 0)
                {
                    return GetVerticalRate(INTEGRATION_STEP).Max();
                }
                else
                {
                    return 0.0;
                }
            }
        }
        /// <summary>
        /// Maximum sink on the flight integrated over 8 secondes.
        /// </summary>
        public double MaxSink
        {
            get
            {
                if (FlightPoints.Count > 0)
                    return GetVerticalRate(INTEGRATION_STEP).Min();
                else
                    return 0.0;
            }
        }


        private double[] GetVerticalRate(int integrationStep)
        {
            _flightWithData ??= _db.GetFlightWithData(_flight);
            if (_flightWithData is not null && FlightPoints.Count != 0)
            {
                var l_verticalRates = new List<double>();
                for (int i = integrationStep; i < _flightWithData.FlightPoints.Count; i++)
                {
                    l_verticalRates.Add((FlightPoints[i].Height - FlightPoints[i - INTEGRATION_STEP].Height) / INTEGRATION_STEP);
                }
                return [.. l_verticalRates];
            }
            return [];
        }

    }


}

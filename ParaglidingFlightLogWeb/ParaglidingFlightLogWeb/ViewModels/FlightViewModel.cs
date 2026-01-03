using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ParaglidingFlightLogWeb.ViewModels;
using ParagliderFlightLog.DataAccess;
using ParagliderFlightLog.Models;
using ParagliderFlightLog;

namespace ParaglidingFlightLogWeb.ViewModels;

/// <summary>
/// Flight model ready to be used by the views
/// </summary>
public class FlightViewModel
{
    private const int INTEGRATION_STEP = 8;
    private readonly Flight _flight;
    private FlightWithData? _flightWithData = null;
    private readonly FlightLogDB _db;

    /// <summary>
    /// ctor
    /// </summary>
    /// <param name="flight"></param>
    /// <param name="db"></param>
    public FlightViewModel(Flight flight, FlightLogDB db)
    {
        _flight = flight;
        _db = db;
    }

    /// <summary>
    /// Flight Id
    /// </summary>
    public string FlightID => _flight.Flight_ID;

    /// <summary>
    /// Underlying flight of the model
    /// </summary>
    public Flight Flight => _flight;

    /// <summary>
    /// Take off time
    /// </summary>
    public DateTime TakeOffDateTime
    {
        get => _flight.TakeOffDateTime;
        set => _flight.TakeOffDateTime = value;
    }

    /// <summary>
    /// Flight duration
    /// </summary>
    public TimeSpan FlightDuration
    {
        get { return _flight.FlightDuration; }
        set => _flight.FlightDuration_s = (int)value.TotalSeconds;
    }

    /// <summary>
    /// Take off Id
    /// </summary>
    public string TakeOffSiteID => _flight.REF_TakeOffSite_ID;

    /// <summary>
    /// Score of the flight
    /// </summary>
    public XcScoreViewModel? XcScore { get => _flight.XcScore != null ? new XcScoreViewModel(_flight.XcScore) : null; }

    /// <summary>
    /// Take off
    /// </summary>
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

    /// <summary>
    /// Objective of the flight
    /// </summary>
    public string Objective
    {
        get { return _db.GetFlightObjective(Flight).ToString(); }
        set
        {
            if (Enum.TryParse<EFlightObjective>(value, out var enumValue))
            {
                Flight.Objective = enumValue;
                _db.UpdateFlightObjective(Flight, enumValue);
            }
            else
            {
                Flight.Objective = EFlightObjective.Undefined;
            }
        }
    }

    /// <summary>
    /// Take off name
    /// </summary>
    public string TakeOffSiteName
    {
        get
        {
            return _db.GetFlightTakeOffSite(Flight)?.Name ?? "Site not found";
        }
    }

    /// <summary>
    /// Glider name
    /// </summary>
    public string GliderName
    {
        get
        {
            return _db.GetFlightGlider(Flight)?.FullName ?? "Glider not found";
        }
    }

    /// <summary>
    /// Glider
    /// </summary>
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

    /// <summary>
    /// Maximum altitude reached during the flight
    /// </summary>
    public double MaxAltitude
    {
        get
        {
            _flightWithData ??= _db.GetFlightWithData(_flight);
            if (_flightWithData is not null && FlightPoints.Count > 0)
            {
                return _flightWithData.FlightPoints.Select(fp => fp.Altitude).Max();
            }
            else
            {
                return 0.0;
            }
        }
    }

    /// <summary>
    /// Flight point from the IGC file
    /// </summary>
    public List<FlightPoint> FlightPoints
    {
        get
        {
            _flightWithData ??= _db.GetFlightWithData(_flight);

            return _flightWithData?.FlightPoints ?? [];
        }
    }

    /// <summary>
    /// Comment (saved directly in the data base)
    /// </summary>
    public string Comment
    {
        get => _db.GetFlightComment(Flight) ?? "";
        set
        {
            _flight.Comment = value;
            _db.UpdateFlightComment(Flight, value);
        }
    }

    /// <summary>
    /// Get the trace length in km
    /// </summary>
    public double TraceLength => ViewModelHelpers.GetTraceLength(FlightPoints);

    /// <summary>
    /// Maximum climb on the flight integrated over 8 secondes.
    /// </summary>
    public double MaxClimb
    {
        get
        {
            if (FlightPoints.Count > 0)
            {
                return ViewModelHelpers.GetVerticalRate(FlightPoints, INTEGRATION_STEP).Max();
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
                return ViewModelHelpers.GetVerticalRate(FlightPoints, INTEGRATION_STEP).Min();
            else
                return 0.0;
        }
    }

    /// <summary>
    /// Flight with all the data from the igc file
    /// </summary>
    public FlightWithData FlightWithData => _flightWithData ??= _db.GetFlightWithData(_flight);

    /// <summary>
    /// Return the photo of the flights for the UI
    /// </summary>
    public List<FlightPhotoViewModel> GetFlightPhotos() =>
        FlightWithData.FlightPhotos.Select(x => new FlightPhotoViewModel(x)).ToList();

    /// <summary>
    /// Update the flight in the database
    /// </summary>
    public async Task UpdateOrCreateFlightInDb()
    {
        if (await _db.FlightExistsAsync(_flight))
        {
            await _db.UpdateFlightAsync(_flight);
        }
        else
        {
            await _db.AddFlightAsync(_flight);
        }
    }
}
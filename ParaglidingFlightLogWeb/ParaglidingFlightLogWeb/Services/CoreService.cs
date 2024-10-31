using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ParaglidingFlightLogWeb.ViewModels;
using ParagliderFlightLog.DataAccess;
using ParagliderFlightLog.Models;
using Microsoft.Identity.Client;
using ParagliderFlightLog.Services;

namespace ParaglidingFlightLogWeb.Services;

/// <summary>
/// Provide Core functionnality of the app
/// </summary>
public class CoreService
{
    private readonly FlightLogDB _flightLog;
    private readonly ILogger<CoreService> _logger;
    private readonly LogFlyDB _logFlyDB;
    private readonly XcScoreManager _xcScoreManager;
    /// <summary>
    /// ctor
    /// </summary>
    /// <param name="flightLogDB"></param>
    /// <param name="logger"></param>
    /// <param name="logFlyDB"></param>
    /// <param name="xcScoreManager"></param>
    public CoreService(FlightLogDB flightLogDB, ILogger<CoreService> logger, LogFlyDB logFlyDB, XcScoreManager xcScoreManager)
    {
        _flightLog = flightLogDB;
        _logger = logger;
        _logFlyDB = logFlyDB;
        _xcScoreManager = xcScoreManager;
    }
    /// <summary>
    /// Init the page for <paramref name="userId"/>
    /// </summary>
    /// <param name="userId"></param>
    /// <returns></returns>
    public async Task Init(string userId)
    {
        _flightLog.Init(userId);
        FlightListViewModel = (await _flightLog.GetAllFlights()).Select(f => f.ToVM(_flightLog)).ToList();
        SiteListViewModel = (await _flightLog.GetAllSites()).Select(s => s.ToVM(_flightLog)).ToList();
        GliderListViewModel = (await _flightLog.GetAllGliders()).Select(g => g.ToVM(_flightLog)).ToList();
    }

    /// <summary>
    /// Call the db to remove the <paramref name="flightViewModel"/> from the db
    /// </summary>
    /// <param name="flightViewModel"></param>
    public void RemoveFlight(FlightViewModel flightViewModel)
    {
        _flightLog.DeleteFlight(flightViewModel.Flight);
        FlightListViewModel.Remove(flightViewModel);
    }
    /// <summary>
    /// Call the db to edit the <paramref name="glider"/> infos
    /// </summary>
    /// <param name="glider"></param>
    public void EditGlider(GliderViewModel glider)
    {
        if (glider != null)
        {
            _flightLog.UpdateGlider(glider.Glider);
        }
    }

    /// <summary>
    /// Not implemented. Will call the db to add a glider
    /// </summary>
    /// <exception cref="NotImplementedException"></exception>
    public void AddGlider()
    {

        throw new NotImplementedException();
    }

    /// <summary>
    /// Import an IGC file in the data model and use the result to instanciate and add a new FlightViewModel in the FlightListViewModel to update the UI and do the same with the takeoff site if it doesn't exist yet.
    /// </summary>
    /// <param name="filePaths"></param>
    public void AddFlightsFromIGC(string[] filePaths)
    {
        foreach (string filePath in filePaths)
        {
            _logger.LogDebug("importing {filePath}", filePath);
            var flight = _flightLog.ImportFlightFromIGC(filePath);
            var fvm = new FlightViewModel(flight.ToFlight(), _flightLog);
            FlightListViewModel.Add(fvm);
            EnqueueFlightForScore(fvm);
        }
    }
    /// <summary>
    /// List of all the year where a flight has been made
    /// </summary>
    public List<int> YearsOfFlying
    {
        get
        {
            List<int> l_yearsOfFlying = FlightListViewModel.Select(f => f.TakeOffDateTime.Year).Distinct().ToList();
            l_yearsOfFlying.Sort();
            return l_yearsOfFlying;

        }
    }

    /// <summary>
    /// Return a TimeSpan representing the cumulative flight duration in the period between start and end
    /// </summary>
    /// <param name="start"></param>
    /// <param name="end"></param>
    /// <returns></returns>
    public TimeSpan FlightDurationInPeriod(DateTime start, DateTime end)
    {
        // Linq Sum does not have an overload for IEnumerable<TimeSpan>...
        return new TimeSpan(FlightListViewModel
        .Where(x => x.TakeOffDateTime >= start && x.TakeOffDateTime <= end)
        .Sum(x => x.FlightDuration.Ticks));
    }
    /// <summary>
    /// Return a List of all the flight in the period specified between "start" and "end"
    /// </summary>
    /// <param name="start"></param>
    /// <param name="end"></param>
    /// <returns></returns>
    public List<FlightViewModel> FlightsInPeriod(DateTime start, DateTime end)
    {
        return FlightListViewModel.Where(f => f.TakeOffDateTime > start && f.TakeOffDateTime < end).ToList();
    }

    internal async Task<(int importedSitesCount, int improtedGlidersCount, int importedFlightCount)> ImportLogFlyDb(string path)
    {
        await _flightLog.BackupDb();
        await Task.Run(() => _logFlyDB.LoadLogFlyDB(path));
        (int importedSitesCount, int improtedGlidersCount, int importedFlightCount) = await Task.Run(_logFlyDB.ImportInFlightLogDB);

        return (importedSitesCount, improtedGlidersCount, importedFlightCount);
    }

    internal List<SiteViewModel> SiteUsedInTimeRange(DateTime startDate, DateTime endDate)
    {
        List<SiteViewModel> output = [];
        var sites = _flightLog.GetSitesUsedInTimeRange(startDate, endDate);
        foreach (var item in sites)
        {
            var siteVm = new SiteViewModel(item, _flightLog);
            output.Add(siteVm);
        }
        return output;
    }
    /// <summary>
    /// Put the flight in the queue to calculate its score
    /// </summary>
    /// <param name="lastSelectedFlight"></param>
    public void EnqueueFlightForScore(FlightViewModel? lastSelectedFlight)
    {
        if (lastSelectedFlight?.FlightWithData is null) return;
        _xcScoreManager.QueueFlightForScoring(lastSelectedFlight.FlightWithData, _flightLog);
    }
    /// <summary>
    /// Get a flight to remember. 
    /// </summary>
    /// <returns></returns>
    public FlightViewModel? GetFlightToRemember()
    {
        double anniversaryFlightProbability = 0;
        var random = new Random();
        double kindOfMemory = random.NextDouble();
        var anniversaryFlight = FlightListViewModel.Where(f => f.TakeOffDateTime.Day == DateTime.Today.Day && f.TakeOffDateTime.Month == DateTime.Today.Month).ToArray();
        if (anniversaryFlight.Length > 0)
        {
            anniversaryFlightProbability = 0.9;
        }
        if (kindOfMemory < anniversaryFlightProbability)
        {
            return anniversaryFlight[random.Next(anniversaryFlight.Length)];
        }
        else
        {
            return GetFlightWithBigComment();
        }
    }

    private FlightViewModel? GetFlightWithBigComment()
    {
        var random = new Random();
        int bigCommentLimit = 200;
        var flightWithBigComment = FlightListViewModel.Where(f => f.Comment.Length > bigCommentLimit).ToArray();
        if (flightWithBigComment.Length > 0)
        {
            return flightWithBigComment[random.Next(flightWithBigComment.Length)];
        }
        else if (FlightListViewModel.Count > 0)
        {
            return FlightListViewModel[random.Next(FlightListViewModel.Count)];
        }
        else
        {
            return null;
        }
    }
    /// <summary>
    /// Get a list of up to <paramref name="maxSiteCount"/> sites from the one that have not been visited since <paramref name="olderThan"/>
    /// </summary>
    /// <param name="olderThan"></param>
    /// <param name="maxSiteCount"></param>
    /// <returns></returns>
    public List<SiteViewModel> GetRandomSitesToReturnTo(TimeSpan olderThan, int maxSiteCount = 3)
    {
        var lastTakeOffTime = DateTime.UtcNow - olderThan;
        var recentlyFlownSitesId = FlightListViewModel
            .Where(x => x.TakeOffDateTime > lastTakeOffTime && x.TakeOffSite is not null)
            .Select(x => x.TakeOffSite!.Site_ID);
        var output = SiteListViewModel.Where(x => !recentlyFlownSitesId.Any(id => x.Site_ID == id))
        .Distinct()
        .OrderBy(_ => Random.Shared.Next())
        .Take(maxSiteCount)
        .ToList();
        return output;
    }
    /// <summary>
    /// All the flights
    /// </summary>
    public List<FlightViewModel> FlightListViewModel { get; private set; } = [];
    /// <summary>
    /// All the sites
    /// </summary>
    public List<SiteViewModel> SiteListViewModel { get; private set; } = [];
    /// <summary>
    /// All the glider
    /// </summary>
    public List<GliderViewModel> GliderListViewModel { get; private set; } = [];
}

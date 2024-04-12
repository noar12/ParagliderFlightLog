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

namespace ParaglidingFlightLogWeb.ViewModels
{
	public class MainViewModel
	{
		private readonly FlightLogDB _flightLog;
		private readonly ILogger<MainViewModel> _logger;
		private readonly LogFlyDB _logFlyDB;

		public MainViewModel(FlightLogDB flightLogDB, ILogger<MainViewModel> logger, LogFlyDB logFlyDB)
		{
			_flightLog = flightLogDB;
			_logger = logger;
			_logFlyDB = logFlyDB;

		}

		public async Task Init(string userId){
			_flightLog.Init(userId);
			FlightListViewModel = (await _flightLog.GetAllFlights()).Select(f => f.ToVM(_flightLog)).ToList();
			SiteListViewModel = (await _flightLog.GetAllSites()).Select(s => s.ToVM(_flightLog)).ToList();
			GliderListViewModel = (await _flightLog.GetAllGliders()).Select(g => g.ToVM(_flightLog)).ToList();
		}



		public void UpdateFlight(FlightViewModel flight)
		{
			if (flight.Flight != null)
			{
				_flightLog.UpdateFlight(flight.Flight);
			}

		}

		public void EditSite(SiteViewModel site)
		{

			if (site.Site != null)
			{
				_flightLog.UpdateSite(site.Site);
			}


		}
		public void RemoveFlight(FlightViewModel flightViewModel)
		{
			_flightLog.DeleteFlight(flightViewModel.Flight);
			FlightListViewModel.Remove(flightViewModel);
		}

		public void EditGlider(GliderViewModel glider)
		{
			if (glider != null)
			{
				_flightLog.UpdateGlider(glider.Glider);
			}
		}

 

		public void AddGlider()
		{

			throw new NotImplementedException();
		}

		/// <summary>
		/// Import an IGC file in the data model and use the result to instanciate and add a new FlightViewModel in the FlightListViewModel to update the UI and do the same with the takeoff site if it doesn't exist yet.
		/// </summary>
		/// <param name="filePath"></param>
		public void AddFlightsFromIGC(string[] filePaths)
		{
			foreach (string filePath in filePaths)
			{
				_logger.LogDebug("importing {filePath}", filePath);
				var flight = _flightLog.ImportFlightFromIGC(filePath);
				FlightListViewModel.Add(new FlightViewModel(flight.ToFlight(), _flightLog));
			}
		}

		public TimeSpan TotalFlightDuration
		{
			get
			{
				return _flightLog.GetTotalFlightDuration();
			}
		}
		public List<int> YearsOfFlying
		{
			get
			{
				List<int> l_yearsOfFlying = FlightListViewModel.Select(f => f.TakeOffDateTime.Year).Distinct().ToList();
				l_yearsOfFlying.Sort();
				return l_yearsOfFlying;

			}
		}

		public IEnumerable<String> SiteNameList { get { return SiteListViewModel.Select(s => s.Name); } }
		
		/// <summary>
		/// Return a TimeSpan representing the cumulative flight duration in the period between start and end
		/// </summary>
		/// <param name="start"></param>
		/// <param name="end"></param>
		/// <returns></returns>
		public TimeSpan FlightDurationInPeriod(DateTime start, DateTime end)
		{
			return _flightLog.GetTotalFlightDuration(start, end);
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

		public List<FlightViewModel> FlightListViewModel { get; private set; } = [];
		public List<SiteViewModel> SiteListViewModel { get; private set; } = [];
		public List<GliderViewModel> GliderListViewModel { get; private set; } = [];
	}
}

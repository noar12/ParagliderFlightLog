using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BlazorUI.ViewModels;
using ParagliderFlightLog.DataAccess;
using ParagliderFlightLog.Models;

namespace ParagliderFlightLog.ViewModels
{
    public class MainViewModel
    {
        FlightLogDB _flightLog;
        IConfiguration _config;

        public MainViewModel(IConfiguration config)
        {
            _config = config;
            _flightLog = new FlightLogDB(_config); //todo move to DI
            FlightListViewModel = _flightLog.Flights.Select(f => f.ToVM(_flightLog)).ToList();
            SiteListViewModel = _flightLog.Sites.Select(s => s.ToVM(_flightLog)).ToList();
        }





        public void EditFlight(FlightViewModel selectedItem)
        {
            Flight? l_OldFlight = _flightLog.Flights.Where(f => f.Flight_ID == selectedItem.FlightID).FirstOrDefault();
            if (l_OldFlight != null)
            {
                int l_Index = _flightLog.Flights.IndexOf(l_OldFlight);
                _flightLog.Flights[l_Index] = selectedItem.Flight;
            }

        }

        public void EditSite(SiteViewModel selectedItem)
        {

            Site? l_OldSite = _flightLog.Sites.Where(s => s.Site_ID == selectedItem.Site_ID).FirstOrDefault();
            if (l_OldSite != null)
            {
                int l_Index = _flightLog.Sites.IndexOf(l_OldSite);
                _flightLog.Sites[l_Index] = selectedItem.Site;
            }


        }

        public void EditGlider(GliderViewModel selectedItem)
        {
            Glider? l_OldGlider = _flightLog.Gliders.Where(g => g.Glider_ID == selectedItem.GliderId).FirstOrDefault();
            if (l_OldGlider != null)
            {
                int l_Index = _flightLog.Gliders.IndexOf(l_OldGlider);
                _flightLog.Gliders[l_Index] = selectedItem.Glider;
            }
        }

        public void AddSite(SiteViewModel svm)
        {
            _flightLog.Sites.Add(svm.Site);
        }

        public void AddGlider()
        {

            throw new NotImplementedException();
        }

        public void ImportLogFlyDB(string fileName)
        {
            LogFlyDB l_logFlyDB = new LogFlyDB(_flightLog);//todo: put it in DI
            l_logFlyDB.LoadLogFlyDB(fileName);
            FlightLogDB l_FlightLogDB = l_logFlyDB.BuildFlightLogDB();
            foreach (Glider glider in l_FlightLogDB.Gliders)
            {
                _flightLog.Gliders.Add(glider);
            }
            foreach (Site site in l_FlightLogDB.Sites)
            {
                _flightLog.Sites.Add(site);
            }
            foreach (Flight flight in l_FlightLogDB.Flights)
            {
                _flightLog.Flights.Add(flight);
            }


        }
        /// <summary>
        /// Import an IGC file in the data model and use the result to instanciate and add a new FlightViewModel in the FlightListViewModel to update the UI and do the same with the takeoff site if it doesn't exist yet.
        /// </summary>
        /// <param name="filePath"></param>
        public void AddFlightsFromIGC(string[] filePaths)
        {
            foreach (string filePath in filePaths)
            {
                Flight flight = _flightLog.ImportFlightFromIGC(filePath);
                FlightListViewModel.Add(new FlightViewModel(flight, _flightLog));
                
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
        public List<FlightViewModel> FlightListViewModel { get; set; }
        public List<SiteViewModel> SiteListViewModel { get; set; }
        public List<GliderViewModel> GliderListViewModel { get => _flightLog.Gliders.Select(g => g.ToVM(_flightLog)).ToList(); }
    }
}

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ParagliderFlightLog.DataModels;

namespace ParagliderFlightLog.ViewModels
{
    public class MainViewModel
    {
        FlightLogDB m_flightLog;
        Settings m_Settings;

        public MainViewModel()
        {
            m_Settings = new Settings();
            m_Settings.Build();
            m_flightLog = new FlightLogDB(m_Settings);

            m_flightLog.LoadFlightLogDB();
            m_flightLog.Flights.CollectionChanged += Flights_CollectionChanged;
            m_flightLog.Sites.CollectionChanged += Sites_CollectionChanged;
            m_flightLog.Gliders.CollectionChanged += Gliders_CollectionChanged;

            BuildSiteListViewModel();
            BuildFlightListViewModel();
            BuildGliderListViewModel();

            //m_flightLog.Flights.CollectionChanged += UpdateFlightListViewModel;
            //FlightListViewModel.CollectionChanged += FlightListViewModel_CollectionChanged;
            //SiteListViewModel.CollectionChanged += SiteListViewModel_CollectionChanged;
        }

 

        private void Flights_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    foreach (var item in e.NewItems ?? new List<Flight>())
                    {
                        if (item is Flight flight)
                        {
                            FlightListViewModel.Add(new FlightViewModel(flight, m_flightLog.Flights, m_flightLog.Sites, m_flightLog.Gliders));
                        }
                    }
                    break;
                case NotifyCollectionChangedAction.Remove:
                    foreach (var item in e.OldItems ?? new List<Flight>())
                    {
                        if (item is Flight flight)
                        {
                            FlightListViewModel.Remove(FlightListViewModel.Where(f => f.FlightID == flight.Flight_ID).FirstOrDefault(new FlightViewModel()));
                        }
                    }
                    break;
                case NotifyCollectionChangedAction.Replace:
                    foreach (var item in e.NewItems ?? new List<Flight>())
                    {
                        if (item is Flight flight)
                        {
                            FlightViewModel? l_OldFlgihtViewModel = FlightListViewModel.Where(f => f.FlightID == flight.Flight_ID).FirstOrDefault();
                            if (l_OldFlgihtViewModel != null)
                            {
                                int l_Index = FlightListViewModel.IndexOf(l_OldFlgihtViewModel);
                                FlightListViewModel[l_Index].Flight = flight;
                            }
                        }
                    }
                    break;
                default:
                    break;
            }
        }
        private void Sites_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    foreach (var item in e.NewItems ?? new List<Site>())
                    {
                        // if the site is not in the data model yet we add it
                        if (item is Site site)
                        {
                            SiteListViewModel.Add(new SiteViewModel(site, m_flightLog.Flights));
                        }
                    }
                    break;
                case NotifyCollectionChangedAction.Remove:
                    throw new NotImplementedException();
                case NotifyCollectionChangedAction.Replace:
                    foreach (var item in e.NewItems ?? new List<Site>())
                    {
                        if (item is Site site)
                        {
                            SiteViewModel? l_OldSiteViewModel = SiteListViewModel.Where(s => s.Site_ID == site.Site_ID).FirstOrDefault();
                            if (l_OldSiteViewModel != null)
                            {
                                int l_Index = SiteListViewModel.IndexOf(l_OldSiteViewModel);
                                SiteListViewModel[l_Index].Site = site;
                            }
                        }
                    }
                    break;

                default:
                    break;
            }
        }
        private void Gliders_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Replace:
                    foreach (var item in e.NewItems ?? new List<Glider>())
                    {
                        if( item is Glider glider)
                        {
                            GliderViewModel? gliderViewModel = GliderListViewModel.Where(g => g.GliderId == glider.Glider_ID).FirstOrDefault();
                            if (gliderViewModel != null)
                            {
                                int l_Index = GliderListViewModel.IndexOf(gliderViewModel);
                                GliderListViewModel[l_Index].Glider = glider;
                            }
                        }
                    }
                    break;
            }
        }
        public void EditFlight(FlightViewModel selectedItem)
        {
            Flight? l_OldFlight= m_flightLog.Flights.Where(f => f.Flight_ID == selectedItem.FlightID).FirstOrDefault();
            if (l_OldFlight != null)
            {
                int l_Index = m_flightLog.Flights.IndexOf(l_OldFlight);
                m_flightLog.Flights[l_Index] = selectedItem.Flight;
            }
            
        }

        public void EditSite(SiteViewModel selectedItem)
        {

            Site? l_OldSite = m_flightLog.Sites.Where(s => s.Site_ID == selectedItem.Site_ID).FirstOrDefault();
            if (l_OldSite != null)
            {
                int l_Index = m_flightLog.Sites.IndexOf(l_OldSite);
                m_flightLog.Sites[l_Index] = selectedItem.Site;
            }
            
            
        }

        public void EditGlider(GliderViewModel selectedItem)
        {
            Glider? l_OldGlider = m_flightLog.Gliders.Where(g => g.Glider_ID == selectedItem.GliderId).FirstOrDefault();
            if (l_OldGlider != null)
            {
                int l_Index = m_flightLog.Gliders.IndexOf(l_OldGlider);
                m_flightLog.Gliders[l_Index] = selectedItem.Glider;
            }
        }

        public void AddSite(SiteViewModel svm)
        {
            m_flightLog.Sites.Add(svm.Site);
        }

        public void AddGlider(GliderViewModel  gvm)
        {
            m_flightLog.Gliders.Add(gvm.Glider);
        }




        private void BuildSiteListViewModel()
        {
            foreach (Site site in m_flightLog.Sites)
            {
                SiteListViewModel.Add(new SiteViewModel(site, m_flightLog.Flights));
            }
        }

        private void BuildFlightListViewModel()
        {

            foreach (Flight flight in m_flightLog.Flights)
            {
                FlightViewModel fvm = new FlightViewModel(flight, m_flightLog.Flights, m_flightLog.Sites, m_flightLog.Gliders);
                FlightListViewModel.Add(fvm);
            }
        }

        private void BuildGliderListViewModel()
        {
            foreach (Glider glider in m_flightLog.Gliders)
            {
                GliderListViewModel.Add(new GliderViewModel(glider, m_flightLog.Flights));
            }
        }
        public void ImportLogFlyDB(string fileName)
        {
            LogFlyDB l_logFlyDB = new LogFlyDB(m_Settings);
            l_logFlyDB.LoadLogFlyDB(fileName);
            FlightLogDB l_FlightLogDB = l_logFlyDB.BuildFlightLogDB();
            foreach (Glider glider in l_FlightLogDB.Gliders)
            {
                m_flightLog.Gliders.Add(glider);
            }
            foreach (Site site in l_FlightLogDB.Sites)
            {
                m_flightLog.Sites.Add(site);
            }
            foreach (Flight flight in l_FlightLogDB.Flights)
            {
                m_flightLog.Flights.Add(flight);
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
                m_flightLog.ImportFlightFromIGC(filePath);
            }
        }

        public TimeSpan TotalFlightDuration
        {
            get
            {
                return m_flightLog.GetTotalFlightDuration();
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
        public IEnumerable<String> GliderNameList { get { return m_flightLog.Gliders.Select(g => g.FullName); } }
        /// <summary>
        /// Return a TimeSpan representing the cumulative flight duration in the period between start and end
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        public TimeSpan FlightDurationInPeriod(DateTime start, DateTime end)
        {
            return m_flightLog.GetTotalFlightDuration(start, end);
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
        public ObservableCollection<FlightViewModel> FlightListViewModel { get; set; } = new ObservableCollection<FlightViewModel>();
        public ObservableCollection<SiteViewModel> SiteListViewModel { get; set; } = new ObservableCollection<SiteViewModel>();
        public ObservableCollection<GliderViewModel> GliderListViewModel { get; set; } = new ObservableCollection<GliderViewModel>();
    }
}

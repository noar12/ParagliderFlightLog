using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ParagliderFlightLog.DataModels;

namespace WpfUI.ViewModels
{
    public class MainViewModel
    {
        FlightLogDB m_flightLog = new FlightLogDB();

        public MainViewModel()
        {
            //LogFlyDB l_logFlyDB = new LogFlyDB();
            //l_logFlyDB.LoadLogFlyDB("Logfly.db");
            //m_flightLog.Flights.CollectionChanged += BuildFlightListViewModel; It does not work so i am calling the method manually for now
            //m_flightLog = l_logFlyDB.BuildFlightLogDB();



            //BuildFlightListViewModel(null , null); //jsut to call it until i understand how to trigger it through collectionchanged
            //BuildSiteListViewModel();

            m_flightLog.LoadFlightLogDB();

            BuildSiteListViewModel();
            BuildFlightListViewModel();

            //m_flightLog.Flights.CollectionChanged += UpdateFlightListViewModel;
            FlightListViewModel.CollectionChanged += FlightListViewModel_CollectionChanged;
            SiteListViewModel.CollectionChanged += SiteListViewModel_CollectionChanged;






        }

        private void SiteListViewModel_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    foreach (var item in e.NewItems)
                    {
                        // if the site is not in the data model yet we add it
                        if (item is SiteViewModel svm)
                        {
                            if (m_flightLog.Sites.Where(s => s.Site_ID == svm.Site_ID).Count() == 0)
                            {
                                Enum.TryParse(svm.Country, out ECountry l_Country);
                                Enum.TryParse(svm.WindOrientationBegin, out EWindOrientation l_WindOrientationBegin);
                                Enum.TryParse(svm.WindOrientationEnd, out EWindOrientation l_WindOrientationEnd);
                                Site l_site = new Site()
                                {
                                    Site_ID = svm.Site_ID,
                                    Name = svm.Name,
                                    Altitude = svm.Altitude,
                                    Latitude = svm.Latitude,
                                    Longitude = svm.Longitude,
                                    Country = l_Country,
                                    Town = svm.Town,
                                    WindOrientationBegin = l_WindOrientationBegin,
                                    WindOrientationEnd = l_WindOrientationEnd,

                                };
                                m_flightLog.Sites.Add(l_site);
                            }
                        }

                    }
                    break;
                case NotifyCollectionChangedAction.Remove:
                    throw new NotImplementedException();
                    break;
                case NotifyCollectionChangedAction.Replace:
                    throw new NotImplementedException();
                    break;
                
                default:
                    break;
            }
        }

        private void FlightListViewModel_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Remove:
                    foreach(var item in e.OldItems)
                    {
                        if (item is FlightViewModel fvm)
                        {

                            m_flightLog.Flights.Remove(m_flightLog.Flights.Where(f => f.Flight_ID == fvm.FlightID).FirstOrDefault(new Flight()));
                        }
                    }
                    break;
                case NotifyCollectionChangedAction.Replace:
                    throw new NotImplementedException();
                    break;
                default:
                    break;
            }
        }

        //private void UpdateFlightListViewModel(object? sender, NotifyCollectionChangedEventArgs e)
        //{
        //    switch (e.Action)
        //    {
        //        case NotifyCollectionChangedAction.Add:
        //            foreach (var item in e.NewItems)
        //            {
        //                if (item is Flight flight)
        //                {
        //                    string l_TakeOffSiteName = m_flightLog.Sites.Where(site => site.Site_ID == flight.REF_TakeOffSite_ID)
        //                        .FirstOrDefault(new Site() { Name = "Site not found" }).Name;
        //                    string l_GliderName = m_flightLog.Gliders.Where(glider => glider.Glider_ID == flight.REF_Glider_ID)
        //                        .FirstOrDefault(new Glider() { Model = "Glider not found" }).Model;
        //                    FlightViewModel fvm = new FlightViewModel(flight.Flight_ID, flight.TakeOffDateTime,
        //                        flight.FlightDuration, l_TakeOffSiteName, l_GliderName, flight.FlightPoints, flight.Comment);
                
        //                    FlightListViewModel.Add(fvm);
        //                }
                        
        //            }
                    
        //            break;
        //        case NotifyCollectionChangedAction.Remove:
        //            foreach (var item in e.OldItems)
        //            {
        //                if (item is Flight flight)
        //                {
                            
        //                    FlightListViewModel.Remove(FlightListViewModel.Where(fwm => fwm.FlightID == flight.Flight_ID).FirstOrDefault(new FlightViewModel()));
        //                }
        //            }
        //            break;

        //        default:
        //            throw new InvalidOperationException();
        //            break;
        //    }
        //}

        private void BuildSiteListViewModel()
        {
            foreach (Site site in m_flightLog.Sites)
            {
                SiteListViewModel.Add(new SiteViewModel(site));
            }
        }

        private void BuildFlightListViewModel()
        {
            
            foreach (Flight flight in m_flightLog.Flights)
            {
                FlightViewModel fvm = new FlightViewModel(flight,m_flightLog.Sites, m_flightLog.Gliders);
                FlightListViewModel.Add(fvm);
            }
             
        }

        internal void ImportLogFlyDB(string fileName)
        {
            LogFlyDB l_logFlyDB = new LogFlyDB();
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
        internal void AddFlightFromIGC(string filePath)
        {

            (Flight l_NewFlight, Site l_NewSite) = m_flightLog.ImportFlightFromIGC(filePath);

            FlightViewModel fvm = new FlightViewModel(l_NewFlight, m_flightLog.Sites, m_flightLog.Gliders);
            FlightListViewModel.Add(fvm);
            if(SiteListViewModel.Where(s => s.Site_ID == l_NewSite.Site_ID).Count() == 0)
            {
                SiteViewModel svm = new SiteViewModel(l_NewSite);
                
                SiteListViewModel.Add(svm);
            }

                
          
        }

        public TimeSpan TotalFlightDuration { get {
                return m_flightLog.GetTotalFlightDuration();
                    } }
        public List<int> YearsOfFlying
        {
            get
            {
                List <int> l_yearsOfFlying = FlightListViewModel.Select(f => f.TakeOffDateTime.Year).Distinct().ToList();
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
        


    }
}

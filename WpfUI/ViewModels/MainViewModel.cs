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
            LogFlyDB l_logFlyDB = new LogFlyDB();
            l_logFlyDB.LoadLogFlyDB("Logfly.db");
            //m_flightLog.Flights.CollectionChanged += BuildFlightListViewModel; It does not work so i am calling the method manually for now
            m_flightLog = l_logFlyDB.BuildFlightLogDB();



            BuildFlightListViewModel(null , null); //jsut to call it until i understand how to trigger it through collectionchanged
            BuildSiteListViewModel();
            

            

        }

        private void BuildSiteListViewModel()
        {
            foreach (Site site in m_flightLog.Sites)
            {
                SiteListViewModel.Add(new SiteViewModel()
                {
                    Name = site.Name,
                    Altitude = site.Altitude,
                    WindOrientation = $"{site.WindOrientationBegin} - {site.WindOrientationEnd}"
                });
            }
        }

        private void BuildFlightListViewModel(object sender, NotifyCollectionChangedEventArgs e)
        {
            
            foreach (Flight flight in m_flightLog.Flights)
            {
                //Site? l_site = (from s in m_flightLog.Sites where s.Site_ID == flight.REF_TakeOffSite_ID select s).FirstOrDefault();
                //string l_siteName = l_site != null ? l_site.Name : "Site not found";
                FlightListViewModel.Add(new FlightViewModel()
                {
                    TakeOffDateTime = flight.TakeOffDateTime,
                    TakeOffSiteName = m_flightLog.Sites.Where(site => site.Site_ID == flight.REF_TakeOffSite_ID).FirstOrDefault(new Site() { Name = "Site not found" }).Name,
                    //TakeOffSiteName = (from s in m_flightLog.Sites where s.Site_ID == flight.REF_TakeOffSite_ID select s).FirstOrDefault().Name,
                    //TakeOffSiteName = l_siteName,
                    GliderName = m_flightLog.Gliders.Where(glider => glider.Glider_ID == flight.REF_Glider_ID).FirstOrDefault(new Glider() { Model = "Glider not found" }).Model,
                    FlightDuration = flight.FlightDuration,
                    Comment = flight.Comment,
                    FlightPoints = flight.FlightPoints

                });
            }
             
        }


        internal void AddFlightFromIGC(string fileName)
        {
            m_flightLog.ImportFlightFromIGC(fileName);
            var test = m_flightLog.Flights[m_flightLog.Flights.Count - 2].FlightPoints.Count;
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

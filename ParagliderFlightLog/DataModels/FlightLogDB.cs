using System;
using System.IO;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Globalization;

namespace ParagliderFlightLog.DataModels
{
    public class FlightLogDB
    {
        private ObservableCollection<Flight> m_flights = new System.Collections.ObjectModel.ObservableCollection<Flight>();
        private ObservableCollection<Site> m_sites = new System.Collections.ObjectModel.ObservableCollection<Site>();
        private ObservableCollection<Glider> m_gliders = new System.Collections.ObjectModel.ObservableCollection<Glider>();

        public ObservableCollection<Flight> Flights { get => m_flights; set => m_flights = value; }
        public ObservableCollection<Site> Sites { get => m_sites; set => m_sites = value; }
        public ObservableCollection<Glider> Gliders { get => m_gliders; set => m_gliders = value; }
        /// <summary>
        /// Get the cumulative flight duration of all the flight between analyzePeriodStart and analyszePeriodEnd
        /// </summary>
        /// <param name="analyzePeriodStart"></param>
        /// <param name="analyzePeriodEnd"></param>
        /// <returns></returns>
        public TimeSpan GetTotalFlightDuration(DateTime? analyzePeriodStart = null, DateTime? analyzePeriodEnd = null)
        {
            if (analyzePeriodStart == null)
            {
                analyzePeriodStart = DateTime.MinValue;
            }
            if (analyzePeriodEnd == null)
            {
                analyzePeriodEnd = DateTime.Now;
            }
            TimeSpan l_totalFlightDuration = new TimeSpan(0, 0, 0, 0);

            l_totalFlightDuration = Flights.Where(flight => flight.TakeOffDateTime > analyzePeriodStart && flight.TakeOffDateTime < analyzePeriodEnd)
                .Aggregate(TimeSpan.Zero, (subtotal, flight) => subtotal.Add(flight.FlightDuration));

            return l_totalFlightDuration;
        }
        /// <summary>
        /// Import a IGC file and put it as a Flight in the datamodel.
        /// </summary>
        /// <param name="IGC_FilePath"></param>
        /// <exception cref="NotImplementedException"></exception>
        public void ImportFlightFromIGC(string IGC_FilePath) 
        {
            Flight l_Newflight = new Flight();
            
            using (var sr = new StreamReader(IGC_FilePath))
            {
                // to be done: check if it is a correct igc file before injecting
                   l_Newflight.IgcFileContent = sr.ReadToEnd();           
            }

            // check if we were able to parse some point before inserting the new flight
            if (l_Newflight.FlightPoints.Any())
            {
                m_flights.Add(l_Newflight);
            }
            else
            {
                throw new Exception();
            }
        }
    }
  
}

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

    }
  
}

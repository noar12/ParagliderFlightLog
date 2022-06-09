using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ParagliderFlightLog.DataModels;

namespace ParagliderFlightLog
{
    public class FlightStatistic
    {
        
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


            return l_totalFlightDuration;
        }
    }
}

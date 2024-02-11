using ParagliderFlightLog.DataAccess;
using ParagliderFlightLog.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParagliderFlightLog.ViewModels
{
    public class GliderViewModel
    {
        private readonly FlightLogDB _db;

        public GliderViewModel(Glider glider, FlightLogDB db)
        {
            Glider = glider;
            _db = db;
        }
        public Glider Glider { get; set; }
        public string GliderId { get => Glider.Glider_ID; }
        public string Manufactuer { get => Glider.Manufacturer; set => Glider.Manufacturer = value; }
        public string Model { get => Glider.Model; set => Glider.Model = value; }
        public int BuildYear { get => Glider.BuildYear; set => Glider.BuildYear = value; }
        public string IgcName { get => Glider.IGC_Name; set => Glider.IGC_Name = value; }

        public string FullName { get => Glider.FullName; }
        public int TotalFlightCount { get => _db.GetFlightDoneCountWithGlider(Glider); }
        public string TotalFlightTime
        {
            get
            {
                TimeSpan l_TotalFlightTime = _db.FlightTimeInPeriodWithGlider(Glider, DateTime.MinValue, DateTime.MaxValue);
                return $"{(int)l_TotalFlightTime.TotalHours}:{l_TotalFlightTime.Minutes.ToString().PadLeft(2,'0')}";
            }
        }
        public string FlightTimeSinceLastCheck
        {
            get
            {
                TimeSpan l_TimeSinceLastCheck = _db.FlightTimeInPeriodWithGlider(Glider, Glider.LastCheckDateTime, DateTime.MaxValue);
                return $"{(int)l_TimeSinceLastCheck.TotalHours}:{l_TimeSinceLastCheck.Minutes.ToString().PadLeft(2, '0')}";
            }
        }
        public DateTime LastCheckDateTime { get => Glider.LastCheckDateTime; set => Glider.LastCheckDateTime = value; }

        public EHomologationCategory HomologationCategory { get => Glider.HomologationCategory; set => Glider.HomologationCategory = value; }
        public string HomologationCategoryDisplay
        {
            get => Glider.HomologationCategory.ToString();
            set
            {
                bool success = Enum.TryParse<EHomologationCategory>(value, out EHomologationCategory homologation);
                Glider.HomologationCategory = success ? homologation: EHomologationCategory.Undefined;
            }

        }


        public static string[] HomologationCategories { get => Enum.GetNames(typeof(EHomologationCategory)); }
        public void SetHomologationCategoryByName(string categoryName)
        {
            if(Enum.TryParse<EHomologationCategory>(categoryName, out EHomologationCategory parsedCat))
            {
                Glider.HomologationCategory = parsedCat;
            }


        }

        public override string ToString()
        {
            return FullName;
        }

    }
}

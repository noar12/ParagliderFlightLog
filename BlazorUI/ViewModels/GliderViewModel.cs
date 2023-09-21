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
        private Glider m_Glider;
        private readonly FlightLogDB _db;
        private ICollection<Flight> m_FlightCollection = new List<Flight>();

        public GliderViewModel(Glider glider, FlightLogDB db)
        {
            m_Glider = glider;
            _db = db;
        }
        public Glider Glider { get { return m_Glider; } set { m_Glider = value; } }
        public string GliderId { get => m_Glider.Glider_ID; }
        public string Manufactuer { get => m_Glider.Manufacturer; set => m_Glider.Manufacturer = value; }
        public string Model { get => m_Glider.Model; set => m_Glider.Model = value; }
        public int BuildYear { get => m_Glider.BuildYear; set => m_Glider.BuildYear = value; }
        public string IgcName { get => m_Glider.IGC_Name; set => m_Glider.IGC_Name = value; }

        public string FullName { get => m_Glider.FullName; }
        public int TotalFlightCount { get => _db.GetFlightDoneCountWithGlider(m_Glider); }
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
                TimeSpan l_TimeSinceLastCheck = _db.FlightTimeInPeriodWithGlider(Glider, m_Glider.LastCheckDateTime, DateTime.MaxValue);
                return $"{(int)l_TimeSinceLastCheck.TotalHours}:{l_TimeSinceLastCheck.Minutes.ToString().PadLeft(2, '0')}";
            }
        }
        public DateTime LastCheckDateTime { get => m_Glider.LastCheckDateTime; set => m_Glider.LastCheckDateTime = value; }

        public EHomologationCategory HomologationCategory { get => m_Glider.HomologationCategory; set => m_Glider.HomologationCategory = value; }
        public string HomologationCategoryDisplay
        {
            get => m_Glider.HomologationCategory.ToString();
            set
            {
                Enum.TryParse<EHomologationCategory>(value, out EHomologationCategory homologation);
                m_Glider.HomologationCategory = homologation;
            }

        }


        public static string[] HomologationCategories { get => Enum.GetNames(typeof(EHomologationCategory)); }
        public void SetHomologationCategoryByName(string categoryName)
        {
            if(Enum.TryParse<EHomologationCategory>(categoryName, out EHomologationCategory parsedCat))
            {
                m_Glider.HomologationCategory = parsedCat;
            }


        }

        public override string ToString()
        {
            return FullName;
        }

    }
}

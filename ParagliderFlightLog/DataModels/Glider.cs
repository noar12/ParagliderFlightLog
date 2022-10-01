using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ParagliderFlightLog.DataModels
{
    public class Glider : INotifyPropertyChanged
    {
        private string m_glider_ID = Guid.NewGuid().ToString();

        private string m_manufacturer = "Unknown manufacturer";

        private string m_model = "Unkown model";
        private string m_IGC_Name = "UnknownGlider";
        private int m_buildYear;

        private System.DateTime m_lastCheckDateTime;

        private bool m_lastCheckDateTimeSpecified;

        private EHomologationCategory m_homologationCategory;

        public string Glider_ID { get => m_glider_ID; set => m_glider_ID = value; }
        public string Manufacturer { get => m_manufacturer; set => m_manufacturer = value; }
        public string Model { get => m_model; set => m_model = value; }
        public string IGC_Name { get => m_IGC_Name; set => m_IGC_Name = value; }
        public int BuildYear { get => m_buildYear; set => m_buildYear = value; }
        public DateTime LastCheckDateTime { get => m_lastCheckDateTime; set => m_lastCheckDateTime = value; }
        public bool LastCheckDateTimeSpecified { get => m_lastCheckDateTimeSpecified; set => m_lastCheckDateTimeSpecified = value; }
        public EHomologationCategory HomologationCategory { get => m_homologationCategory; set => m_homologationCategory = value; }
        public string FullName { get { return $"{Manufacturer} {Model}"; } }
        public event PropertyChangedEventHandler? PropertyChanged;
        // This method is called by the Set accessor of each property.  
        // The CallerMemberName attribute that is applied to the optional propertyName  
        // parameter causes the property name of the caller to be substituted as an argument.  
        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public enum EHomologationCategory
    {
        ENALow,

        /// <remarks/>

        ENAHigh,

        /// <remarks/>

        ENBLow,

        /// <remarks/>

        ENBHigh,

        /// <remarks/>

        ENCLow,

        /// <remarks/>

        ENCHigh,

        /// <remarks/>

        END,

        /// <remarks/>
        CCC,
    }
}



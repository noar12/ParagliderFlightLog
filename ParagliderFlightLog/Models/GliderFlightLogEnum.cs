using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParagliderFlightLog.Models
{
    public enum ECountry
    {
        Undefined = -1,
        /// <remarks/>
        Switzerland,

        /// <remarks/>
        France,

        /// <remarks/>
        Italy,

        /// <remarks/>
        Austria,

        /// <remarks/>
        Spain,
    }
    public enum EWindOrientation
    {
        Undefined = -1,
        /// <remarks/>
        North,
        NorthEast,

        /// <remarks/>
        East,
        SouthEast,

        /// <remarks/>
        South,
        SouthWest,

        /// <remarks/>
        West,
        NorthWest,
    }
    public enum EHomologationCategory
    {
        Undefined = -1,
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

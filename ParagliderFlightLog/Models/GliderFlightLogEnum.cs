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
        
        /// <remarks/>
        Greece,
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
    /// <summary>
    /// Objective of a flight
    /// </summary>
    public enum EFlightObjective
    {
        /// <summary>
        /// Not defined, default value if the user has not specified anything
        /// </summary>
        Undefined = 0,
        /// <summary>
        /// Glide only to enjoy the air and/or preserve knee.
        /// </summary>
        Glide = 1,
        /// <summary>
        /// Fly locally, staying within a certain area.
        /// </summary>
        Local = 2,
        /// <summary>
        /// Cross-country flight, aiming to cover long distances.
        /// </summary>
        XC = 3,
        /// <summary>
        /// Represents a competition event type.
        /// </summary>
        Competition = 4,
    }
}

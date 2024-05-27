﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParagliderFlightLog.Models
{

    public class XcScoreGeoJson
    {
        public string type { get; set; }
        public RootProperties properties { get; set; }
        public Feature[] features { get; set; }
    }

    public class RootProperties
    {
        public string name { get; set; }
        public int id { get; set; }
        public float score { get; set; }
        public float bound { get; set; }
        public bool optimal { get; set; }
        public float processedTime { get; set; }
        public int processedSolutions { get; set; }
        public string type { get; set; }
        public string code { get; set; }
    }

    public class Feature
    {
        public string type { get; set; }
        public string id { get; set; }
        public FeatureProperties properties { get; set; }
        public Geometry geometry { get; set; }
    }

    public class FeatureProperties
    {
        public string id { get; set; }
        public int r { get; set; }
        public long timestamp { get; set; }
        public string stroke { get; set; }
        public int strokewidth { get; set; }
        public float d { get; set; }
    }

    public class Geometry
    {
        public string type { get; set; }
        public object[] coordinates { get; set; }
        public Style style { get; set; }
    }

    public class Style
    {
        public string stroke { get; set; }
        public int strokewidth { get; set; }
    }
}
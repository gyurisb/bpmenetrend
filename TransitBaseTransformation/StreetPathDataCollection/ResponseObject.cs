using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StreetPathData
{
    class ResponseObject
    {
        public List<RouteObject> Routes { get; set; }
        public string Status { get; set; }
    }

    class RouteObject
    {
        public List<LegObject> Legs { get; set; }
    }

    class LegObject
    {
        public DistanceObject Distance { get; set; }
        public DurationObject Duration { get; set; }
    }

    class DistanceObject
    {
        public string Text { get; set; }
        public int Value { get; set; }
    }

    class DurationObject
    {
        public string Text { get; set; }
        public int Value { get; set; }
    }
}

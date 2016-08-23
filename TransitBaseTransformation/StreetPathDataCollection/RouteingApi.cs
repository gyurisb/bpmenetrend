using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TransitBase.Entities;

namespace StreetPathData
{
    abstract class RouteingApi
    {
        public async Task<WalkPath> FindRoute(Stop stop1, Stop stop2)
        {
            return await FindRoute(stop1.Latitude, stop1.Longitude, stop2.Latitude, stop2.Longitude);
        }
        public async Task<WalkPath> FindRoute(Point p1, Point p2)
        {
            return await FindRoute(p1.Latitude, p1.Longitude, p2.Latitude, p2.Longitude);
        }
        public abstract Task<WalkPath> FindRoute(double lat1, double lon1, double lat2, double lon2);
    }
}

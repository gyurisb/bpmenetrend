using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using TransitBase.Entities;
using System.Linq;
using CityTransitApp;
using CityTransitApp.Common;

namespace CityTransitServices.Tools
{
    public static class TransitProvider
    {
        public static Tuple<DateTime, Trip>[] GetCurrentTrips(DateTime currentTime, Route route, StopGroup stopGroup, int prevCount, int nextCount, TimeSpan? limit = null)
        {
            return GetCurrentTrips(currentTime, route, stopGroup.Stops, prevCount, nextCount, limit);
        }

        public static Tuple<DateTime, Trip>[] GetCurrentTrips(DateTime currentTime, Route route, Stop stop, int prevCount, int nextCount, TimeSpan? limit = null)
        {
            return GetCurrentTrips(currentTime, route, new Stop[] { stop }, prevCount, nextCount, limit);
        }

        public static Tuple<DateTime, Trip>[] GetCurrentTrips(DateTime currentTime, Route route, IEnumerable<Stop> stops, int prevCount, int nextCount, TimeSpan? limit0 = null)
        {
            TimeSpan limit = limit0 ?? TimeSpan.FromDays(3);
            currentTime = new DateTime(currentTime.Year, currentTime.Month, currentTime.Day, currentTime.Hour, currentTime.Minute, 0);

            if (CommonComponent.Current.Planner != null)
            {
                return CommonComponent.Current.Planner.GetCurrentTrips(currentTime, route, stops, prevCount, nextCount, limit).ToArray();
            }
            return CommonComponent.Current.TB.Logic.GetCurrentTrips(currentTime, route, stops, prevCount, nextCount, limit);
        }
        
    }
}

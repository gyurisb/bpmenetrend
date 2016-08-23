using NetPortableServices;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TransitBase.Entities;

namespace TransitBase.BusinessLogic
{
    public enum BusType { Nightbus, CableCar, Bus };

    public class RouteType
    {
        static public readonly int Tram = 0;
        static public readonly int Metro = 1;
        static public readonly int RailRoad = 2;
        static public readonly int Bus = 3;
        static public readonly int Ferry = 4;
        static public readonly int CableCar = 5;
        static public readonly int Gondola = 6;
        static public readonly int Funicular = 7;
    }


    public class TransitLogic
    {
        public static readonly char[] WordDelimiterChars = new char[] { ' ', '/', ',' };

        public StopTransfers StopTransfers { get; private set; }

        private TransitBaseComponent outer;
        private SearchIndex searchIndex;

        public TransitLogic(TransitBaseComponent outer, IDirectionsService directionsService)
        {
            this.outer = outer;
            this.StopTransfers = new StopTransfers(directionsService);
        }

        public void LoadCache()
        {
            searchIndex = new SearchIndex(outer.StopGroups, outer.RouteGroups);
        }

        public IEnumerable<RouteGroup> GetCategory(int category)
        {
            return outer.RouteGroups.Where(r => r.Type == category).OrderBy(r => r.Name);
        }

        public Agency FindAgency(string name)
        {
            return outer.Agencies.First(a => a.ShortName == name);
        }

        public RouteGroup[] FindRoutes(String text)
        {
            while (searchIndex == null)
            {
                Task.Delay(100).Wait();
            }
            return searchIndex.SearchRoute(text).ToArray();
        }

        public StopGroup[] FindStops(String text)
        {
            while (searchIndex == null)
            {
                Task.Delay(100).Wait();
            }
            return searchIndex.SearchStop(text).ToArray();
        }

        public Route GetRouteByID(int id)
        {
            return outer.Routes[id];
        }
        public Stop GetStopByID(int id)
        {
            return outer.Stops[id];
        }
        public Trip GetTripByID(int id, int routeId)
        {
            foreach (var tt in outer.Routes[routeId].TripTypes)
                foreach (var t3 in tt.TripTimeTypes)
                    foreach (var trip in t3.Trips)
                        if (trip.ID == id)
                            return trip;
            throw new InvalidOperationException();
        }
        public RouteGroup GetRouteGroupByID(int id)
        {
            return outer.RouteGroups[id];
        }
        public StopGroup GetStopGroupByID(int id)
        {
            return outer.StopGroups[id];
        }

        public StopGroup SearchStop(Func<StopGroup, bool> predicate)
        {
            return outer.StopGroups.FirstOrDefault(predicate);
        }
        public Route SearchRoute(Func<Route, bool> predicate)
        {
            return outer.Routes.FirstOrDefault(predicate);
        }
        public IEnumerable<StopGroup> StopGroups { get { return outer.StopGroups; } }

        #region Timetable getters
        public Tuple<DateTime, Trip>[] GetTimetable(Route route, StopGroup stopGroup, DateTime date_)
        {
            return GetTimetable(route, stopGroup.Stops, date_);
        }
        public Tuple<DateTime, Trip>[] GetTimetable(Route route, Stop stop, DateTime date_)
        {
            return GetTimetable(route, new Stop[] { stop }, date_);
        }
        public Tuple<DateTime, Trip>[] GetTimetable(Route route, IEnumerable<Stop> stops, DateTime date_)
        {
            var sameRoutes = new HashSet<Route>(route.SameRoutes);
            var date = date_.Date;
            var list = new List<Tuple<DateTime, Trip>>();

            foreach (Stop stop in stops)
                foreach (var ttAndTime in stop.TripTypes.Where(tat => sameRoutes.Contains(tat.Item1.Route) && tat.Item1.Stops.Last().Item2.Group != stop.Group))
                    foreach (Trip trip in ttAndTime.Item1.Trips)
                    {
                        DateTime tripDate = date;
                        if (trip.StartTime + TimeSpan.FromMinutes(ttAndTime.Item2) >= TimeSpan.FromDays(1))
                            tripDate -= TimeSpan.FromDays(1);

                        if (trip.Service.IsActive(tripDate))
                            list.Add(Tuple.Create(tripDate + trip.StartTime + TimeSpan.FromMinutes(ttAndTime.Item2), trip));
                    }

            if (list.FirstOrDefault(x => x.Item2.Route == route) == null) return new Tuple<DateTime, Trip>[0];
            return list.OrderBy(x1 => x1.Item1).ToArray();
        }
        #endregion

        #region Current times
        public Tuple<DateTime, Trip>[] GetCurrentTrips(DateTime currentTime, Route route, StopGroup stopGroup, int prevCount, int nextCount, TimeSpan? limit = null)
        {
            return GetCurrentTrips(currentTime, route, stopGroup.Stops, prevCount, nextCount, limit);
        }
        public Tuple<DateTime, Trip>[] GetCurrentTrips(DateTime currentTime, Route route, Stop stop, int prevCount, int nextCount, TimeSpan? limit = null)
        {
            return GetCurrentTrips(currentTime, route, new Stop[] { stop }, prevCount, nextCount, limit);
        }
        public Tuple<DateTime, Trip>[] GetCurrentTrips(DateTime currentTime, Route route, IEnumerable<Stop> stops, int prevCount, int nextCount, TimeSpan? limit0 = null)
        {
            TimeSpan limit = limit0 ?? TimeSpan.FromDays(3);
            currentTime = new DateTime(currentTime.Year, currentTime.Month, currentTime.Day, currentTime.Hour, currentTime.Minute, 0);

            //if (Engine.NativeComponent != null)
            //{
            //    var arr = Engine.NativeComponent.GetCurrentTrips(currentTime.ToString(CultureInfo.InvariantCulture), route.ID, stops.Select(s => s.ID).ToArray(), prevCount, nextCount, (int)limit.TotalMinutes);
            //    return arr.Select(x => x.TimeDifference > limit.TotalMinutes || x.TripID == 0 ? null : Tuple.Create(currentTime + TimeSpan.FromMinutes(x.TimeDifference), GetTripByID(x.TripID, route.ID))).ToArray();
            //}

            int i = 0;
            var date = currentTime.Date;
            var trips = new List<Tuple<DateTime, Trip>>();
            while (i < nextCount && date - currentTime < limit)
            {
                var timetable = GetTimetable(route, stops, date);
                foreach (var trip in timetable)
                {
                    if (i >= nextCount) break;
                    if (trip.Item1 >= currentTime)
                    {
                        trips.Add(trip);
                        i++;
                    }
                }
                date += TimeSpan.FromDays(1);
            }
            while (i++ < nextCount) trips.Add(null);

            date = currentTime.Date;
            i = 0;
            while (i < prevCount && currentTime - (date + TimeSpan.FromDays(1)) < limit)
            {
                var timetable = GetTimetable(route, stops, date);
                foreach (var trip in timetable.Reverse())
                {
                    if (i >= prevCount) break;
                    if (trip.Item1 < currentTime)
                    {
                        trips.Insert(0, trip);
                        i++;
                    }
                }
                date -= TimeSpan.FromDays(1);
            }
            while (i++ < prevCount) trips.Insert(0, null);

            return trips.Select(x => x != null && Abs(x.Item1 - currentTime) < limit ? x : null).ToArray();
        }
        #endregion


        private static TimeSpan Abs(TimeSpan x)
        {
            return x < TimeSpan.Zero ? -x : x;
        }
    }

    //TODO refactoring: ezek használata tuple-ök helyett
    public class ArrivingTrip
    {
        public DateTime Time;
        public Trip Trip;
        public Stop Stop;
    }
    public class TripRouteEntry
    {
        public TimeSpan Time;
        public Stop Stop;
    }
}

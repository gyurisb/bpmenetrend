using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using System.Diagnostics;
using FastDatabaseLoader.Attributes;
using FastDatabaseLoader;
using TransitBase.BusinessLogic;

namespace TransitBase.Entities
{
    [Table]
    public class Route : Entity
    {
        [Column]
        public string Name { get; set; }

        [Column]
        public int RouteGroupID { get; set; }

        [ForeignKey]
        public RouteGroup RouteGroup { get; set; }

        [MultiReference(Real = true)]
        public IList<TripType> TripTypes { get; set; }

        [MultiReference(Real = true)]
        public IList<RouteStopEntry> Entries { get; set; } //TODO átnevezni

        public IEnumerable<TripTimeType> TripTimeTypes
        {
            get { return TripTypes.SelectMany(tt => tt.TripTimeTypes); }
        }

        public IEnumerable<Trip> Trips
        {
            get { return TripTypes.SelectMany(tt => tt.Trips); }
        }

        public IList<RouteStopEntry> TravelRoute { get { return Entries; } }
        //public IList<RouteStopEntry> TravelRoute { get { return CreateTravelRoute(); } }

        public IEnumerable<StopGroup> Stops
        {
            get
            {
                return TripTypes.SelectMany(tt => tt.Stops.Select(s => s.Group)).Distinct();
            }
        }

        public IEnumerable<Route> SameRoutes
        {
            get
            {
                if (!TransitBaseComponent.Current.CheckSameRoutes)
                    return new Route[] { this };
                return TransitBaseComponent.Current.Routes
                    .Where(r => r.RouteGroup.Name == RouteGroup.Name &&
                                (r.Name == Name || r.Entries.HasSequence(Entries, (x, y) => x.Stop == y.Stop)));
            }
        }

        #region TravelRouteCreation
        public static int TotalRequests = 0;
        public static int FailedRequests = 0;
        public IList<RouteStopEntry> CreateTravelRoute(bool filterTravelRouteStops)
        {
            TotalRequests++;
            if (TripTypes == null || TripTypes.Count == 0) return new RouteStopEntry[0];

            var routes = this.TripTimeTypes.Select(t3 => new GTripType { Trip = new GTrip(t3.Stops), Count = t3.Trips.Count });
            RouteGraphV2 graph = new RouteGraphV2(routes);
            if (filterTravelRouteStops)
                graph = graph.Filter(0.2);
            return graph.CreateLine().Select(e => new RouteStopEntry { Route = this, Stop = e.Stop, Time = (short)Math.Round(e.TimeInMinutes) }).ToList();

            //try
            //{
                //RouteGraph graph = new RouteGraph(TripTimeTypes.Select(t3 => Tuple.Create(t3.Stops, t3.Trips.Count())));
                //return graph.CreateLine().Select(x => Tuple.Create((int)x.Item2.TotalMinutes, x.Item1)).ToList();
            //}
            //catch (Exception e)
            //{
            //    FailedRequests++;
            //    return createTravelRouteSimple();
            //    //throw;
            //}
        }

        private IList<Tuple<int, StopGroup>> createTravelRouteSimple()
        {
            //nem biztos módszer:
            Dictionary<StopGroup, int> stopTimes = new Dictionary<StopGroup, int>();
            double size = 0;

            int id = this.ID;
            foreach (var triptype in TripTimeTypes)
            {
                var stops = triptype.Stops;
                var trips = triptype.Trips.ToArray();

                foreach (var entry in stops)
                {
                    if (stopTimes.ContainsKey(entry.Item2.Group))
                        stopTimes[entry.Item2.Group] = stopTimes[entry.Item2.Group] + trips.Length * entry.Item1.Minutes;
                    else
                        stopTimes.Add(entry.Item2.Group, trips.Length * entry.Item1.Minutes);
                }

                size += trips.Length;
            }

            List<Tuple<int, StopGroup>> list = new List<Tuple<int, StopGroup>>();
            foreach (var entry in stopTimes)
                list.Add(Tuple.Create((int)Math.Round(entry.Value / size), entry.Key));
            return list.OrderBy(t => t.Item1).ToList();
        }
        #endregion
    }

    [Table(BigTable = 3)]
    public class RouteStopEntry : Entity
    {
        [Column]
        public short Time { get; set; }
        [ForeignKey]
        public StopGroup Stop { get; set; }
        [ForeignKey(Hidden = true)]
        public Route Route { get; set; }
    }

    public static class Excensions
    {
        public static bool HasSequence<T>(this IList<T> enumParent, IList<T> enumChild, Func<T, T, bool> equalityComparer)
        {
            if (enumParent.Count < enumChild.Count) return false;
            for (int i = 0; i < enumChild.Count; i++)
                if (!equalityComparer(enumParent[i], enumChild[i]))
                    return false;
            return true;
        }
    }
}

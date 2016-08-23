using FastDatabaseLoader;
using FastDatabaseLoader.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransitBase.Entities
{
    [Table]
    public class Stop : Entity
    {
        [Column]
        public string Name { get; set; }

        [Column]
        public double Latitude { get; set; }

        [Column]
        public double Longitude { get; set; }

        [Column]
        public byte WheelchairBoarding { get; set; }

        [ForeignKey]
        public StopGroup Group { get; set; }

        [MultiReference(Real = true)]
        public IList<TTEntry> TTEntries { get; set; }

        [MultiReference(Real = true)]
        public IList<Transfer> Transfers { get; set; }

        public bool WheelchairBoardingAvailable
        {
            get { return WheelchairBoarding == 1; }
        }

        public IEnumerable<Route> Routes
        {
            get
            {
                return TTEntries.Select(e => e.TripType.Route).Distinct();
            }
        }

        public IEnumerable<Tuple<Route, int>> GetRoutes()
        {
            return TTEntries.Where(e => e.Position < e.TripType.StopEntries.Count - 1).Select(e => Tuple.Create(e.TripType.Route, e.Position)).Distinct();
        }

        public Tuple<TripTimeType, short>[] TripTypes
        {
            get
            {
                return TTEntries.SelectMany(
                    tte => tte.TripType.TripTimeTypes.Select(t3 => Tuple.Create(t3, t3.TimeEntries[tte.Position].Time))
                ).ToArray();
            }
        }

        public override string ToString()
        {
            return Name;
        }

        //the distance from the other stop in meters
        public double StraightLineDistanceTo(Stop other)
        {
            double dLat = (other.Latitude - Latitude) * TransitBaseComponent.Current.LatitudeDegreeDistance;
            double dLon = (other.Longitude - Longitude) * TransitBaseComponent.Current.LongitudeDegreeDistance;
            return Math.Sqrt(dLat*dLat + dLon*dLon);
        }
        public double StraightLineDistanceTo(double latitude, double longitude)
        {
            double dLat = (latitude - Latitude) * TransitBaseComponent.Current.LatitudeDegreeDistance;
            double dLon = (longitude - Longitude) * TransitBaseComponent.Current.LongitudeDegreeDistance;
            return Math.Sqrt(dLat*dLat + dLon*dLon);
        }

        public static double StraightLineDistance(double lat1, double lon1, double lat2, double lon2)
        {
            double dLat = (lat1 - lat2) * TransitBaseComponent.Current.LatitudeDegreeDistance;
            double dLon = (lon1 - lon2) * TransitBaseComponent.Current.LongitudeDegreeDistance;
            return Math.Sqrt(dLat * dLat + dLon * dLon);
        }

        public double DirectionAngleTo(double latitude, double longitude)
        {
            double dLat = (latitude - Latitude) * TransitBaseComponent.Current.LatitudeDegreeDistance;
            double dLon = (longitude - Longitude) * TransitBaseComponent.Current.LongitudeDegreeDistance;
            if (dLon == 0) return Math.Sign(dLat) * Math.PI / 2;
            double res = Math.Atan(dLat / dLon);
            if (dLon < 0) res += Math.PI;
            if (res > Math.PI) res -= Math.PI * 2;
            return res;
        }
        public Cardinal DirectionTo(double latitude, double longitude)
        {
            double angle = DirectionAngleTo(latitude, longitude);
            return ConvertToCardinal(angle);
        }
        public Cardinal InverseDirectionTo(double latitude, double longitude)
        {
            double angle = DirectionAngleTo(latitude, longitude);
            angle += angle >= 0 ? -Math.PI : Math.PI;
            return ConvertToCardinal(angle);
        }

        public static Cardinal ConvertToCardinal(double angle)
        {
            int i = (int)Math.Floor(16 * angle / (2 * Math.PI));
            if (i == -1 || i == 0)
                return Cardinal.East;
            if (i == 1 || i == 2)
                return Cardinal.NorthEast;
            if (i == 3 || i == 4)
                return Cardinal.North;
            if (i == 5 || i == 6)
                return Cardinal.NorthWest;
            if (i == 7 || i == -8)
                return Cardinal.West;
            if (i == -7 || i == -6)
                return Cardinal.SouthWest;
            if (i == -5 || i == -4)
                return Cardinal.South;
            if (i == -3 || i == -2)
                return Cardinal.SouthEast;
            throw new InvalidOperationException();
        }

        #region legacy
        //public TimeSpan WalkTimeTo(double lat, double lon)
        //{
        //    double walkDistance = StraightLineDistanceTo(lat, lon);
        //    return TimeSpan.FromMinutes(walkDistance / Engine.Config.WalkSpeedRate);
        //}
        //public TimeSpan WalkTimeTo(Stop other)
        //{
        //    double walkDistance = StraightLineDistanceTo(other);
        //    return TimeSpan.FromMinutes(walkDistance / Engine.Config.WalkSpeedRate);
        //}
        //private TimeSpan? nextArrival(TripType tt, short travelTime, DateTime time, Func<Trip, bool> tripPred)
        //{
        //    var trips = tt.Trips.ToArray();
        //    if (trips.Length == 0) return null;
        //    List<TimeSpan> list = new List<TimeSpan>();

        //    DateTime startTime = time - TimeSpan.FromMinutes(travelTime);

        //    //A mai járat keresése
        //    var trip0 = findNextTrip(trips, startTime.Date, startTime.TimeOfDay, tripPred);
        //    if (trip0 != null)
        //        list.Add(trip0.Value);

        //    //A tegnapi éjszakai járat keresése (amely átnyúlik mába)
        //    TimeSpan startTimePlus24Hour = startTime.TimeOfDay + TimeSpan.FromDays(1);
        //    if (trips.Last().StartTime >= startTimePlus24Hour)
        //    {
        //        var trip1 = findNextTrip(trips, startTime.Date - TimeSpan.FromDays(1), startTimePlus24Hour, tripPred);
        //        if (trip1 != null)
        //            list.Add(trip1.Value);
        //    }

        //    if (list.Count == 1)
        //        return list.Single();
        //    else if (list.Count == 0)
        //    {
        //        //Ha nem találtunk járatot, a holnapi elsőt választjuk ki
        //        var trip2 = findNextTrip(trips, startTime.Date + TimeSpan.FromDays(1), TimeSpan.Zero, tripPred);
        //        if (trip2 != null)
        //            return TimeSpan.FromDays(1) - startTime.TimeOfDay + trip2.Value;
        //    }
        //    else if (list.Count == 2)
        //    {
        //        return list.Min();
        //    }
        //    return null;
        //}
        //private TimeSpan? prevArrival(TripType tt, short travelTime, DateTime time, Func<Trip, bool> tripPred)
        //{
        //    var trips = tt.Trips.ToArray();
        //    if (trips.Length == 0) return null;
        //    List<TimeSpan> list = new List<TimeSpan>();

        //    DateTime startTime = time - TimeSpan.FromMinutes(travelTime);

        //    //A mai járat keresése
        //    var trip0 = findPrevTrip(trips, startTime.Date, startTime.TimeOfDay, tripPred);
        //    if (trip0 != null)
        //        list.Add(trip0.Value);

        //    //A tegnapi éjszakai járat keresése (amely átnyúlik mába)
        //    TimeSpan startTimePlus24Hour = startTime.TimeOfDay + TimeSpan.FromDays(1);
        //    if (trips.Last().StartTime >= startTimePlus24Hour)
        //    {
        //        var trip1 = findPrevTrip(trips, startTime.Date - TimeSpan.FromDays(1), startTimePlus24Hour, tripPred);
        //        if (trip1 != null)
        //            list.Add(trip1.Value);
        //    }

        //    if (list.Count == 1)
        //        return list.Single();
        //    else if (list.Count == 0)
        //    {
        //        //Ha nem találtunk járatot, akkor a tegnapi utolsót választjuk ki
        //        var trip2 = findPrevTrip(trips, startTime.Date - TimeSpan.FromDays(1), TimeSpan.FromDays(1), tripPred);
        //        if (trip2 != null)
        //            //return TimeSpan.FromDays(1) - startTime.TimeOfDay + trip2.Value;
        //            return -startTime.TimeOfDay + trip2.Value;
        //    }
        //    else if (list.Count == 2)
        //    {
        //        return list.Max();
        //    }
        //    return null;
        //}

        //public IEnumerable<ArrivalResult> NextArrivalTripTypes(DateTime time, Func<TripType, bool> tripTypePred, Func<Trip, bool> tripPred)
        //{
        //    foreach (var ttAndTime in TripTypes)
        //    {
        //        if (tripTypePred(ttAndTime.Item1))
        //        {
        //            TimeSpan? nextArrivalTime = nextArrival(ttAndTime.Item1, ttAndTime.Item2, time, tripPred);
        //            if (nextArrivalTime != null)
        //                yield return new ArrivalResult
        //                {
        //                    ArrivalTime = nextArrivalTime.Value,
        //                    Trip = ttAndTime.Item1,
        //                    TravelTime = ttAndTime.Item2
        //                };
        //        }
        //    }
        //}
        //internal IEnumerable<ArrivalResult> PrevArrivalTripTypes(DateTime time, Func<TripType, bool> tripTypePred, Func<Trip, bool> tripPred)
        //{
        //    foreach (var ttAndTime in TripTypes)
        //    {
        //        if (tripTypePred(ttAndTime.Item1))
        //        {
        //            TimeSpan? prevArrivalTime = prevArrival(ttAndTime.Item1, ttAndTime.Item2, time, tripPred);
        //            if (prevArrivalTime != null)
        //                yield return new ArrivalResult
        //                {
        //                    ArrivalTime = prevArrivalTime.Value,
        //                    Trip = ttAndTime.Item1,
        //                    TravelTime = ttAndTime.Item2
        //                };
        //        }
        //    }
        //}
        //public struct ArrivalResult
        //{
        //    public TimeSpan ArrivalTime;
        //    public TripType Trip;
        //    public short TravelTime;
        //}

        //private class TripComparer : IComparer<Trip>
        //{
        //    public static TripComparer Current = new TripComparer();

        //    public int Compare(Trip x, Trip y)
        //    {
        //        return x.StartTime.CompareTo(y.StartTime);
        //    }
        //}

        //private TimeSpan? findNextTrip(Trip[] trips, DateTime date, TimeSpan time, Func<Trip, bool> tripPred)
        //{
        //    int index = Array.BinarySearch(trips, new Trip { StartTime = time }, TripComparer.Current);
        //    if (index >= 0)
        //    {
        //        for (int i = index - 1; i >= 0 && trips[i].StartTime == trips[index].StartTime; i--)
        //            if (trips[i].Service.IsActive(date) && tripPred(trips[i]))
        //                return trips[i].StartTime - time;
        //    }
        //    for (int i = index < 0 ? -index - 1 : index; i < trips.Length; i++)
        //        if (trips[i].Service.IsActive(date) && tripPred(trips[i]))
        //            return trips[i].StartTime - time;
        //    return null;
        //}
        //private TimeSpan? findPrevTrip(Trip[] trips, DateTime date, TimeSpan time, Func<Trip, bool> tripPred)
        //{
        //    int index = Array.BinarySearch(trips, new Trip { StartTime = time }, TripComparer.Current);
        //    if (index >= 0)
        //    {
        //        for (int i = index - 1; i >= 0 && trips[i].StartTime == trips[index].StartTime; i--)
        //            if (trips[i].Service.IsActive(date) && tripPred(trips[i]))
        //                return trips[i].StartTime - time;
        //    }
        //    for (int i = (index < 0 ? -index - 1 : index) - 1; i >= 0; i--)
        //        if (trips[i].Service.IsActive(date) && tripPred(trips[i]))
        //            return trips[i].StartTime - time;
        //    return null;
        //}

        //public IEnumerable<Tuple<TimeSpan, Route>> NextArrivals(DateTime time)
        //{
        //    return NextArrivalTripTypes(time).GroupBy(x => x.Trip.Route).Select(x => Tuple.Create(x.Min(t => t.ArrivalTime), x.Key));
        //}
#endregion


        public GeoCoordinate Coordinate { get { return new GeoCoordinate(Latitude, Longitude); } }
    }
    public enum Cardinal { East, NorthEast, North, NorthWest, West, SouthWest, South, SouthEast }
}

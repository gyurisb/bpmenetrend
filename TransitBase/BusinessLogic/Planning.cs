using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TransitBase.Entities;

namespace TransitBase.BusinessLogic
{
    public class Way : List<Way.Entry>, IEquatable<Way>
    {
        public TimeSpan TotalTime { get; set; }
        public double TotalWalkDistance { get; set; }
        public int TotalTransferCount { get; set; }
        public int LastWalkDistance { get; set; }
        public double walkSpeedMps;

        public TimeSpan DepartTime { get { return this.First().StartTime - TimeSpan.FromMinutes(this.First().WaitMinutes) - TimeSpan.FromSeconds(this.First().WalkBeforeMeters / walkSpeedMps); } }
        public TimeSpan ArrivalTime { get { return this.Last().EndTime + TimeSpan.FromSeconds(LastWalkDistance / walkSpeedMps); } }

        internal Distance TotalDist
        {
            get
            {
                return new Distance
                {
                    Time = (int)TotalTime.TotalMinutes,
                    WalkDistance = TotalWalkDistance,
                    TransferCount = TotalTransferCount + 1
                };
            }
            set
            {
                TotalTime = TimeSpan.FromMinutes(value.Time);
                TotalWalkDistance = value.WalkDistance;
                TotalTransferCount = value.TransferCount - 1;
            }
        }

        public class Entry : List<Stop>, IEquatable<Entry>
        {
            public Route Route { get; set; }
            public TripType TripType { get; set; }
            public Stop StartStop { get; set; }
            public Stop EndStop { get; set; }
            public TimeSpan StartTime, EndTime;
            public int WaitMinutes { get; set; }
            public int WalkBeforeMeters { get; set; }
            public int StopCount { get; set; }

            public Entry() { }
            public Entry(IEnumerable<Stop> stops) : base(stops) { }
            public Entry(TripType tripType)
            {
                TripType = tripType;
                StartStop = tripType.StopEntries.First().Stop;
                EndStop = tripType.StopEntries.Last().Stop;
                AddRange(tripType.StopEntries.Select(e => e.Stop));
            }

            public bool Equals(Entry other)
            {
                return StartTime == other.StartTime && EndTime == other.EndTime &&
                    WaitMinutes == other.WaitMinutes && WalkBeforeMeters == other.WalkBeforeMeters &&
                    Route == other.Route && this.SequenceEqual(other);
            }

            public IEnumerable<GeoCoordinate> ShapePoints
            {
                get
                {
                    ShapePoint[] shapePoints = this.TripType.Shape.Points.ToArray();
                    if (shapePoints.Length > 0)
                    {
                        int begin = shapePoints.IndexOfMin(p => Stop.StraightLineDistance(p.Latitude, p.Longitude, this.StartStop.Latitude, this.StartStop.Longitude));
                        int end = shapePoints.IndexOfMin(p => Stop.StraightLineDistance(p.Latitude, p.Longitude, this.EndStop.Latitude, this.EndStop.Longitude));
                        if (begin < end)
                            return shapePoints.Where((s, ind) => ind >= begin && ind <= end).Select(p => new GeoCoordinate(p.Latitude, p.Longitude));
                    }
                    return this.Select(s => new GeoCoordinate(s.Latitude, s.Longitude));
                }
            }
        }

        public bool IsNotWorse(Way other)
        {
            return TotalTime <= other.TotalTime && TotalWalkDistance <= other.TotalWalkDistance && TotalTransferCount <= other.TotalTransferCount;
        }

        public bool Equals(Way other)
        {
            return TotalDist == other.TotalDist && this.SequenceEqual(other);
        }


        public static bool operator <(Way w1, Way w2)
        {
            return w1.TotalDist < w2.TotalDist;
        }
        public static bool operator >(Way w1, Way w2)
        {
            return w1.TotalDist > w2.TotalDist;
        }
    }

    internal class Distance : IComparable<Distance>
    {
        public int Time;
        public double WalkDistance;
        public int TransferCount;

        public int CompareTo(Distance d2)
        {
            if (Time != d2.Time) return Time - d2.Time;
            else if (TransferCount != d2.TransferCount) return TransferCount - d2.TransferCount;
            else if (WalkDistance != d2.WalkDistance) return Math.Sign(WalkDistance - d2.WalkDistance);
            else return 0;
        }

        public static bool operator <(Distance d1, Distance d2)
        {
            return d1.CompareTo(d2) < 0;
        }
        public static bool operator >(Distance d1, Distance d2)
        {
            return d1.CompareTo(d2) > 0;
        }
        public static bool operator ==(Distance d1, Distance d2)
        {
            return d1.CompareTo(d2) == 0;
        }
        public static bool operator !=(Distance d1, Distance d2)
        {
            return d1.CompareTo(d2) != 0;
        }
    }
}

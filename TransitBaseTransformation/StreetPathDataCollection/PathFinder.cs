using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Json;
using System.Xml.Serialization;
using TransitBase.Entities;
using TransitBase;

namespace StreetPathData
{
    public class PathFinder
    {
        public static readonly int MaxTransferAirDistance = 200;

        private TransitBaseComponent ctx;
        Knowledge knowledge;
        List<WalkPath> newKnowledge = new List<WalkPath>();
        SortedDictionary<Point, List<WalkPath>> index;
        SortedDictionary<Point, List<Point>> scheduled;
        Tuple<Point, Point>[] queue;
        public int totalRequests;
        public int completeRequests;
        Action<string> reportProgress;

        public PathFinder() { reportProgress = x => { }; }
        public PathFinder(TransitBaseComponent ctx, Action<string> reportProgress)
        {
            this.ctx = ctx;
            this.reportProgress = reportProgress;
        }

        public void LoadData(string path)
        {
            using (var stream = File.OpenRead(path))
            {
                XmlSerializer ser = new XmlSerializer(typeof(Knowledge));
                knowledge = (Knowledge)ser.Deserialize(stream);
                var fromList = knowledge.Entries.Select(x => Tuple.Create(x.From, x));
                var toList = knowledge.Entries.Select(x => Tuple.Create(x.To, x));
                index = new SortedDictionary<Point, List<WalkPath>>();
                foreach (var entry in fromList.Concat(toList))
                {
                    if (!index.ContainsKey(entry.Item1))
                        index.Add(entry.Item1, new List<WalkPath>());
                    index[entry.Item1].Add(entry.Item2);
                }
            }
            reportProgress("Data loaded done. Total requests done: " + knowledge.Entries.Length);
        }

        public void FindRemainingPaths()
        {
            var stops = ctx.Stops.ToArray();
            scheduled = new SortedDictionary<Point, List<Point>>();
            for (int i = 0; i < stops.Length - 1; i++)
                for (int k = i + 1; k < stops.Length; k++)
                    if (IsNearNotSamePace(stops[i], stops[k]) && !ContainsPath(stops[i], stops[k]))
                        SchedulePath(stops[i], stops[k]);
            var queue2x = new List<Tuple<Point, Point>>();
            foreach (var item in scheduled)
            {
                queue2x.AddRange(item.Value.Select(x => Tuple.Create(item.Key, x)));
            }
            queue2x = queue2x.Select(x => x.Item1.CompareTo(x.Item2) > 0 ? Tuple.Create(x.Item2, x.Item1) : x).ToList();
            queue = new SortedSet<Tuple<Point, Point>>(queue2x, new PointComparer()).ToArray();
            if (queue.Length != queue2x.Count / 2)
                throw new Exception("Queue size is wrong.");
        }

        public static bool IsNear(Stop stop1, Stop stop2)
        {
            return stop1.StraightLineDistanceTo(stop2) < MaxTransferAirDistance || stop1.Group == stop2.Group;
        }
        public static bool IsNearNotSamePace(Stop stop1, Stop stop2)
        {
            return DistanceInRange(stop1.StraightLineDistanceTo(stop2)) || stop1.Group == stop2.Group;
        }
        private static bool DistanceInRange(double distance)
        {
            return 20 < distance && distance < MaxTransferAirDistance;
        }

        internal async Task CalculateTransfers(Stream errorLog, RouteingApi routeingApi, int enabledRequests = int.MaxValue)
        {
            StreamWriter errorWriter = new StreamWriter(errorLog);
            reportProgress(String.Format("Starting Google API requests. Requests count: " + queue.Length));
            totalRequests = Math.Min(enabledRequests, queue.Length);
            int maximumTry = 20;
            await Task.WhenAll(queue.Take(enabledRequests).Select(async airPath =>
            {
                try
                {
                    int tryCount = 0;
                    WalkPath walkPath = null;
                    while (walkPath == null && tryCount < maximumTry)
                    {
                        walkPath = await routeingApi.FindRoute(airPath.Item1, airPath.Item2);
                        tryCount++;
                    }
                    if (walkPath != null)
                    {
                        lock (newKnowledge)
                        {
                            newKnowledge.Add(walkPath);
                        }
                        completeRequests++;
                    }
                    else throw new Exception("Api Request limit exceeded.");
                }
                catch (Exception e)
                {
                    lock (errorWriter)
                    {
                        errorWriter.WriteLine("Error: " + e.Message + "\n" + e.StackTrace);
                        errorWriter.WriteLine("-------------------------------------------------------------------------------------------");
                    }
                }
            }));
            reportProgress(String.Format("Requests done, failure rate: {0}%", 100 - newKnowledge.Count * 100 / enabledRequests));
        }

        public void Save(string path)
        {
            reportProgress(String.Format("Saving data of {0} requests.", newKnowledge.Count));
            if (newKnowledge.Count > 0)
            {
                int notZeroCount = newKnowledge.Count(x => x.Distance != 0);
                reportProgress(String.Format("Path with distance: {0}%", notZeroCount * 100 / newKnowledge.Count));
                knowledge = new Knowledge { Entries = knowledge.Entries.Concat(newKnowledge).ToArray() };
                using (var stream = File.OpenWrite(path))
                {
                    XmlSerializer ser = new XmlSerializer(typeof(Knowledge));
                    ser.Serialize(stream, knowledge);
                }
            }
            reportProgress(String.Format("Saving done: {0}", newKnowledge.Count));
        }

        private void SchedulePath(Stop stop1, Stop stop2)
        {
            var p1 = new Point(stop1.Latitude, stop1.Longitude);
            var p2 = new Point(stop2.Latitude, stop2.Longitude);
            if (scheduled.ContainsKey(p1))
            {
                if (scheduled[p1].Any(x => x.Equals(p2)))
                    throw new InvalidOperationException("Something is wrong.");
            }
            else
            {
                scheduled.Add(p1, new List<Point>());
            }
            if (scheduled.ContainsKey(p2))
            {
                if (scheduled[p2].Any(x => x.Equals(p1)))
                    throw new InvalidOperationException("Something is wrong.");
            }
            else
            {
                scheduled.Add(p2, new List<Point>());
            }
            scheduled[p1].Add(p2);
            scheduled[p2].Add(p1);
        }
        private bool ContainsScheduledPath(Point p1, Point p2)
        {
            if (scheduled.ContainsKey(p1))
            {
                if (scheduled[p1].Any(x => x.Equals(p2)))
                    return true;
            }
            return false;
        }

        private bool ContainsPath(Stop stop1, Stop stop2)
        {
            return ContainsPath(stop1.Latitude, stop1.Longitude, stop2.Latitude, stop2.Longitude);
        }
        private bool ContainsPath(double lat1, double lon1, double lat2, double lon2)
        {
            return ContainsPath(new Point(lat1, lon1), new Point(lat2, lon2));
        }
        private bool ContainsPath(Point from, Point to)
        {
            if (index.ContainsKey(from))
            {
                var ret = index[from].Any(x => PointsEqual(from, to, x.From, x.To));
                if (ret == true)
                    return ret;
            }
            if (index.ContainsKey(to))
            {
                var ret = index[to].Any(x => PointsEqual(from, to, x.From, x.To));
                if (ret == true)
                    throw new InvalidOperationException("Something is wrong.");
            }
            return ContainsScheduledPath(from, to);
            //return false;
        }
        public WalkPath GetPath(Point from, Point to)
        {
            if (index.ContainsKey(from))
            {
                return index[from].FirstOrDefault(x => PointsEqual(from, to, x.From, x.To));
            }
            return null;
        }

        private bool PointsEqual(Point a1, Point a2, Point b1, Point b2)
        {
            return (a1.Equals(b1) && a2.Equals(b2)) || (a1.Equals(b2) && a2.Equals(b1));
        }
        private class PointComparer : IComparer<Tuple<Point, Point>>
        {
            public int Compare(Tuple<Point, Point> x, Tuple<Point, Point> y)
            {
                int ret = x.Item1.CompareTo(y.Item1);
                if (ret != 0) return ret;
                return x.Item2.CompareTo(y.Item2);
            }
        }

        private bool SamePlace(Stop s1, Stop s2)
        {
            return s1.Latitude == s2.Latitude && s1.Longitude == s2.Longitude;
        }
    }

    public class Knowledge
    {
        public WalkPath[] Entries { get; set; }
    }

    public class WalkPath
    {
        public Point From { get; set; }
        public Point To { get; set; }
        public double Distance { get; set; }
        public double Time { get; set; }
        public Point[] InnerPoints { get; set; }
        public string Query { get; set; }
    }

    public class Point : IComparable<Point>
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }

        public Point() { }
        public Point(double lat, double lon)
        {
            this.Latitude = lat;
            this.Longitude = lon;
        }

        public int CompareTo(Point other)
        {
            int comp1 = Latitude.CompareTo(other.Latitude);
            if (comp1 != 0)
                return comp1;
            return Longitude.CompareTo(other.Longitude);
        }
        public override bool Equals(object obj)
        {
            var other = obj as Point;
            if (other == null) return false;
            return Latitude == other.Latitude && Longitude == other.Longitude;
        }
    }
}

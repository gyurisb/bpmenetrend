using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TransitBase.Entities;

namespace TransitBase.BusinessLogic
{
    public class RouteGraphV2
    {
        List<Vertex> Vertexes;
        int totalTripCount;

        private class Vertex
        {
            public StopGroup Stop;
            public int Position;
            public int TripCount;
            public Dictionary<Vertex, double> Neighbors;
            public List<Vertex> ReverseNeighbors = new List<Vertex>();

            protected bool HasAncestor(Vertex vertex) { return HasAncestor(vertex, new HashSet<Vertex>()); }
            protected bool HasAncestor(Vertex vertex, HashSet<Vertex> visitedVertexes)
            {
                if (this == vertex)
                    return true;
                visitedVertexes.Add(this);
                return ReverseNeighbors.Any(n => !visitedVertexes.Contains(n) && n.HasAncestor(vertex, visitedVertexes));
            }

            private int maxDepth = -1;
            public int GetMaxDepth()
            {
                if (maxDepth < 0)
                {
                    if (ReverseNeighbors.Count == 0)
                        maxDepth = 0;
                    else
                        maxDepth = ReverseNeighbors.Max(n => n.GetMaxDepth()) + 1;
                }
                return maxDepth;
            }

            public IEnumerable<Vertex> GetComponent() { return GetComponent(new HashSet<Vertex>()); }
            public IEnumerable<Vertex> GetComponent(HashSet<Vertex> vertexes)
            {
                if (vertexes.Contains(this)) return new Vertex[0];
                vertexes.Add(this);
                return new Vertex[] { this }
                    .Concat(Neighbors.Keys.SelectMany(n => n.GetComponent(vertexes)))
                    .Concat(ReverseNeighbors.SelectMany(n => n.GetComponent(vertexes)));
            }

            public Vertex Take(HashSet<Vertex> vertexes)
            {
                return new Vertex
                {
                    Stop = this.Stop,
                    Position = this.Position,
                    TripCount = this.TripCount,
                    Neighbors = this.Neighbors.Where(n => vertexes.Contains(n.Key)).ToDictionary(x => x.Key, y => y.Value),
                    ReverseNeighbors = this.ReverseNeighbors.Intersect(vertexes).ToList()
                };
            }

            internal void Reset(List<Vertex> vertexes)
            {
                Neighbors = this.Neighbors.ToDictionary(x => vertexes.First(v => v.Stop == x.Key.Stop && v.Position == x.Key.Position), y => y.Value);
                ReverseNeighbors = this.ReverseNeighbors.Select(x => vertexes.First(v => v.Stop == x.Stop && v.Position == x.Position)).ToList();
            }
        }
        private class InitialVertex : Vertex
        {
            public Dictionary<InitialVertex, Tuple<int, int>> InitialNeighbors = new Dictionary<InitialVertex,Tuple<int,int>>();
            public bool AddNeighbor(InitialVertex neighbor, int time, int count)
            {
                Tuple<int, int> currentValue = null;
                InitialNeighbors.TryGetValue(neighbor, out currentValue);
                if (currentValue == null)
                {
                    if (this.HasAncestor(neighbor))
                        return false;
                    currentValue = Tuple.Create(0, 0);
                    neighbor.ReverseNeighbors.Add(this);
                }
                InitialNeighbors[neighbor] = Tuple.Create(currentValue.Item1 + time*count, currentValue.Item2 + count);
                return true;
            }
            public void Finalize()
            {
                base.Neighbors = InitialNeighbors.ToDictionary(x => (Vertex)x.Key, y => y.Value.Item1 / (double)y.Value.Item2);
                this.InitialNeighbors = null;
            }
        }

        public RouteGraphV2(IEnumerable<GTripType> triptypes)
        {
            this.totalTripCount = triptypes.Sum(tt => tt.Count);
            var initialVertexes = new Dictionary<Tuple<StopGroup, int>, InitialVertex>();
            foreach (var triptype in triptypes)
            {
                var currentStopPosition = new Dictionary<StopGroup, int>();
                InitialVertex prevVertex = null;
                int prevTime = 0;
                foreach (var entry in triptype.Trip)
                {
                    int currentPos = 0;
                    currentStopPosition.TryGetValue(entry.Stop.Group, out currentPos);

                    InitialVertex currentVertex = null;
                    initialVertexes.TryGetValue(Tuple.Create(entry.Stop.Group, currentPos), out currentVertex);
                    if (currentVertex == null)
                        initialVertexes.Add(Tuple.Create(entry.Stop.Group, currentPos), currentVertex = new InitialVertex { Stop = entry.Stop.Group, Position = currentPos });
                    if (prevVertex != null)
                        while (!prevVertex.AddNeighbor(currentVertex, entry.Time - prevTime, triptype.Count))
                        {
                            currentPos++;
                            currentVertex = null;
                            initialVertexes.TryGetValue(Tuple.Create(entry.Stop.Group, currentPos), out currentVertex);
                            if (currentVertex == null)
                                initialVertexes.Add(Tuple.Create(entry.Stop.Group, currentPos), currentVertex = new InitialVertex { Stop = entry.Stop.Group, Position = currentPos });
                        }
                    prevVertex = currentVertex;
                    prevTime = entry.Time;

                    currentVertex.TripCount += triptype.Count;
                    currentStopPosition[entry.Stop.Group] = currentPos + 1;
                }
            }
            foreach (var vertex in initialVertexes.Values)
                vertex.Finalize();
            this.Vertexes = initialVertexes.Values.Cast<Vertex>().ToList();
        }

        private RouteGraphV2(IEnumerable<Vertex> vertexes)
        {
            HashSet<Vertex> vertexesHash = new HashSet<Vertex>(vertexes);
            this.Vertexes = vertexes.Select(v => v.Take(vertexesHash)).ToList();
            foreach (var vertex in this.Vertexes)
                vertex.Reset(this.Vertexes);
        }
        private RouteGraphV2(List<Vertex> vertexes, bool isGraphIsolated)
        {
            this.Vertexes = vertexes;
        }

        public IList<RutePathEntry> CreateLine(bool isGraphConnected = false)
        {
            //ha a gráf nem összefüggő (csak kezdetkor), szétszedem komponensekre és azokra oldom meg a problémát
            if (!isGraphConnected)
            {
                var remComponentVertexes = new HashSet<Vertex>(this.Vertexes);
                var components = new List<RouteGraphV2>();
                while (remComponentVertexes.Any())
                {
                    var component = remComponentVertexes.First().GetComponent().ToList();
                    components.Add(new RouteGraphV2(component, isGraphIsolated: true));
                    remComponentVertexes.ExceptWith(component);
                }
                if (components.Count > 1)
                {
                    return components.OrderByDescending(c => c.Vertexes.Count).SelectMany(c => c.CreateLine(isGraphConnected: true)).ToList();
                }
            }

            //megkeresem a leghosszabb utat, DAC-ra alkalmazott algoritmussal
            var maxLineEnd = Vertexes.MaxBy(v => v.GetMaxDepth());
            var maxLine = new List<Vertex>(new Vertex[] { maxLineEnd });
            while (maxLineEnd.GetMaxDepth() > 0)
            {
                var nextLineEnd = maxLineEnd.ReverseNeighbors.MaxBy(n => n.GetMaxDepth());
                maxLine.Insert(0, nextLineEnd);
                maxLineEnd = nextLineEnd;
            }
            //átalakítom a leghosszabb utat megálló-idő útra
            Vertex prev = null;
            double sumTime = 0.0;
            var vertexToEntry = new Dictionary<Vertex, RutePathEntry>();
            var maxLineWithTime = new List<RutePathEntry>();
            foreach (var vertex in maxLine)
            {
                if (prev != null)
                    sumTime += prev.Neighbors[vertex];
                maxLineWithTime.Add(new RutePathEntry { Stop = vertex.Stop, TimeInMinutes = sumTime });
                vertexToEntry[vertex] = maxLineWithTime.Last();
                prev = vertex;
            }
            //a maradék csúszokat szétszedem komponensekre és megoldom rájuk a problémát, majd beszúróm a kapott utat a megfelelő csatlakozási pontra
            var remVertexes = Vertexes.Except(maxLine).ToList();
            var checkedVertexHash = new HashSet<Vertex>(maxLine);
            while (remVertexes.Count > 0)
            {
                var component = remVertexes.First().GetComponent(checkedVertexHash).ToList();
                remVertexes = remVertexes.Except(component).ToList();
                checkedVertexHash.UnionWith(component);

                RouteGraphV2 subGraph = new RouteGraphV2(component);
                var newLine = subGraph.CreateLine(isGraphConnected: true);
                bool insertDone = false;

                foreach (var lineVertex in maxLine)
                {
                    if (lineVertex.Neighbors.Keys.Intersect(component).Any())
                    {
                        double firstTime = lineVertex.Neighbors[lineVertex.Neighbors.Keys.Intersect(component).MinBy(n => indexInList(n, newLine, int.MaxValue))];
                        var lineEntry = vertexToEntry[lineVertex];
                        foreach (var newLineEntry in newLine)
                            newLineEntry.TimeInMinutes += lineEntry.TimeInMinutes + firstTime;
                        maxLineWithTime.InsertRange(maxLineWithTime.IndexOf(lineEntry) + 1, newLine);
                        insertDone = true;
                        break;
                    }
                    if (lineVertex.ReverseNeighbors.Intersect(component).Any())
                    {
                        double firstTime = lineVertex.ReverseNeighbors.Intersect(component).MaxBy(n => indexInList(n, newLine, int.MinValue)).Neighbors[lineVertex];
                        var lineEntry = vertexToEntry[lineVertex];
                        foreach (var newLineEntry in newLine)
                            newLineEntry.TimeInMinutes += lineEntry.TimeInMinutes - firstTime - newLine.Last().TimeInMinutes;
                        maxLineWithTime.InsertRange(maxLineWithTime.IndexOf(lineEntry), newLine);
                        insertDone = true;
                        break;
                    }
                }
                if (!insertDone)
                    throw new Exception("Subgraph line failed to insert");
            }
            return maxLineWithTime;
        }

        private int indexInList(Vertex v, IList<RutePathEntry> list, int defaultValue)
        {
            int i = 0;
            foreach (var e in list)
            {
                if (e.Stop == v.Stop)
                    return i;
                i++;
            }
            return defaultValue;
        }

        public RouteGraphV2 Filter(double limitInPercentage)
        {
            return new RouteGraphV2(Vertexes.Where(v => v.TripCount >= totalTripCount * limitInPercentage));
        }
    }

    public class RutePathEntry
    {
        public StopGroup Stop;
        public double TimeInMinutes;
    }

    public class GTrip : List<GTrip.Entry>
    {
        public GTrip(IEnumerable<Tuple<TimeSpan, Stop>> trip)
            : base(trip.Select(x => new Entry { Stop = x.Item2, Time = (int)x.Item1.TotalMinutes })) { }

        public class Entry
        {
            public int Time;
            public Stop Stop;
        }
    }
    public class GTripType
    {
        public GTrip Trip;
        public int Count;
    }
}

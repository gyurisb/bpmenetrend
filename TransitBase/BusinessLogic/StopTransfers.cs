using NetPortableServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TransitBase.Entities;

namespace TransitBase.BusinessLogic
{
    public class StopTransfers
    {
        public static readonly double PlanMinimumInMeters = 100.0;
        public static readonly double PlanMaximumInMeters = 1000.0;
        public static Func<double> WalkingSpeed;

        public static StopDistanceResult LastNearestStop { get; private set; }
        private static DateTime nearestStopTime = DateTime.MinValue;
        private static readonly SemaphoreSlim nearestStopLock = new SemaphoreSlim(1);

        private static IDirectionsService directionsService;

        internal StopTransfers(IDirectionsService directionsService)
        {
            StopTransfers.directionsService = directionsService;
        }

        public static async Task<StopDistanceResult> GetNearestStop(GeoCoordinate center)
        {
            await nearestStopLock.WaitAsync();
            if (nearestStopTime < DateTime.Now - TimeSpan.FromMinutes(1))
            {
                LastNearestStop = await calculateNearestStop(center);
                nearestStopTime = DateTime.Now;
            }
            nearestStopLock.Release();
            return LastNearestStop;
        }

        private static async Task<StopDistanceResult> calculateNearestStop(GeoCoordinate center)
        {
            StopDistanceResult result = null;
            if (center != null)
            {
                var stops = TransitBaseComponent.Current.Stops
                    .Where(s => s.StraightLineDistanceTo(center.Latitude, center.Longitude) < 2000)
                    .Select(s => new { Stop = s, Distance = s.StraightLineDistanceTo(center.Latitude, center.Longitude) })
                    .OrderBy(x => x.Distance)
                    .ToList();
                StopDistanceResult prevResult = new StopDistanceResult { DistanceInMeters = double.MaxValue };
                while (stops.Count > 0)
                {
                    var minDistance = stops.First().Distance;
                    var nextStops = stops.TakeWhile(s => s.Distance < minDistance + 200.0).ToList();
                    stops.RemoveRange(0, nextStops.Count);

                    var results = await WalkDistances(center, nextStops.Select(x => x.Stop));

                    var minResult = results.Concat(new StopDistanceResult[] { prevResult }).MinBy(r => r.DistanceInMeters);
                    if (stops.Count == 0 || minResult.DistanceInMeters <= stops.First().Distance)
                    {
                        result = minResult;
                        break;
                    }
                    else
                        prevResult = minResult;
                }
            }
            return result;
        }

        public static async Task<IEnumerable<StopGroupDistanceResult>> NearStops(GeoCoordinate center, double radius)
        {

            if (center == null)
                return new StopGroupDistanceResult[0];
            var stops = TransitBaseComponent.Current.Stops
                .Where(stop => stop.StraightLineDistanceTo(center.Latitude, center.Longitude) <= radius)
                .Select(s => s.Group)
                .Distinct()
                .SelectMany(s => s.Stops);
            var results = await WalkDistances(center, stops);
            return results
                .GroupBy(e => e.Stop.Group)
                .Select(g => new StopGroupDistanceResult(g))
                .Where(ge => ge.AverageDistance <= radius)
                .OrderBy(ge => ge.AverageDistance)
                .ToList();
        }
        public static async Task<IEnumerable<StopDistanceResult>> NearStopsFrom(Stop sourceStop, double radius)
        {
            var stops = TransitBaseComponent.Current.Stops.Where(s => s.StraightLineDistanceTo(sourceStop) <= radius);
            return await WalkDistances(sourceStop, stops);
        }

        public static async Task<Direction> WalkDistance(GeoCoordinate source, GeoCoordinate target)
        {
            if (source == target)
                return new Direction();

            var cachedValue = LookupCache(source, target);
            if (cachedValue != null)
                return cachedValue;

            double straightLineDistance = source.GetDistanceTo(target);
            if (straightLineDistance < PlanMinimumInMeters)
            {
                return defaultWalkDistance(straightLineDistance);
            }

            var res = await directionsService.WalkDistanceAsync(source.Latitude, source.Longitude, target.Latitude, target.Longitude);
            if (res == null)
            {
                res = defaultWalkDistance(straightLineDistance, true);
            }
            AddToCache(source, target, res);
            return res;
        }
        private static Direction defaultWalkDistance(double straightLineDistance, bool slower = false)
        {
            double currentSpeed = WalkingSpeed();
            if (slower) currentSpeed *= 0.75;
            return new Direction
            {
                DistanceInMeters = straightLineDistance,
                EstimatedDuration = TimeSpan.FromSeconds(straightLineDistance / currentSpeed)
            };
        }

        public static async Task<IEnumerable<Direction>> WalkDistances(GeoCoordinate source, IEnumerable<GeoCoordinate> targets)
        {
            return await Task.WhenAll(targets.Select(async t => await WalkDistance(source, t)));
        }
        public static async Task<IEnumerable<StopDistanceResult>> WalkDistances(GeoCoordinate source, IEnumerable<Stop> targets)
        {
            return await Task.WhenAll(targets.Select(async stop =>
            {
                var dist = await WalkDistance(source, stop.Coordinate);
                return new StopDistanceResult
                {
                    Stop = stop,
                    DistanceInMeters = dist.DistanceInMeters,
                    EstimatedDuration = dist.EstimatedDuration,
                };
            }));
        }

        public static async Task<IEnumerable<StopDistanceResult>> WalkDistances(Stop sourceStop, IEnumerable<Stop> targetStops)
        {
            var unknownStops = new List<Stop>();
            var result = new List<StopDistanceResult>();
            double walkingSpeed = WalkingSpeed();

            var sourceTransfers = sourceStop.Transfers.ToArray();
            foreach (Stop currentStop in targetStops)
            {
                if (currentStop.Coordinate == sourceStop.Coordinate)
                    result.Add(new StopDistanceResult
                    {
                        Stop = currentStop,
                        EstimatedDuration = TimeSpan.Zero,
                        DistanceInMeters = 0.0
                    });
                else
                {
                    Transfer matchingTransfer = sourceTransfers.FirstOrDefault(t => t.Target.Coordinate == currentStop.Coordinate);
                    if (matchingTransfer != null)
                    {
                        var entry = new StopDistanceResult
                        {
                            Stop = currentStop,
                            EstimatedDuration = TimeSpan.FromSeconds(matchingTransfer.Distance / walkingSpeed),
                            DistanceInMeters = (double)matchingTransfer.Distance
                        };
                        result.Add(entry);
                        AddToCache(sourceStop.Coordinate, currentStop.Coordinate, new Direction { EstimatedDuration = entry.EstimatedDuration, DistanceInMeters = entry.DistanceInMeters });
                    }
                    else
                        unknownStops.Add(currentStop);
                }
            }
            result.AddRange(await WalkDistances(sourceStop.Coordinate, unknownStops));
            return result;
        }
        public static async Task<StopDistanceResult> WalkDistance(Stop sourceStop, Stop targetStop)
        {
            return (await WalkDistances(sourceStop, new Stop[] { targetStop })).Single();
        }

        private static Dictionary<Tuple<double, double, double, double>, Direction> cache = new Dictionary<Tuple<double, double, double, double>, Direction>();
        private static double walkingSpeedCache = -1;

        private static Direction LookupCache(GeoCoordinate source, GeoCoordinate target)
        {
            Direction route;
            if (cache.TryGetValue(Tuple.Create(source.Latitude, source.Longitude, target.Latitude, target.Longitude), out route))
                return route;
            if (cache.TryGetValue(Tuple.Create(target.Latitude, target.Longitude, source.Latitude, source.Longitude), out route))
                return route;
            return null;
        }
        private static void AddToCache(GeoCoordinate source, GeoCoordinate target, Direction route)
        {
            if (cache.Count > 1000)
                cache.Clear();
            cache[Tuple.Create(source.Latitude, source.Longitude, target.Latitude, target.Longitude)] = route;
        }
    }

    public class StopGroupDistanceResult : List<StopDistanceResult>
    {
        public StopGroup StopGroup;
        public double AverageDistance;
        public double MinDistance { get { return this.Min(e => e.DistanceInMeters); } }
        public double MaxDistance { get { return this.Max(e => e.DistanceInMeters); } }
        public StopGroupDistanceResult() { }
        public StopGroupDistanceResult(IEnumerable<StopDistanceResult> stopEntries)
            : base(stopEntries)
        {
            AverageDistance = this.Average(e => e.DistanceInMeters);
            StopGroup = this.First().Stop.Group;
        }
    }
    public class StopDistanceResult
    {
        public Stop Stop;
        public double DistanceInMeters;
        public TimeSpan EstimatedDuration;
    }
}

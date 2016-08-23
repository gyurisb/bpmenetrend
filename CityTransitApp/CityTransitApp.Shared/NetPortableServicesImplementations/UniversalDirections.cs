using CityTransitApp;
using CityTransitApp.CityTransitElements.Pages;
using NetPortableServices;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TransitBase;
using TransitBase.BusinessLogic;
using TransitBase.Entities;
using Windows.Devices.Geolocation;
using Windows.System;
using Windows.UI.Popups;
#if WINDOWS_PHONE_APP
using Windows.Services.Maps;
using CityTransitApp.CityTransitServices.Tools;
#endif

namespace CityTransitApp.NetPortableServicesImplementations
{
    class UniversalDirections : IDirectionsService
    {
#if WINDOWS_PHONE_APP
        public static List<TimeSpan> QueryTimes = new List<TimeSpan>();

        public static async Task Test(int count = 50)
        {
            var provider = new UniversalDirections();
            var sw = Stopwatch.StartNew();
            var results = await Task.WhenAll(Enumerable.Range(0, count).Select(async i =>
            {
                var swInner = Stopwatch.StartNew();
                var res = await provider.WalkDistanceAsync(
                    47.473525, 19.052782,
                    47.478281 - 0.0033 * i, 19.043815
                    );
                sw.Stop();
                swInner.Stop();
                return Tuple.Create(swInner.Elapsed, res);
            }));
            results = results.Where(r => r != null).ToArray();
            //var results = new List<Tuple<TimeSpan, RouteResult>>();
            //for (int i = 0; i < count; i++)
            //{
            //    var swInner = Stopwatch.StartNew();
            //    var res = await provider.QueryWalkDistance(
            //        new GeoCoordinate(47.473525, 19.052782),
            //        new GeoCoordinate(47.478281 - 0.0033 * i, 19.043815)
            //        );
            //    sw.Stop();
            //    swInner.Stop();
            //    results.Add(Tuple.Create(swInner.Elapsed, res));
            //}
            //results = results.Where(r => r != null).ToList();

            var average = TimeSpan.FromMilliseconds(results.Average(x => x.Item1.TotalMilliseconds));
            var total = sw.Elapsed;

            await new MessageDialog(total + "\navg: " + average + "\ndist: " + results.First().Item2.DistanceInMeters + "\ntime: " + results.First().Item2.EstimatedDuration).ShowAsync();
        }

        private async Task<NetPortableServices.Direction> queryWalkDistance(double sourceLat, double sourceLon, double targetLat, double targetLon)
        {
            try
            {
                var result = await Windows.Services.Maps.MapRouteFinder.GetWalkingRouteFromWaypointsAsync(
                    new Geopoint[]{
                        new Geopoint(new BasicGeoposition { Latitude = sourceLat, Longitude = sourceLon }),
                        new Geopoint(new BasicGeoposition { Latitude = targetLat, Longitude = targetLon })
                });
                if (result.Status == Windows.Services.Maps.MapRouteFinderStatus.Success)
                {
                    return new NetPortableServices.Direction
                    {
                        DistanceInMeters = result.Route.LengthInMeters,
                        EstimatedDuration = result.Route.EstimatedDuration
                    };
                }
            }
            catch (Exception) { }
            return null;
        }
        private async Task<NetPortableServices.Direction> queryTimeOut()
        {
            await Task.Delay(10000);
            return null;
        }
#endif

        private Task<T> WhenAny<T>(params Task<T>[] tasks)
        {
            bool isStarted = false;
            T res = default(T);
            Task<T> task = new Task<T>(() => res);

            Task.WhenAll(tasks.Select(async currentTask =>
            {
                T curRes = await currentTask;
                if (!isStarted)
                {
                    isStarted = true;
                    res = curRes;
                    task.Start();
                }
            }));
            return task;
        }

        public async Task<NetPortableServices.Direction> WalkDistanceAsync(double sourceLat, double sourceLon, double targetLat, double targetLon)
        {
#if WINDOWS_PHONE_APP
            TimeSpan starTime = DateTime.Now.TimeOfDay;
            var sw = Stopwatch.StartNew();
            var retVal = await WhenAny(
                queryTimeOut(),
                queryWalkDistance(sourceLat, sourceLon, targetLat, targetLon)
            );
            sw.Stop();
            QueryTimes.Add(sw.Elapsed);
            PerfLogging.AddRow("walkQuery", sourceLat, sourceLon, targetLat, targetLon, starTime, sw.Elapsed, retVal != null ? retVal.DistanceInMeters : -1, retVal != null ? retVal.EstimatedDuration : TimeSpan.Zero);
            return retVal;
#endif
            return null;
        }

        public Task<NetPortableServices.Direction> WalkDistanceAsync(NetPortableServices.Geoposition source, NetPortableServices.Geoposition target)
        {
            return WalkDistanceAsync(source.Latitude, source.Longitude, target.Latitude, target.Longitude);
        }
    }
}

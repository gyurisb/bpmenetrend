using Microsoft.Phone.Maps.Services;
using NetPortableServices;
using System;
using System.Collections.Generic;
using System.Device.Location;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CityTransitApp.WPSilverlight.NetPortableServicesImplementations
{
    class SilverlightDirections : IDirectionsService
    {
        public async Task<Direction> WalkDistanceAsync(double sourceLat, double sourceLon, double targetLat, double targetLon)
        {
            Task completeTask = new Task(() => { });
            double length = -1;
            TimeSpan time;

            var query = new RouteQuery
            {
                TravelMode = TravelMode.Walking,
                Waypoints = new GeoCoordinate[] { new GeoCoordinate(sourceLat, sourceLon), new GeoCoordinate(targetLat, targetLon) }
            };
            query.QueryCompleted += delegate(object sender, QueryCompletedEventArgs<Microsoft.Phone.Maps.Services.Route> e)
            {
                if (e.Error == null)
                {
                    var resultRoute = e.Result;
                    length = resultRoute.LengthInMeters;
                    time = resultRoute.EstimatedDuration;
                    //MapRoute MyMapRoute = new MapRoute(resultRoute);
                    query.Dispose();
                }
                completeTask.Start();
            };
            query.QueryAsync();

            await completeTask;
            if (length == -1)
            {
                return null;
            }
            else
            {
                var res = new Direction { DistanceInMeters = length, EstimatedDuration = time };
                return res;
            }
        }

        public async Task<Direction> WalkDistanceAsync(Geoposition source, Geoposition target)
        {
            return await WalkDistanceAsync(source.Latitude, source.Longitude, target.Latitude, target.Longitude);
        }
    }
}

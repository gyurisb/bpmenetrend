using NetPortableServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Geolocation;

namespace CityTransitApp.NetPortableServicesImplementations
{
    class UniversalLocation : ILocationService
    {
        public async Task<NetPortableServices.Geoposition> GetGeopositionAsync(TimeSpan maximumAge, TimeSpan timeout)
        {
            try
            {
                Geolocator geolocator = new Geolocator();
                geolocator.DesiredAccuracyInMeters = 50;
                var result = await geolocator.GetGeopositionAsync(
                    maximumAge: TimeSpan.FromMinutes(1),
                    timeout: TimeSpan.FromSeconds(10)
                );
                if (result == null || result.Coordinate == null)
                    return null;
                return new NetPortableServices.Geoposition
                {
                    Latitude = result.Coordinate.Latitude,
                    Longitude = result.Coordinate.Longitude
                };
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}

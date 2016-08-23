using NetPortableServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CityTransitApp.WPSilverlight.NetPortableServicesImplementations
{
    class SilverlightLocation : ILocationService
    {
        public async Task<NetPortableServices.Geoposition> GetGeopositionAsync(TimeSpan maximumAge, TimeSpan timeout)
        {
            try
            {
                var geolocator = new Windows.Devices.Geolocation.Geolocator();
                geolocator.DesiredAccuracyInMeters = 50;

                var geoposition = await geolocator.GetGeopositionAsync(maximumAge, timeout);
                if (geoposition == null || geoposition.Coordinate == null)
                    return null;
                return new NetPortableServices.Geoposition { Latitude = geoposition.Coordinate.Latitude, Longitude = geoposition.Coordinate.Longitude };
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}

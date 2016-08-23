using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using TransitBase;
using CityTransitApp;
using CityTransitApp.Common.ViewModels.Settings;
using CityTransitApp.Common;

namespace CityTransitServices.Tools
{
    //forrás: http://msdn.microsoft.com/en-us/library/windowsphone/develop/jj206956(v=vs.105).aspx
    public static class CurrentLocation
    {
        private static DateTime lastResponse = DateTime.MinValue;
        public static GeoCoordinate Last { get; private set; }

        public static async Task<GeoCoordinate> Get(bool force = false, bool upToDate = false)
        {
            if (SettingsModel.LocationServices != true && !force)
            {
                // The user has opted out of Location.
                return null;
            }

            if (DateTime.Now - lastResponse < TimeSpan.FromMinutes(1) && !upToDate)
            {
                // There was a location response in the last minute, return that value
                return Last;
            }

            try
            {
                //lastResponse = DateTime.Now;
                //Last = new GeoCoordinate(40.755152, -73.986934);
                //Last = new GeoCoordinate(47.586360, 19.004620);
                //return Last;
                var geoposition = await CommonComponent.Current.Services.Location.GetGeopositionAsync(
                    maximumAge: TimeSpan.FromMinutes(1),
                    timeout: TimeSpan.FromSeconds(10)
                );
                if (geoposition == null)
                    return null;

                lastResponse = DateTime.Now;
                Last = Convert(geoposition);
                return Last;
            }
            catch (Exception)
            {
                return null;
            }
        }

        private static GeoCoordinate Convert(NetPortableServices.Geoposition position)
        {
            return new GeoCoordinate(position.Latitude, position.Longitude);
        }
    }
}

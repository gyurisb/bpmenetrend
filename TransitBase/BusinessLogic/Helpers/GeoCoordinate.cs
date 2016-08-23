using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransitBase
{
    public class GeoCoordinate
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }

        public GeoCoordinate() { }
        public GeoCoordinate(double latitude, double longitude)
        {
            this.Latitude = latitude;
            this.Longitude = longitude;
        }

        public double GetDistanceTo(GeoCoordinate other)
        {
            return Distance(this, other) * 1000.0;
        }

        public override bool Equals(object obj)
        {
            var coord = obj as GeoCoordinate;
            if (coord == null)
                return false;
            return coord.Latitude == Latitude && coord.Longitude == Longitude;
        }

        public override int GetHashCode()
        {
            return 31 * Latitude.GetHashCode() + Longitude.GetHashCode();
        }

        public static bool operator ==(GeoCoordinate a, GeoCoordinate b)
        {
            if ((object)a == null) return (object)b == null;
            if ((object)b == null) return false;

            return a.Latitude == b.Latitude && a.Longitude == b.Longitude;
        }
        public static bool operator !=(GeoCoordinate a, GeoCoordinate b)
        {
            return !(a == b);
        }

        /// <summary>  
        /// Returns the distance in miles or kilometers of any two  
        /// latitude / longitude points.  
        /// </summary>  
        private static double Distance(GeoCoordinate pos1, GeoCoordinate pos2, bool inMile = false)
        {
            double R = inMile ? 3960 : 6371;
            double dLat = toRadian(pos2.Latitude - pos1.Latitude);
            double dLon = toRadian(pos2.Longitude - pos1.Longitude);
            double a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(toRadian(pos1.Latitude)) * Math.Cos(toRadian(pos2.Latitude)) *
                Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
            double c = 2 * Math.Asin(Math.Min(1, Math.Sqrt(a)));
            double d = R * c;
            return d;
        }
        /// <summary>  
        /// Convert to Radians.  
        /// </summary>  
        private static double toRadian(double val)
        {
            return (Math.PI / 180) * val;
        }  
    }
}

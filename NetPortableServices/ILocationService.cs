using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetPortableServices
{
    public interface ILocationService
    {
        Task<Geoposition> GetGeopositionAsync(TimeSpan maximumAge, TimeSpan timeout);
    }

    public class Geoposition
    {
        public double Latitude;
        public double Longitude;
    }
}

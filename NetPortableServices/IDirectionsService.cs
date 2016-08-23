using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetPortableServices
{
    public interface IDirectionsService
    {
        Task<Direction> WalkDistanceAsync(double sourceLat, double sourceLon, double targetLat, double targetLon);
        Task<Direction> WalkDistanceAsync(Geoposition source, Geoposition target);
    }

    public class Direction
    {
        public double DistanceInMeters;
        public TimeSpan EstimatedDuration;
        public IList<Geoposition> InnerPoints;
    }
}

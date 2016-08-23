using FastDatabaseLoader;
using FastDatabaseLoader.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransitBase.Entities
{
    [Table]
    public class StopGroup : Entity
    {

        [Column]
        public string Name { get; set; }

        [MultiReference(Real = true)]
        public IEnumerable<Stop> Stops { get; set; }

        public double Distance(double latitude, double longitude)
        {
            return Stops.Average(s => s.StraightLineDistanceTo(latitude, longitude));
        }
        public double Distance(StopGroup other)
        {
            return Stops.Join(other.Stops, s => 0, s => 0, (s1, s2) => s1.StraightLineDistanceTo(s2)).Min();
        }
        public Cardinal Direction(double latitude, double longitude)
        {
            double angle = Stops.Average(s => s.DirectionAngleTo(latitude, longitude));
            return Stop.ConvertToCardinal(angle);
        }
        public Cardinal InverseDirection(double latitude, double longitude)
        {
            double angle = Stops.Average(s => s.DirectionAngleTo(latitude, longitude));
            angle +=  angle >= 0 ? -Math.PI : Math.PI;
            return Stop.ConvertToCardinal(angle);
        }


        public StopGroup OppositeOn(Route route)
        {
            return route.Stops.MinBy(s => Distance(s));
        }

        public IEnumerable<Route> Routes
        {
            get
            {
                return Stops.SelectMany(s => s.Routes).Distinct();
            }
        }

        public IEnumerable<RouteGroup> RouteGroups
        {
            get
            {
                return Stops.SelectMany(s => s.Routes.Select(r => r.RouteGroup)).Distinct();
            }
        }

        public IEnumerable<Transfer> Transfers(int radius)
        {
            return Stops
                .SelectMany(s => s.Transfers)
                .Where(t => t.Distance <= radius);
        }
        public IEnumerable<StopGroup> TransferTargets(int radius)
        {
            return Stops
                .SelectMany(s => s.Transfers)
                .Where(t => t.Distance <= radius)
                .GroupBy(t => t.Target.Group)
                .Select(t => t.Key)
                .Except(new StopGroup[] { this });
        }
    }
}

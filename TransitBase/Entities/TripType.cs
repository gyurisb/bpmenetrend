using FastDatabaseLoader;
using FastDatabaseLoader.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransitBase.Entities
{
    [Table(BigTable = 1)]
    public class TripType : Entity
    {
        [Column]
        public string Name { get; set; }

        [ForeignKey]
        public Route Route { get; set; }

        [ForeignKey]
        public Shape Shape { get; set; }

        [MultiReference(Real = true)]
        public IList<TripTimeType> TripTimeTypes { get; set; }

        [MultiReference(Real = true)]
        public IList<StopEntry> StopEntries { get; set; }

        //[MultiReference(Real = true)]
        public IList<TripTypeHeadsign> HeadsignEntries { get; set; }

        public IEnumerable<Trip> Trips
        {
            get
            {
                return TripTimeTypes.SelectMany(t3 => t3.Trips).OrderBy(t => t.StartTime);
            }
        }

        public IEnumerable<Stop> Stops
        {
            get
            {
                return StopEntries.Select(x => x.Stop);
            }
        }
    }

    [Table(BigTable = 2)]
    public class TripTypeHeadsign : Entity
    {
        [Column(OrderBy = true)]
        public short StartIndex { get; set; }

        [Column]
        public string Headsign { get; set; }

        [ForeignKey(Hidden = true)]
        public TripType TripType { get; set; }
    }
}

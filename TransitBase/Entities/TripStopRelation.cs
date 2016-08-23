using FastDatabaseLoader;
using FastDatabaseLoader.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransitBase.Entities
{

    [Table(BigTable = 3)]
    public class StopEntry : Entity
    {
        [ForeignKey]
        public Stop Stop { get; set; }

        [ForeignKey(Hidden = true)]
        public TripType TripType { get; set; }
    }

    [Table(BigTable = 3)]
    public class TimeEntry : Entity
    {
        [Column]
        public short Time { get; set; }

        [ForeignKey(Hidden = true)]
        public TripTimeType TripTimeType { get; set; }
    }

    [Table(BigTable = 3)]
    public class TTEntry : Entity
    {
        [Column]
        public int Position { get; set; }

        [ForeignKey]
        public TripType TripType { get; set; }

        [ForeignKey(Hidden = true)]
        public Stop Stop { get; set; }
    }

    [Table(BigTable = 2)]
    public class TripTimeType : Entity
    {
        [ForeignKey(Hidden = true, PostSet = true)]
        public TripType TripType { get; set; }

        [MultiReference(Real = true)]
        public IList<TimeEntry> TimeEntries { get; set; }

        [MultiReference(Real = true)]
        public IList<Trip> Trips { get; set; }

        public Route Route { get { return TripType.Route; } }
        public IEnumerable<Tuple<TimeSpan, Stop>> Stops
        {
            get
            {
                return TimeEntries.Zip(TripType.StopEntries, (te, se) => Tuple.Create(TimeSpan.FromMinutes(te.Time), se.Stop));
            }
        }
    }
}

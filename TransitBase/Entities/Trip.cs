using FastDatabaseLoader;
using FastDatabaseLoader.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransitBase.Entities
{
    [Table(BigTable = 2)]
    public class Trip : Entity
    {
        [Column]
        public Tuple<bool?, TimeSpan> WheelchairAndTime
        {
            get { return Tuple.Create(WheelchairAccessible, StartTime); }
            set { WheelchairAccessible = value.Item1; StartTime = value.Item2; }
        }

        [HiddenColumn]
        public bool? WheelchairAccessible { get; set; }

        [HiddenColumn(OrderBy = true)]
        public TimeSpan StartTime { get; set; }

        [ForeignKey(Hidden = true, PostSet = true)]
        public TripTimeType TripTimeType { get; set; }

        [ForeignKey]
        public Service Service { get; set; }

        public TripType TripType { get { return TripTimeType.TripType; } }
        public Route Route { get { return TripTimeType.TripType.Route; } }

        public string GetNameAt(StopGroup stop, DateTime time)
        {
            if (TripType.HeadsignEntries != null && TripType.HeadsignEntries.Any())
            {
                int index = IndexAt(stop, time);
                string lastName = TripType.Name;
                foreach (var entry in TripType.HeadsignEntries)
                {
                    if (entry.StartIndex > index)
                        break;
                    lastName = entry.Headsign;
                }
                return lastName;
            }
            else
            {
                return TripType.Name;
            }
        }

        public Tuple<TimeSpan, Stop>[] Stops
        {
            get
            {
                return TripTimeType.Stops.Select(t => Tuple.Create(StartTime + t.Item1, t.Item2)).ToArray();
            }
        }

        public int IndexAt(StopGroup stop, DateTime dateTime)
        {
            return Stops.IndexOfMin(x => x.Item2.Group == stop ? dateTime.NextDateTimeAt(x.Item1) : DateTime.MaxValue);
        }
    }
}

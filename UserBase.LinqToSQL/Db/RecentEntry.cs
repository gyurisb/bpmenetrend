using System;
using System.Collections.Generic;
using System.Data.Linq.Mapping;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TransitBase;
using TransitBase.Entities;
using UserBase.Interface;

namespace UserBase.LinqToSQL
{
    [Table]
    public class RecentEntry : IRecentEntry
    {
        [Column(DbType = "INT NOT NULL IDENTITY", IsDbGenerated = true, IsPrimaryKey = true)]
        public int ID;

        [Column]
        public int StopID;

        [Column]
        public int RouteID;

        [Column]
        public DateTime CreationTime { get; set; }

        public RecentEntry()
        {
            CreationTime = DateTime.Now;
        }

        public Route Route
        {
            get
            {
                if (RouteID == 0) return null;
                return TransitBaseComponent.Current.Logic.GetRouteByID(RouteID);
            }
            set
            {
                if (value == null) RouteID = 0;
                else RouteID = value.ID;
            }
        }

        public StopGroup Stop
        {
            get
            {
                if (StopID == 0) return null;
                return TransitBaseComponent.Current.Logic.GetStopGroupByID(StopID);
            }
            set
            {
                if (value == null) StopID = 0;
                else StopID = value.ID;
            }
        }
    }
}

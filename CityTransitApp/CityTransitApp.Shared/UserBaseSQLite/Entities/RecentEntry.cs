using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TransitBase;
using TransitBase.Entities;
using UserBase.Interface;

namespace UserBase.Entities
{
    public class RecentEntry : IRecentEntry
    {
        [PrimaryKey, AutoIncrement]
        public int ID { get; set; }
        public int StopID { get; set; }
        public int RouteID { get; set; }
        public DateTime CreationTime { get; set; }

        public RecentEntry()
        {
            CreationTime = DateTime.Now;
        }

        [Ignore]
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

        [Ignore]
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

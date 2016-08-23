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
    public class PlanningHistoryEntry : CountableHistoryEntry, IPlanningHistoryEntry
    {
        [PrimaryKey, AutoIncrement]
        public int ID { get; set; }
        public int StopID { get; set; }
        public bool IsSource { get; set; }
        public int Count { get; set; }
        
        public PlanningHistoryEntry(){}
        public PlanningHistoryEntry(DateTime time) : base(time) { }

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
        [Ignore]
        public Route Route { get { return null; } set { } }

        [Ignore]
        protected override int CountProperty
        {
            get { return Count; }
            set { Count = value; }
        }
    }
}

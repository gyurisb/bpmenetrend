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
    public class PlanningHistoryEntry : CountableHistoryEntry, IPlanningHistoryEntry
    {
        [Column(DbType = "INT NOT NULL IDENTITY", IsDbGenerated = true, IsPrimaryKey = true)]
        public int ID;
        [Column]
        public int StopID;
        [Column]
        public bool IsSource { get; set; }
        [Column]
        public int Count;
        
        public PlanningHistoryEntry(){}
        public PlanningHistoryEntry(DateTime time) : base(time) { }

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
        public Route Route { get { return null; } set { } }

        protected override int CountProperty
        {
            get { return Count; }
            set { Count = value; }
        }
    }
}

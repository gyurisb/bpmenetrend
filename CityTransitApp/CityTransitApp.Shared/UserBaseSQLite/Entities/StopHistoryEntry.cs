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
    public class StopHistoryEntry : CountableHistoryEntry, IStopHistoryEntry
    {
        [PrimaryKey, AutoIncrement]
        public int ID { get; set; }
        public int StopID { get; set; }
        public int Count { get; set; }

        public StopHistoryEntry(){}
        public StopHistoryEntry(DateTime time) : base(time) { }

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

        protected override int CountProperty
        {
            get { return Count; }
            set { Count = value; }
        }
    }
}

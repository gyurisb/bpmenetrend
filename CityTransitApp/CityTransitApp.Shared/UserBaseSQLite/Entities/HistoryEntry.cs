using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TransitBase.Entities;
using TransitBase;
using UserBase.Interface;

namespace UserBase.Entities
{
    public class HistoryEntry : CountableHistoryEntry, ITimetableHistoryEntry
    {
        [PrimaryKey, AutoIncrement]
        public int ID { get; set; }
        public int RouteID { get; set; }
        public int StopID { get; set; }
        public int Count { get; set; }

        public override bool Equals(object obj)
        {
            HistoryEntry other = obj as HistoryEntry;
            if (other == null) return false;
            else return ID == other.ID;
        }

        public HistoryEntry() { }
        public HistoryEntry(DateTime time) : base(time) { }

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

        #region legacy
        [Ignore]
        public int TypeIDLegacy
        {
            get { return Count.GetBits(31, 28); }
            set { Count = Count.SetBits(31, 28, value); }
        }
        [Ignore]
        public HistoryEntryTypeLegacy TypeLegacy
        {
            get
            {
                if (RouteID > 0) return HistoryEntryTypeLegacy.Timetable;
                switch (TypeIDLegacy)
                {
                    case 1: return HistoryEntryTypeLegacy.Stop;
                    case 2: return HistoryEntryTypeLegacy.SourceTarget;
                }
                return HistoryEntryTypeLegacy.None;
            }
        }
        public enum HistoryEntryTypeLegacy { Timetable = 0, Stop = 1, SourceTarget = 2, None }
        #endregion

        [Ignore]
        protected override int CountProperty
        {
            get { return Count; }
            set { Count = value; }
        }
    }

}

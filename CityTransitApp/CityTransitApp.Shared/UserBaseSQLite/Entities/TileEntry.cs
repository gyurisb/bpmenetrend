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
    public class TileEntry : IRouteStopPair
    {
        [PrimaryKey, AutoIncrement]
        public int EntryID { get; set; }
        public int RouteID { get; set; }
        public int StopID { get; set; }

        [Ignore]
        public Route Route
        {
            get { return TransitBaseComponent.Current.Logic.GetRouteByID(RouteID); }
            set { RouteID = value.ID; }
        }

        [Ignore]
        public StopGroup Stop
        {
            get { return TransitBaseComponent.Current.Logic.GetStopGroupByID(StopID); }
            set { StopID = value.ID; }
        }

        public override bool Equals(object obj)
        {
            TileEntry other = obj as TileEntry;
            if (other == null) return false;
            else return EntryID == other.EntryID;
        }
    }
}

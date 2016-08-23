using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TransitBase;
using TransitBase.Entities;
using UserBase.Interface;

namespace UserBase.Entities
{
    public class FavoriteEntry : IFavoriteEntry
    {
        [PrimaryKey, AutoIncrement]
        public int EntryID { get; set; }
        public int RouteID { get; set; }
        public int StopID { get; set; }
        public int? Position { get; set; }

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
            FavoriteEntry other = obj as FavoriteEntry;
            if (other == null) return false;
            else return EntryID == other.EntryID;
        }
    }
}

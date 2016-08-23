using System;
using System.Collections.Generic;
using System.Data.Linq.Mapping;
using System.Linq;
using System.Text;
using TransitBase;
using TransitBase.Entities;
using UserBase.Interface;

namespace UserBase.LinqToSQL
{
    [Table]
    public class FavoriteEntry : IFavoriteEntry
    {
        [Column(DbType = "INT NOT NULL IDENTITY", IsDbGenerated = true, IsPrimaryKey = true)]
        public int EntryID;
        [Column]
        public int RouteID;
        [Column]
        public int StopID;
        [Column]
        public int? Position { get; set; }

        public Route Route
        {
            get { return TransitBaseComponent.Current.Logic.GetRouteByID(RouteID); }
            set { RouteID = value.ID; }
        }

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

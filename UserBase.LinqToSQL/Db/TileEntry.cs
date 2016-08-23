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
    public class TileEntry : IRouteStopPair
    {
        [Column(DbType = "INT NOT NULL IDENTITY", IsDbGenerated = true, IsPrimaryKey = true)]
        public int EntryID;
        [Column]
        public int RouteID;
        [Column]
        public int StopID;

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
            TileEntry other = obj as TileEntry;
            if (other == null) return false;
            else return EntryID == other.EntryID;
        }
    }
}

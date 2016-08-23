using FastDatabaseLoader;
using FastDatabaseLoader.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransitBase.Entities
{
    [Table]
    public class Shape : Entity
    {
        [Column]
        public double Latitude { get; set; }
        [Column]
        public double Longitude { get; set; }
        [MultiReference(Real = true)]
        public IList<ShapePoint> Points { get; set; }
    }

    [Table(BigTable = 3)]
    public class ShapePoint : Entity
    {
        [Column]
        public short DLat { get; set; }
        [Column]
        public short DLon { get; set; }
        [HiddenColumn(OrderBy = true)]
        public int Index { get; set; }
        [ForeignKey(Hidden = true, PostSet = true)]
        public Shape Shape { get; set; }

        public double Latitude
        {
            get { return Shape.Latitude + DLat / 10000.0; }
        }
        public double Longitude
        {
            get { return Shape.Longitude + DLon / 10000.0; }
        }
    }
}

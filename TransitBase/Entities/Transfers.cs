using FastDatabaseLoader;
using FastDatabaseLoader.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransitBase.Entities
{
    [Table(BigTable = 3)]
    public class Transfer : Entity
    {
        [Column(OrderBy = true)]
        public short Distance { get; set; }

        [ForeignKey(Hidden = true, PostSet = true)]
        public Stop Origin { get; set; }

        [ForeignKey]
        public Stop Target { get; set; }

        [MultiReference(Real = true)]
        public IList<TransferPoint> InnerPoints { get; set; }
    }

    [Table(BigTable = 3)]
    public class TransferPoint : Entity
    {
        [Column]
        public short DLat { get; set; }
        [Column]
        public short DLon { get; set; }

        [ForeignKey(Hidden = true, PostSet = true)]
        public Transfer Transfer { get; set; }

        public double Latitude
        {
            get { return Transfer.Origin.Latitude + DLat / 10000.0; }
        }
        public double Longitude
        {
            get { return Transfer.Origin.Longitude + DLat / 10000.0; }
        }
    }
}

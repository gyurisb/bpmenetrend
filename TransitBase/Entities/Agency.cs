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
    public class Agency : Entity
    {
        [Column]
        public string Name { get; set; }
        [Column]
        public string ShortName { get; set; }

        [MultiReference(Real = true)]
        public IList<RouteGroup> Routes { get; set; }
    }
}

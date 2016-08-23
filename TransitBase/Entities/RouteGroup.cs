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
    public partial class RouteGroup : Entity
    {
        //TODO Az azonos nevű routegroup-okat össze kell vonni, azon belül az azonos nevű route-okat
        //TODO a route-groupon belül új mező, ami arra a route-ra mutat ami a hátralevő megállókat tartalmazza
        [Column]
        public string Name { get; set; }

        //private string name;
        //[Column]
        //public string Name
        //{
        //    get
        //    {
        //        if (name == "218")
        //            return "Ürömi";
        //        return name;
        //    }
        //    set { name = value; }
        //}

        [Column]
        public string Description { get; set; }

        [Column]
        public byte Type { get; set; }

        [Column]
        public uint BgColor { get; set; }

        [Column]
        public uint FontColor { get; set; }

        [ForeignKey]
        public Agency Agency { get; set; }

        [MultiReference(Real = true)]
        public IList<Route> Routes { get; set; }


        public Route FirstRoute { get { return Routes.FirstOrDefault(); } }
        public Route SecondRoute { get { return Routes.LastOrDefault(); } }

        public double TypePriority { get { return Type == 0 ? 2.5 : Type; } }

        public object T { get { return TFactory(this); } }

        public static Func<RouteGroup, object> TFactory;
    }
}

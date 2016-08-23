using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TransitBase.Entities;

namespace UserBase.Interface
{
    public interface IRouteStopPair
    {
        Route Route { get; set; }
        StopGroup Stop { get; set; }
    }

    public class RouteStopPair : IRouteStopPair
    {
        public Route Route { get; set; }
        public StopGroup Stop { get; set; }
    }
}

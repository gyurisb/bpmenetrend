using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TransitBase.Entities;

namespace UserBase.Interface
{
    public interface ITileRegister : IEnumerable<IRouteStopPair>
    {
        int Bind(Route route, StopGroup stop);
        IRouteStopPair Get(int id);
        int Get(Route route, StopGroup stop);
        void Update();
    }
}

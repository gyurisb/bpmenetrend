using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TransitBase.Entities;

namespace UserBase.Interface
{
    public interface IFavorites : IEnumerable<IFavoriteEntry>
    {
        void Add(Route route, StopGroup stop);
        void Remove(Route route, StopGroup stop);
        void PushForward(Route route, StopGroup stop);
        void PushBack(Route route, StopGroup stop);
        bool Contains(Route route, StopGroup stop);
    }
}

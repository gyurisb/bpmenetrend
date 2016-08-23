using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TransitBase.Entities;

namespace UserBase.Interface
{
    public interface IFavoriteEntry : IRouteStopPair
    {
        int? Position { get; }
    }
}

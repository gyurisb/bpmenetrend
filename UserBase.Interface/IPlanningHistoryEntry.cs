using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UserBase.Interface
{
    public interface IPlanningHistoryEntry : ICountableHistoryEntry, IRouteStopPair
    {
        bool IsSource { get; }
    }
}

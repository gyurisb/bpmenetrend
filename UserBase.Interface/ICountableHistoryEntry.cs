using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UserBase.Interface
{
    public interface ICountableHistoryEntry
    {
        int DayPart { get; }
        int RawCount { get; }
        bool IsActive(DateTime time);
        double CurrentCount { get; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TransitBase;

namespace UserBase.LinqToSQL
{
    public abstract class CountableHistoryEntry
    {
        protected abstract int CountProperty { get; set; }

        public CountableHistoryEntry() { }
        public CountableHistoryEntry(DateTime time)
        {
            DayPart = time.Hour / 3;
        }

        public int DayPart
        {
            get { return CountProperty.GetBits(27, 24); }
            protected set { CountProperty = CountProperty.SetBits(27, 24, value); }
        }
        public int RawCount
        {
            get { return CountProperty.GetBits(23, 0); }
            set { CountProperty = CountProperty.SetBits(23, 0, value); }
        }
        public bool IsActive(DateTime time)
        {
            return DayPart == time.Hour / 3;
        }

        public double CurrentCount
        {
            get { return (double)RawCount / (1 << History.FloatingBits); }
        }
    }
}

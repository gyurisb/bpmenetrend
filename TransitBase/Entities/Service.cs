using FastDatabaseLoader;
using FastDatabaseLoader.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TransitBase.Entities
{
    [Table]
    public class Service : Entity
    {
        [Column]
        public int IDays { get; set; }

        public bool[] Days
        {
            get
            {
                bool[] array = new bool[7];
                for (int i = 0; i < 7; i++)
                    array[i] = (IDays & (1 << i)) != 0;
                return array;
            }
            set
            {
                IDays = 0;
                for (int i = 0; i < 7; i++)
                    IDays |= value[i] ? 1 << i : 0;
            }
            
        }

        [Column]
        public DateTime startDate { get; set; }
        [Column]
        public DateTime endDate { get; set; }

        [MultiReference(Real = true)]
        public IEnumerable<CalendarException> Exceptions { get; set; }

        private Dictionary<DateTime, bool> cache = new Dictionary<DateTime,bool>();

        public virtual bool IsActive(DateTime dateTime)
        {
            DateTime date = dateTime.Date;
            if (cache.ContainsKey(date)) return cache[date];
            bool ret;

            CalendarException exc = Exceptions.SingleOrDefault(e => e.Date == date);
            if (exc != null)
            {
                if (exc.Type == 1) ret = true;
                else ret = false;
            }
            else ret = Days[(int)date.DayOfWeek] && startDate <= date && endDate >= date;

            cache[date] = ret;
            return ret;
        }

        public static Service CreateEmptyService()
        {
            return new Service
            {
                Days = new bool[7],
                startDate = new DateTime(1900, 1, 1),
                endDate = new DateTime(1900, 1, 1)
            };
        }
    }

    [Table]
    public class CalendarException : Entity
    {
        [Column]
        public DateTime Date { get; set; }
        [Column]
        public int Type { get; set; }

        [ForeignKey(Hidden = true)]
        public Service Service { get; set; }
    }
}

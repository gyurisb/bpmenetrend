using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FastDatabaseLoader.Tables
{
    public interface IAccessTable : ITable
    {
        void Flush();
    }

    public class AccessTable<T> : Table<T>, IAccessTable, IDisposable where T : IEntity, new()
    {
        private EntityReader<T> reader;
        private int count;

        private Dictionary<int, T> cache = new Dictionary<int, T>();
        private bool fullLoaded = false;

        public AccessTable(FastDatabase dbm) : base(dbm) { }

        public override void Init()
        {
            var stream = OpenFileOfTable();
            reader = new EntityReader<T>(stream, DBM);
            count = reader.TotalRecords;
        }

        public override int Count { get { return count; } }

        public override IEntity Get(int i)
        {
            lock (cache)
            {
                if (fullLoaded || cache.Keys.Contains(i))
                {
                    return cache[i];
                }
                else
                {
                    T entity = reader.ReadRecord(i);
                    cache.Add(i, entity);
                    return entity;
                }
            }
        }

        public void Flush()
        {
            cache.Clear();
            fullLoaded = false;
        }

        public override IEnumerator<T> GetEnumerator()
        {
            if (!fullLoaded)
            {
                for (int i = 1; i <= count; i++)
                    Get(i);
                fullLoaded = true;
            }
            return cache.Values.GetEnumerator();
        }

        public void Dispose()
        {
            reader.Dispose();
        }
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FastDatabaseLoader.Tables
{
    class FetchTable<T> : Table<T>, ILoadedTable, IDisposable where T : IEntity, new()
    {
        private EntityReader<T> reader;
        private byte[] data;

        public FetchTable(FastDatabase dbm) : base(dbm) { }
        public override int Count { get { return reader.TotalRecords; } }

        public override IEntity Get(int index)
        {
            lock (reader)
            {
                return reader.ReadRecord(index);
            }
        }
        public override IEnumerator<T> GetEnumerator()
        {
            for (int i = 1; i <= Count; i++)
                yield return (T)Get(i);
        }

        public override void Init()
        {
            var stream = OpenFileOfTable();
            reader = new EntityReader<T>(stream, DBM);
        }
        public void LoadData()
        {
            using (var fileStream = OpenFileOfTable())
            {
                data = new byte[fileStream.Length];
                fileStream.Read(data, 0, data.Length);
            }
            lock (reader)
            {
                reader.Close();
                reader = new EntityReader<T>(new MemoryStream(data), DBM);
            }
            Loaded.Start();
        }
        public void Load()
        {
            //do nothing
        }

        public void Dispose()
        {
            //if (!(reader.Stream is MemoryStream))
            //    throw new NotImplementedException();
            lock (reader)
                reader.Dispose();
        }

        public override byte[] Data { get { return data; } }
    }
}

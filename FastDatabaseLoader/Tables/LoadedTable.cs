using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FastDatabaseLoader.Tables
{
    public interface ILoadedTable : ITable
    {
        void Load();
        void LoadData();
    }

    class LoadedTable<T> : Table<T>, ILoadedTable, IDisposable where T : IEntity, new()
    {
        private byte[] data;
        private T[] array;
        private EntityReader<T> reader;

        public LoadedTable(FastDatabase dbm) : base(dbm) { }

        public override void Init()
        {
            var stream = OpenFileOfTable();
            reader = new EntityReader<T>(stream, DBM);
            array = new T[reader.TotalRecords];
        }

        public override int Count { get { return array.Length; } }

        public override IEntity Get(int index)
        {
            if (array[index - 1] != null)
                return array[index - 1];
            else
            {
                //double check minta: optimális konkurencia-kezelés
                lock (reader)
                {
                    if (array[index - 1] == null)
                    {
                        T entity = reader.ReadRecord(index);
                        array[index - 1] = entity;
                        return entity;
                    }
                    else return array[index - 1];
                }
            }
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
                this.reader.Close();
                this.reader = new EntityReader<T>(new MemoryStream(data), DBM);
            }
        }

        public void Load()
        {
            using (EntityReader<T> curReader = new EntityReader<T>(new MemoryStream(data), DBM))
            {
                for (int i = 1; i <= curReader.TotalRecords; i++)
                {
                    //nincs double check, mert kevés az az eset amikor nem kellene lockolni (1. ág)
                    lock (this.reader)
                    {
                        if (array[i - 1] != null)
                            curReader.SkipRecord();
                        else
                        {
                            array[i - 1] = curReader.ReadRecord();
                        }
                    }
                }
            }
            lock (this.reader)
            {
                this.reader.Close();
                this.reader = null;
                this.data = null; //free unused memory
            }
            Loaded.Start();
        }

        public override byte[] Data { get { return data; } }

        public override IEnumerator<T> GetEnumerator()
        {
            //if (stream == null)
            //    return ((IEnumerable<T>)array).GetEnumerator();
            for (int i = 1; i <= array.Length; i++)
                yield return (T)Get(i);
        }

        public void Dispose()
        {
            if (reader != null)
                lock (reader)
                    reader.Dispose();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FastDatabaseLoader.Tables
{
    class EmptyTable<T> : Table<T>, ILoadedTable, IAccessTable
        where T : IEntity, new()
    {

        public override IEntity Get(int index) { return null; }
        public override void Init() { }
        public override IEnumerator<T> GetEnumerator() { return new List<T>().GetEnumerator(); }
        public override int Count { get { return 0; } }
        public void Load() { }
        public void Flush() { }
        public void LoadData() { }
    }
}

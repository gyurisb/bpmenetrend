using FastDatabaseLoader.Tables;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FastDatabaseLoader.MultiReferenceImpls
{
    class FullScanMultiRefList<T> : IEnumerable<T> where T : IEntity, new()
    {
        private Table<T> table;
        private IEntity target;

        public FullScanMultiRefList(Table<T> table, IEntity target)
        {
            this.table = table;
            this.target = target;
        }

        public IEnumerator<T> GetEnumerator()
        {
            return table.Where(x => referencesTarget(x)).GetEnumerator();
        }
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        private bool referencesTarget(T source)
        {
            return table.Info.HasReference(source, target);
        }

    }
}

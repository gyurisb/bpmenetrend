using FastDatabaseLoader.Tables;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace FastDatabaseLoader.MultiReferenceImpls
{
    public class QuickMultiRefList<T> : IList, IList<T> where T : IEntity, new()
    {
        private Table<T> table;
        private int startIndex, count;
        private IEntity outer;
        private Action<object, object> postSetter = null;

        public QuickMultiRefList(Table<T> table, IEntity outer, int startIndex, int count, Action<object, object> postSetter = null)
        {
            this.outer = outer;
            this.table = table;
            this.startIndex = startIndex;
            this.count = count;
            this.postSetter = postSetter;
        }

        private T Get(int i)
        {
            T entity = table[startIndex + i];
            if (postSetter != null)
                postSetter(entity, outer);
            return entity;
        }

        #region IList<T> implementations

        public IEnumerator<T> GetEnumerator()
        {
            for (int i = 0; i < count; i++)
            {
                yield return Get(i);
            }
        }
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        public int IndexOf(T item)
        {
            return item.ID - startIndex;
        }

        public void Insert(int index, T item)
        {
            throw new NotImplementedException();
        }

        public void RemoveAt(int index)
        {
            throw new NotImplementedException();
        }

        public T this[int index]
        {
            get
            {
                return Get(index);
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public void Add(T item)
        {
            throw new NotImplementedException();
        }

        public void Clear()
        {
            throw new NotImplementedException();
        }

        public bool Contains(T item)
        {
            for (int i = 0; i < count; i++)
            {
                T entity = Get(i);
                if (entity as object == item as object)
                    return true;
            }
            return false;
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            for (int i = 0; i < count; i++)
            {
                array[arrayIndex + i] = Get(i);
            }
        }

        public int Count
        {
            get { return count; }
        }

        public bool IsReadOnly
        {
            get { return true; }
        }

        public bool Remove(T item)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region IList functions

        public int Add(object value)
        {
            Add((T)value);
            return 0;
        }

        public bool Contains(object value)
        {
            return Contains((T)value);
        }

        public int IndexOf(object value)
        {
            return IndexOf((T)value);
        }

        public void Insert(int index, object value)
        {
            throw new NotImplementedException();
        }

        public bool IsFixedSize
        {
            get { return true; }
        }

        public void Remove(object value)
        {
            throw new NotImplementedException();
        }

        object IList.this[int index]
        {
            get
            {
                return Get(index);
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public void CopyTo(Array array, int index)
        {
            throw new NotImplementedException();
        }

        public bool IsSynchronized
        {
            get { return false; }
        }

        public object SyncRoot
        {
            get { return null; }
        }

        #endregion
    }
}

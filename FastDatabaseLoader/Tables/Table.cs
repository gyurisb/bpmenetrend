using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace FastDatabaseLoader.Tables
{
    public interface ITable
    {
        IEntity Get(int index);

        TableInfo Info { get; }
        IEntityFactory Factory { get; set; }

        void Init();
        ITable Clone(FastDatabase dbm);
        void InitClone();
    }

    public abstract class Table<T> : IList<T>, ITable where T : IEntity, new()
    {
        public TableInfo Info { get; private set; }
        protected FastDatabase DBM { get; private set; }

        private EntityFactory<T> factory;
        public IEntityFactory Factory
        {
            get { return factory; }
            set
            {
                factory = (EntityFactory<T>)value;
                //TODO factory-s megoldás
            }
        }

        public Task Loaded { get; protected set; }

        protected Table()
        {
            Loaded = new Task(() => { }); 
        }
        protected Table(FastDatabase dbm)
        {
            Info = new TableInfo(this, dbm);
            DBM = dbm;
            Loaded = new Task(() => { }); 
        }

        protected Stream OpenFileOfTable()
        {
            //return DBM.Root.OpenFile(typeof(T).AssemblyQualifiedName + ".dat");
            return DBM.Root.OpenFile(DBM.Meta.GetFileName<T>());
        }
        public string GetFileName()
        {
            //return Windows.Storage.ApplicationData.Current.LocalFolder.Path + "\\" + typeof(T).AssemblyQualifiedName + ".dat";
            if (DBM == null)
                return null;
            //return Windows.Storage.ApplicationData.Current.LocalFolder.Path + "\\" + DBM.Meta.GetFileName<T>();
            return DBM.Root.Path + "\\" + DBM.Meta.GetFileName<T>();
        }

        public T this[int i]
        {
            get
            {
                return (T)Get(i);
            }
            set { throw new NotImplementedException(); }
        }

        public abstract IEntity Get(int index);
        public abstract void Init();
        public abstract IEnumerator<T> GetEnumerator();

        public virtual ITable Clone(FastDatabase dbm) { throw new NotImplementedException(); }
        public virtual void InitClone() { throw new NotImplementedException(); }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        //List interface functions:

        #region ModifiableListInterface
        public void Insert(int index, T item)
        {
            throw new NotImplementedException();
        }
        public void RemoveAt(int index)
        {
            throw new NotImplementedException();
        }
        public void Add(T item)
        {
            throw new NotImplementedException();
        }
        public void Clear()
        {
            throw new NotImplementedException();
        }
        public bool Remove(T item)
        {
            throw new NotImplementedException();
        }

        public int IndexOf(T item)
        {
            throw new NotImplementedException();
        }
        public bool Contains(T item)
        {
            throw new NotImplementedException();
        }
        #endregion

        public void CopyTo(T[] array, int arrayIndex)
        {
            int i = 0;
            foreach (var elem in this)
                array[arrayIndex + i++] = elem;
        }

        public abstract int Count { get; }

        public bool IsReadOnly
        {
            get { return true; }
        }


        public virtual byte[] Data { get { return null; } }
    }
}

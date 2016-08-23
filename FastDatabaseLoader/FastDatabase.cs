using FastDatabaseLoader.Attributes;
using FastDatabaseLoader.Meta;
using FastDatabaseLoader.Tables;
using NetPortableServices;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace FastDatabaseLoader
{
    //public interface IRootDirectory
    //{
    //    bool FileExists(string path);
    //    Stream OpenFile(string path);
    //}

    public abstract class FastDatabase : IDisposable
    {
        //public delegate void ProgressEventHandler(int progress);
        //public event ProgressEventHandler ProgressChanged;

        internal IDirectoryService Root { get; private set; }
        internal MetaDataInterpreter Meta { get; private set; }
        internal string RootNamespace { get; private set; }

        public string[] Strings { get; private set; }
        private int BigTableLimit = int.MaxValue;

        public FastDatabase(string rootNamespace, IDirectoryService root, int bigTableLimit, bool reserveSize)
        {
            this.RootNamespace = rootNamespace;
            this.Root = root;
            this.BigTableLimit = bigTableLimit;
            Init(reserveSize);
        }

        //public FastDatabase(string rootNamespace, StorageFolder folder, int bigTableLimit, bool reserveSize)
        //{
        //    this.RootNamespace = rootNamespace;
        //    this.Root = new StorageFolderDirectory(folder);
        //    this.BigTableLimit = bigTableLimit;
        //    Init(reserveSize);
        //}

        public void Flush()
        {
            foreach (ITable table in getTables())
            {
                ((IAccessTable)table).Flush();
            }
        }

        private void Init(bool reserveSize)
        {
            //Dispose();
            if (!DatabaseExists) return;

            //sztringeket beolvassuk:
            Strings = initStrings(true);
            //Metaadatokat tároló objektum létrehozása és beolvasása
            Meta = createMeta();

            //minden táblán futtatunk egy konstruktort
            foreach (Type eType in getTableTypes())
            {
                if (!Meta.ContainsTable(eType))
                    constructTable(eType, typeof(EmptyTable<>));
                else
                {
                    Type tableType;
                    if (reserveSize)
                    {
                        tableType = typeof(LoadedTable<>);
                        int bigTable = eType.GetCustomAttributes<Table>(true)[0].BigTable;
                        if (bigTable >= BigTableLimit)
                            tableType = typeof(FetchTable<>);
                    }
                    else
                    {
                        tableType = typeof(AccessTable<>);
                    }
                    constructTable(eType, tableType, this);
                }
            }

            Meta.CreateReaders();

            //táblák inicializálása: (konstruálás után kell, mert egymásra mutatnak)
            foreach (ITable table in getTables())
            {
                var factory = Table.GetFactory(table);
                if (factory != null && Meta.Convert(factory.MetaDefinition) == Meta.GetDefinition(table))
                    table.Factory = factory;
                table.Init();
            }
        }

        public void LoadData()
        {
            foreach (ITable table0 in getTables())
            {
                ILoadedTable table = table0 as ILoadedTable;
                table.LoadData();
            }
        }

        public void Load()
        {
            //rekordok adatainak a beolvasása, entitások betöltése
            foreach (ITable table0 in getTables())
            {
                ILoadedTable table = table0 as ILoadedTable;
                table.Load();
            }
        }

        public ITable GetTable(Type type)
        {
            foreach (var field in this.GetType().GetFields())
            {
                if (field.FieldType.GetInterfaces().Contains(typeof(ITable)) && field.FieldType.GetGenericArguments()[0] == type)
                    return (ITable)field.GetValue(this);
            }
            throw new InvalidOperationException("Table not found of type: " + type.ToString());
        }

        public bool DatabaseExists
        {
            get
            {
                return Root.FileExists("strings.txt");
            }
        }

        public void Dispose()
        {
            foreach (var field in this.GetType().GetFields())
            {
                IDisposable disposable = field.GetValue(this) as IDisposable;
                if (disposable != null)
                    disposable.Dispose();
            }
        }

        private MetaDataInterpreter createMeta()
        {
            Stream stream = Root.FileExists("meta.txt") ? Root.OpenFile("meta.txt") : MetaDataInterpreter.GetDefaultStream();

            using (stream)
                return new MetaDataInterpreter(stream, this);
        }

        private IEnumerable<FieldInfo> tableFields()
        {
            return GetType().GetFields().Where(x => x.FieldType.GetInterfaces().Contains(typeof(ITable)));
        }
        private void constructTable(Type entityType, Type tableType, params object[] args)
        {
            var field = GetType().GetFields().Single(f => f.FieldType.GetGenericArguments()[0] == entityType);
            Type genericTableType = tableType.MakeGenericType(new Type[] { entityType });
            object newValue = Activator.CreateInstance(genericTableType, args);
            field.SetValue(this, newValue);
        }
        private IEnumerable<Type> getTableTypes()
        {
            foreach (var field in this.GetType().GetFields())
                if (field.FieldType.GetInterfaces().Contains(typeof(ITable)))
                    yield return field.FieldType.GetGenericArguments()[0];
        }

        private IEnumerable<ITable> getTables()
        {
            foreach (var field in this.GetType().GetFields())
                if (field.FieldType.GetInterfaces().Contains(typeof(ITable)))
                    yield return (ITable)field.GetValue(this);
        }

        private void createConcreteGenericTables(Type ConcreteType)
        {
            foreach (var field in this.GetType().GetFields())
            {
                if (field.FieldType.GetInterfaces().Contains(typeof(ITable)))
                {
                    Type genericTableType = ConcreteType.MakeGenericType(new Type[] { field.FieldType.GetGenericArguments()[0] });
                    field.SetValue(this, (ITable)Activator.CreateInstance(genericTableType, this));
                }
            }
        }

        private string[] initStrings(bool load)
        {
            string[] strings;
            using (StreamReader reader = new StreamReader(Root.OpenFile("strings.txt")))
            {
                string line;
                line = reader.ReadLine(); //sorok számának beolvasása
                strings = new string[Convert.ToInt32(line)];
                if (load == true)
                {
                    int i = 0;
                    while ((line = reader.ReadLine()) != null)
                        strings[i++] = line;
                }
            }
            return strings;
        }

        #region cloning

        //public StaticDBM Clone()
        //{
        //    StaticDBM dbm = (StaticDBM)Activator.CreateInstance(this.GetType(), Root);
        //    dbm.Strings = Strings;
        //    dbm.Meta = dbm.createMeta();
        //    foreach (var tableField in tableFields())
        //        tableField.SetValue(dbm, (tableField.GetValue(this) as ITable).Clone(dbm));
        //    dbm.Meta.CreateReaders();
        //    foreach (var table in dbm.getTables())
        //        table.InitClone();
        //    return dbm;
        //}
        //public T Assimilate<T>(T entity) where T : IEntity
        //{
        //    return (T)GetTable(typeof(T)).Get(entity.ID);
        //}
        #endregion

        #region file handling
        //private class PathRootDirectory : IRootDirectory
        //{
        //    private string root;
        //    public PathRootDirectory(string root) { this.root = root; }
        //    public bool FileExists(string path)
        //    {
        //        return File.Exists(Path.Combine(root, path));
        //    }
        //    public Stream OpenFile(string path)
        //    {
        //        return File.Open(Path.Combine(root, path), FileMode.Open, FileAccess.Read, FileShare.Read);
        //    }
        //}
        //private class IsolatedStorageRootDirectory : IRootDirectory
        //{
        //    private IsolatedStorageFile root;
        //    public IsolatedStorageRootDirectory(IsolatedStorageFile root) { this.root = root; }
        //    public bool FileExists(string path)
        //    {
        //        return root.FileExists(path);
        //    }
        //    public FileStream OpenFile(string path)
        //    {
        //        return root.OpenFile(path, FileMode.Open, FileAccess.Read, FileShare.Read);
        //    }
        //}
        //private class StorageFolderDirectory : IRootDirectory
        //{
        //    private StorageFolder folder;
        //    public StorageFolderDirectory(StorageFolder folder) { this.folder = folder; }
        //    public bool FileExists(string path)
        //    {
        //        try
        //        {
        //            folder.GetFileAsync(path).AsTask().Wait();
        //            return true;
        //        }
        //        catch { return false; }
        //    }
        //    public Stream OpenFile(string path)
        //    {
        //        var task = folder.OpenStreamForReadAsync(path);
        //        task.Wait();
        //        return task.Result;
        //    }
        //}
        #endregion
    }
}

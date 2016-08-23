using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using FastDatabaseLoader;
using FastDatabaseLoader.Tables;
using FastDatabaseLoader.Attributes;

namespace FastDatabaseSaver
{
    public class StaticDBCreater
    {
        private class Table : List<IEntity>
        {
            private StaticDBCreater outer;

            public Table(Type type, StaticDBCreater outer)
            {
                this.outer = outer;
                this.Type = type;
                //Sorted = true;
                //foreach (var property in type.GetProperties())
                //    foreach (var key in property.GetCustomAttributes(typeof(ForeignKey), true).Cast<ForeignKey>())
                //        if (key.Category)
                //            Sorted = false;
            }

            public bool Sorted { get; private set; }
            private Type Type { get; set; }

            private Table categoryTable = null;
            private bool categoryTableUnset = true;
            public Table CategoryTable
            {
                get
                {
                    if (categoryTableUnset)
                    {
                        Type categoryType = ((IEntity)Activator.CreateInstance(Type)).CategoryType;
                        if (categoryType != null)
                            categoryTable = outer.db[categoryType];
                        categoryTableUnset = false;
                    }
                    return categoryTable;
                }
            }

            public void SortBy()
            {
                if (!Sorted && (CategoryTable == null || CategoryTable.Sorted))
                {
                    var orderByProp = Type.GetProperties().SingleOrDefault(p => columnAttr(p) != null && columnAttr(p).OrderBy);
                    Func<object, object> getPropAccessor = null;
                    if (orderByProp != null)
                        getPropAccessor = TableInfo.BuildGetAccessor(orderByProp.GetGetMethod());
                    bool hasCategory = CategoryTable != null;

                    if (!hasCategory && orderByProp != null)
                    {
                        this.Sort((x, y) => (getPropAccessor(x) as IComparable).CompareTo(getPropAccessor(y) as IComparable));
                    }
                    else if (hasCategory)
                    {
                        List<IEntity>[] lists = Enumerable.Repeat((List<IEntity>)null, CategoryTable.Count)
                            .Select(x => new List<IEntity>())
                            .ToArray();
                        foreach (var e in this)
                            lists[e.Category.ID - 1].Add(e);

                        if (orderByProp != null)
                        {
                            foreach (var list in lists)
                                list.Sort((x, y) => (getPropAccessor(x) as IComparable).CompareTo(getPropAccessor(y) as IComparable));
                        }
                        if (!outer.sources.ContainsKey(CategoryTable.Type))
                            outer.sources.Add(CategoryTable.Type, lists);
                        else
                        {
                            int i = 0;
                            foreach (var list in outer.sources[CategoryTable.Type])
                                list.AddRange(lists[i++]);
                        }
                        Clear();
                        AddRange(lists.SelectMany(x => x));
                    }

                    Sorted = true;
                    complete();
                }
            }

            private IColumnAttribute columnAttr(PropertyInfo info)
            {
                var attr = info.GetCustomAttributes(typeof(Column), true)
                    .Union(info.GetCustomAttributes(typeof(HiddenColumn), true));
                if (attr.Count() == 0) return null;
                else return (IColumnAttribute)attr.Single();
            }

            private void complete()
            {
                int counter = 1;
                foreach (IEntity e in this)
                    e.ID = counter++;
            }

            public void SortBy<TKey, TKey2>(Func<IEntity, TKey> selector, Func<IEntity, TKey2> secondarySelector = null)
                where TKey : IComparable
                where TKey2 : IComparable
            {
                base.Sort((x, y) =>
                    {
                        int ret = selector(x).CompareTo(selector(y));
                        if (secondarySelector != null)
                        {
                            if (ret != 0) return ret;
                            else return secondarySelector(x).CompareTo(secondarySelector(y));
                        }
                        else return ret;
                    });
            }
        }

        private Dictionary<Type, Table> db = new Dictionary<Type, Table>();
        private string path;
        private Func<string, string> stringTransformation;

        private Dictionary<Type, List<IEntity>[]> sources = new Dictionary<Type, List<IEntity>[]>();

        public StaticDBCreater(string path, Func<string, string> stringTransformation)
        {
            this.path = path;
            this.stringTransformation = stringTransformation;
        }

        public List<IEntity> GetTable(Type type) { return db[type]; }
        public IEnumerable<T> GetTable<T>() { return GetTable(typeof(T)).Cast<T>(); }

        public IEnumerable<Type> Tables { get { return db.Keys; } }
        public void RegisterTableType(Type type)
        {
            db.Add(type, new Table(type, this));
        }

        public void AddEntity(IEntity entity)
        {
            if (entity.ID != 0) return;

            Type type = entity.GetType();
            db[type].Add(entity);
        }
        public void RemoveEntity(IEntity entity)
        {
            Type type = entity.GetType();
            db[type].Remove(entity);
        }
        public void RemoveEntityAll<T>(IEnumerable<T> entities) where T : IEntity
        {
            var table = db[typeof(T)];
            foreach (var entity in entities)
                table.Remove(entity);
        }

        public void SaveAll(Action<int> progressChanged)
        {
            progressChanged(0);
            Dictionary<String, int> stringDb = new Dictionary<string, int>();
            int currentStringID = 0;

            while (db.Any(x => !x.Value.Sorted))
                foreach (var table in db.Values)
                    if (!table.Sorted)
                        table.SortBy();

            //sources = db.ToDictionary(
            //    x0 => x0.Key, 
            //    x1 => Enumerable.Repeat((object)null, x1.Value.Count).Select(x => new List<IEntity>()).ToArray()
            //);
            //foreach (var table in db.ToArray())
            //    if (table.Value.First().Category != null)
            //        foreach (var entity in table.Value)
            //            addToSourceList(entity, entity.Category);

            progressChanged(10);
            int last = 1000;
            int total = db.Sum(table => table.Value.Count());
            int current = 0;

            foreach (var table in db)
            {
                using (FileStream stream = File.Create(path + Path.DirectorySeparatorChar + table.Key.AssemblyQualifiedName + ".dat"))
                {
                    stream.Write(BitConverter.GetBytes(table.Value.Count), 0, sizeof(int));
                    foreach (IEntity entity in table.Value)
                    {
                        foreach (var property in table.Key.GetProperties())
                        {
                            object value = null;
                            Type type = null;
                            if (property.GetCustomAttributes(typeof(Column), true).Length > 0)
                            {
                                value = property.GetValue(entity);
                                type = property.PropertyType;
                                if (type == typeof(string))
                                {
                                    if (!stringDb.Keys.Contains(value))
                                        stringDb.Add(value as string, currentStringID++);

                                    stream.Write(BitConverter.GetBytes(stringDb[value as string]), 0, sizeof(int));
                                }
                                else if (type == typeof(double))
                                {
                                    stream.Write(BitConverter.GetBytes((double)value), 0, sizeof(double));
                                }
                                else if (type == typeof(int))
                                {
                                    stream.Write(BitConverter.GetBytes((int)value), 0, sizeof(int));
                                }
                                else if (type == typeof(uint))
                                {
                                    stream.Write(BitConverter.GetBytes((uint)value), 0, sizeof(uint));
                                }
                                else if (type == typeof(short))
                                {
                                    stream.Write(BitConverter.GetBytes((short)value), 0, sizeof(short));
                                }
                                else if (type == typeof(byte))
                                {
                                    stream.WriteByte((byte)value);
                                }
                                //else if (type == typeof(bool?))
                                //{
                                //}
                                //else if (type == typeof(TimeSpan))
                                //{
                                //    TimeSpan tvalue = (TimeSpan)value;
                                //    byte b1 = (byte)(tvalue.Hours + 24 * tvalue.Days);
                                //    byte b2 = (byte)tvalue.Minutes;
                                //    stream.WriteByte(b1);
                                //    stream.WriteByte(b2);
                                //}
                                else if (type == typeof(Tuple<bool?, TimeSpan>))
                                {
                                    var curValue = (Tuple<bool?, TimeSpan>)value;

                                    TimeSpan tvalue = curValue.Item2;
                                    //short s = (short)tvalue.TotalMinutes;
                                    byte b1 = (byte)(tvalue.Hours + 24 * tvalue.Days);
                                    byte b2 = (byte)tvalue.Minutes;

                                    bool? bval = curValue.Item1;
                                    byte highbit = (bval ?? false) ? (byte)0x80 : (byte)0x00;
                                    byte lowbit = bval == null ? (byte)0x40 : (byte)0x00;
                                    b1 |= (byte)(highbit | lowbit);

                                    stream.WriteByte(b1);
                                    stream.WriteByte(b2);
                                }
                                else if (type == typeof(DateTime))
                                {
                                    DateTime dvalue = (DateTime)value;
                                    byte[] yearBytes = BitConverter.GetBytes((short)dvalue.Year);
                                    byte[] bytes = new byte[] { yearBytes[0], yearBytes[1], (byte)dvalue.Month, (byte)dvalue.Day };
                                    stream.Write(bytes, 0, bytes.Length);
                                }
                                else throw new Exception("Column type not supported.");
                            }
                            else if (ForeignKey.Is(property) && !ForeignKey.IsHidden(property))
                            {
                                value = property.GetValue(entity);
                                type = property.PropertyType;
                                IEntity target = (IEntity)value;
                                stream.Write(BitConverter.GetBytes(target.ID), 0, sizeof(int));
                            }
                            else if (MultiReference.IsReal(property))
                            {
                                Type propType = property.PropertyType.GetGenericArguments()[0];
                                var currentSources = sources[table.Key][entity.ID - 1].Where(e => e.GetType() == propType);
                                stream.Write(BitConverter.GetBytes(currentSources.Count() > 0 ? currentSources.Min(s => s.ID) : 0), 0, sizeof(int));
                                stream.Write(BitConverter.GetBytes(currentSources.Count()), 0, sizeof(int));
                            }
                        }
                    }

                    if (last-- == 0)
                    {
                        last = 1000;
                        current += 1001;
                        progressChanged(10 + (int)(current * 100 / total * 0.9));
                    }
                }
            }
            using (StreamWriter stream = new StreamWriter(File.Create(path + Path.DirectorySeparatorChar + "strings.txt")))
            {
                var array = stringDb.ToArray().OrderBy(x => x.Value);

                stream.WriteLine(stringDb.Count);
                foreach (var str in array)
                {
                    stream.WriteLine(stringTransformation(str.Key));
                }
            }
            using (File.Create(path + Path.DirectorySeparatorChar + "database_exists")) ;
        }

        //private void addToSourceList(IEntity entity, IEntity category)
        //{
        //    sources[category.GetType()][category.ID - 1].Add(entity);
        //}
    }
}

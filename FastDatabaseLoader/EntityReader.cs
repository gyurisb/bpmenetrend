using FastDatabaseLoader.Attributes;
using FastDatabaseLoader.Meta;
using FastDatabaseLoader.MultiReferenceImpls;
using FastDatabaseLoader.Tables;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace FastDatabaseLoader
{
    public class EntityReader<T> : IDisposable where T : IEntity, new()
    {
        private byte[] buffer = new byte[20];
        private int curID = 1;

        private int recordLength;
        private FieldReader[] fieldReaders;
        private Action<object, object>[] fieldSetters;
        private TableInfo info;
        private IEnumerable<PropertyInfo> hiddenRefSources;

        public Stream Stream { get; private set; }
        public int TotalRecords { get; private set; }

        internal EntityReader(Stream stream, FastDatabase dbm)
        {
            this.Stream = stream;
            this.info = dbm.GetTable(typeof(T)).Info;
            this.fieldReaders = dbm.Meta[typeof(T)];

            this.recordLength = fieldReaders.Sum(r => r.FieldLength());

            hiddenRefSources = info.ReferenceSources
                .Where(p => !MultiReference.IsReal(p));

            var sourceTypes = info.EntityType.GetProperties()
                .Where(MultiReference.IsReal)
                .Select(MultiReference.SourceType);

            fieldSetters = fieldReaders
                .Select(delegate(FieldReader r)
                {
                    if (r is ColumnReader)
                        return info.BuildColumnSetter(r.Type, r.Name);
                    else if (r is ForeignKeyReader) return info.BuildKeySetter(r.Type, r.Name);
                    else return sourceTypes.Contains(r.Type) ?
                        createSourceSetter(r.Type, info.EntityType)
                        : (Action<object, object>)null;
                })
                .Select(r => r ?? emptySetter)
                .ToArray();

            stream.Read(buffer, 0, sizeof(int));
            TotalRecords = BitConverter.ToInt32(buffer, 0);
        }

        public virtual T ReadRecord(int id = -1)
        {
            if (id != -1)
            {
                Stream.Seek(sizeof(int) + (id - 1) * recordLength, SeekOrigin.Begin);
                curID = id;
            }

            T entity = new T();
            entity.ID = curID++;
            setReferenceSources(entity);
            for (int i = 0; i < fieldReaders.Length; i++)
            {
                object val = fieldReaders[i].ReadField(Stream, entity);
                fieldSetters[i](entity, val);
            }
            return entity;
        }

        public virtual void SkipRecord()
        {
            curID++;
            Stream.Seek(recordLength, SeekOrigin.Current);
        }

        public void Dispose()
        {
            Stream.Dispose();
        }
        public void Close()
        {
            Stream.Close();
        }

        #region Helpers
        private static Action<object, object> createSourceSetter(Type oneSideType, Type manySideType)
        {
            var valueSetMethod = manySideType.GetProperties()
                .Single(p => MultiReference.IsReal(p) && MultiReference.SourceType(p) == oneSideType)
                .SetMethod;
            return TableInfo.BuildSetAccessor(valueSetMethod);
        }

        private void emptySetter(object entity, object data) { }


        protected void setReferenceSources(IEntity entity)
        {
            foreach (var prop in hiddenRefSources)
            {
                Type sourceType = prop.PropertyType.GetGenericArguments()[0];
                object list = Activator.CreateInstance(
                        typeof(FullScanMultiRefList<>).MakeGenericType(new Type[] { sourceType }),
                        new object[] { info.getSourceTable(prop), entity }
                    );
                prop.SetValue(entity, list);
            }
        }
        #endregion
    }
}

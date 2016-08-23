using FastDatabaseLoader.Attributes;
using FastDatabaseLoader.MultiReferenceImpls;
using FastDatabaseLoader.Tables;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FastDatabaseLoader.Meta
{
    class MultiKeyReader<TSource> : FieldReader where TSource : IEntity, new()
    {
        ITable referencedTable;
        private Action<object, object> postSetter = null;

        public MultiKeyReader(FastDatabase db)
        {
            this.db = db;
            referencedTable = db.GetTable(typeof(TSource));
            this.Type = typeof(TSource);

            var postSetProp = typeof(TSource).GetProperties().SingleOrDefault(p => ForeignKey.Is(p) && ForeignKey.IsPostSet(p));
            if (postSetProp != null)
                postSetter = TableInfo.BuildSetAccessor(postSetProp.SetMethod);
        }

        public override object ReadField(System.IO.Stream stream, IEntity obj)
        {
            stream.Read(buffer, 0, sizeof(int));
            int startId = BitConverter.ToInt32(buffer, 0);
            stream.Read(buffer, 0, sizeof(int));
            int size = BitConverter.ToInt32(buffer, 0);

            if (referencedTable != null)
            {
                return new QuickMultiRefList<TSource>((Table<TSource>)referencedTable, obj, startId, size, postSetter);
            }
            else
            {
                return new List<TSource>();
            }
        }
        public override int FieldLength() { return 2*sizeof(int); }
    }
}

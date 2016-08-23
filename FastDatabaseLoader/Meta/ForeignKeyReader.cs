using FastDatabaseLoader.Tables;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FastDatabaseLoader.Meta
{
    class ForeignKeyReader : FieldReader
    {
        public bool Category;
        private ITable referencedTable;

        public ForeignKeyReader(FastDatabase db, Type referencedType)
        {
            this.db = db;
            referencedTable = db.GetTable(referencedType);
            if (referencedTable == null) referencedTable = new EmptyTable<Entity>();
            Type = referencedType;
        }

        public override object ReadField(System.IO.Stream stream, IEntity obj)
        {
            stream.Read(buffer, 0, sizeof(int));
            int id = BitConverter.ToInt32(buffer, 0);
            return referencedTable.Get(id);
        }

        public override int FieldLength() { return sizeof(int); }
    }
}

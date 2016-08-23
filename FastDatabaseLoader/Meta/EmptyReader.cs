using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FastDatabaseLoader.Meta
{
    class EmptyReader : FieldReader
    {
        private int size;

        public EmptyReader(int size)
        {
            Name = "";
            Type = typeof(Entity);
            this.size = size;
        }

        public override object ReadField(System.IO.Stream stream, IEntity obj)
        {
            stream.Seek(size, System.IO.SeekOrigin.Current);
            return null;
        }

        public override int FieldLength()
        {
            return size;
        }
    }
}

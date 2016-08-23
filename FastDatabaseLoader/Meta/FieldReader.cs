using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FastDatabaseLoader.Meta
{
    public abstract class FieldReader
    {
        public string Name;
        public Type Type;
        protected FastDatabase db;

        protected byte[] buffer = new byte[64];

        public abstract object ReadField(Stream stream, IEntity obj);
        public abstract int FieldLength();
    }
}

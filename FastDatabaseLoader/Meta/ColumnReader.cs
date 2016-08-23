using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FastDatabaseLoader.Meta
{
    class ColumnReader : FieldReader
    {
        private Func<Stream, object> currentRead = null;
        private string type;

        public ColumnReader(FastDatabase db, string type)
        {
            this.db = db;
            this.type = type;
            Type = GetType(type);
            currentRead = getCurrentRead(type);
        }

        public override object ReadField(System.IO.Stream stream, IEntity obj)
        {
            return currentRead(stream);
        }

        private Func<Stream, object> getCurrentRead(string type)
        {
            switch (type)
            {
                case "Int16": return readShort;
                case "TimeSpan": return readTime;
                case "Nullable`1<Boolean>": return read2Bit;
                case "String": return readString;
                case "Double": return readDouble;
                case "Int32": return readInt;
                case "Byte": return readByte;
                case "UInt32": return readUInt;
                case "DateTime": return readDate;
                case "Tuple`2<Nullable`1+TimeSpan>": return readTuple2bitTimeSpan;
                default: throw new ArgumentOutOfRangeException();
            }
        }

        private object readTuple2bitTimeSpan(Stream stream)
        {
            stream.Read(buffer, 0, 2);
            TimeSpan tdata = new TimeSpan(buffer[0] & 0x3F, buffer[1], 0);

            bool? bval;
            if ((buffer[0] & 0x40) != 0)
                bval = null;
            else
                bval = (buffer[0] & 0x80) != 0;

            return Tuple.Create(bval, tdata);
        }

        private object readUInt(Stream stream)
        {
            stream.Read(buffer, 0, sizeof(uint));
            uint number = BitConverter.ToUInt32(buffer, 0);
            return number;
        }

        private object read2Bit(Stream stream)
        {
            byte val = (byte)stream.ReadByte();
            stream.Position = stream.Position - 1;
            if ((val & 0x40) != 0)
                return null;
            else
            {
                bool? data = (val & 0x80) != 0;
                return data;
            }
        }

        private object readTime(Stream stream)
        {
            stream.Read(buffer, 0, 2);
            TimeSpan tdata = new TimeSpan(buffer[0] & 0x3F, buffer[1], 0);
            return tdata;
        }

        private object readDate(Stream stream)
        {
            stream.Read(buffer, 0, sizeof(short) + 2 * sizeof(byte));
            short year = BitConverter.ToInt16(buffer, 0);
            DateTime date = new DateTime(year, buffer[2], buffer[3]);
            return date;
        }

        private object readByte(Stream stream)
        {
            byte number = (byte)stream.ReadByte();
            return number;
        }

        private object readInt(Stream stream)
        {
            stream.Read(buffer, 0, sizeof(int));
            int number = BitConverter.ToInt32(buffer, 0);
            return number;
        }

        private object readShort(Stream stream)
        {
            stream.Read(buffer, 0, sizeof(short));
            short number = BitConverter.ToInt16(buffer, 0);
            return number;
        }

        private object readDouble(Stream stream)
        {
            stream.Read(buffer, 0, sizeof(double));
            double number = BitConverter.ToDouble(buffer, 0);
            return number;
        }

        private object readString(Stream stream)
        {
            stream.Read(buffer, 0, sizeof(int));
            int index = BitConverter.ToInt32(buffer, 0);
            return index < db.Strings.Length ? db.Strings[index] : "Error";
        }

        internal static Type GetType(string type)
        {
            switch (type)
            {
                case "TimeSpan": return typeof(TimeSpan);
                case "Nullable`1<Boolean>": return typeof(bool?);
                case "String": return typeof(string);
                case "Double": return typeof(double);
                case "Int32": return typeof(int);
                case "Byte": return typeof(byte);
                case "UInt32": return typeof(uint);
                case "DateTime": return typeof(DateTime);
                case "Int16": return typeof(short);
                case "Tuple`2<Nullable`1+TimeSpan>": return typeof(Tuple<bool?, TimeSpan>);
                default: throw new ArgumentOutOfRangeException();
            }
        }

        public override int FieldLength()
        {
            switch (type)
            {
                case "TimeSpan": return 2*sizeof(byte);
                case "Nullable`1<Boolean>": return 0;
                case "String": return sizeof(int);
                case "Double": return sizeof(double);
                case "Int32": return sizeof(int);
                case "Byte": return sizeof(byte);
                case "UInt32": return sizeof(uint);
                case "DateTime": return sizeof(short) + 2 * sizeof(byte);
                case "Int16": return sizeof(short);
                case "Tuple`2<Nullable`1+TimeSpan>": return 2 * sizeof(byte);
                default: throw new ArgumentOutOfRangeException();
            }
        }
    }
}

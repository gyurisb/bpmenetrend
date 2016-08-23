using FastDatabaseLoader.Attributes;
using FastDatabaseLoader.Tables;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace FastDatabaseLoader.Meta
{
    class MetaDataInterpreter : Dictionary<Type, FieldReader[]>
    {
        private FastDatabase dbm;
        private List<string[]> lines;
        private Type[] types;

        private Dictionary<Type, string> fileNames = new Dictionary<Type, string>();
        internal string GetFileName<T>() { return GetFileName(typeof(T)); }
        internal string GetFileName(Type type)
        {
            string result = null;
            fileNames.TryGetValue(type, out result);
            if (result == null)
                return "null";
            return result + ".dat";
        }

        private Dictionary<Type, string> defs = new Dictionary<Type,string>();
        public string GetDefinition(Type type) { return defs[type]; }
        internal string GetDefinition(ITable table)
        {
            return GetDefinition(table.GetType().GetGenericArguments()[0]);
        }

        public MetaDataInterpreter(Stream fileStream, FastDatabase dbm)
        {
            List<string[]> lines = new List<string[]>();
            using (StreamReader reader = new StreamReader(fileStream))
            {
                string line = null;
                while ((line = reader.ReadLine()) != null)
                    lines.Add(line.Split(';'));
            }

            this.dbm = dbm;
            this.lines = lines;
            this.types = lines.Select(lin => Type.GetType(ConvertNamespace(lin[1])) ?? typeof(object)).ToArray();
            for (int i = 0; i < types.Length; i++)
                if (types[i] != typeof(object))
                    fileNames.Add(types[i], lines[i][1]);

            foreach (Type type in types.Where(t => t != typeof(object)))
                Add(type, new FieldReader[0]);
        }

        public void CreateReaders()
        {
            if (this.lines == null)
                throw new InvalidOperationException();

            int i = 0;
            foreach (string[] lin in lines)
            {
                if (types[i] == typeof(object)) continue;

                defs.Add(types[i], lin[2]);
                string[] entries = lin[2].Split(',');
                var array = this[types[i]] = new FieldReader[entries.Length];
                for (int k = 0; k < entries.Length; k++)
                {
                    string[] parts = entries[k].Split(' ');
                    if (parts[0].StartsWith("ref"))
                    {
                        var referencedType = types[Int32.Parse(parts[0].Remove(0, 3))];
                        if (referencedType == typeof(object)) array[k] = new EmptyReader(sizeof(int));
                        else array[k] = new ForeignKeyReader(dbm, referencedType) { Name = parts[1] };
                    }
                    else if (parts[0].StartsWith("sub="))
                    {
                        //array[k] = new MultiKeyReader(dbm, types[Int32.Parse(parts[0].Remove(0, 4))]);
                        Type referencedType = types[Int32.Parse(parts[0].Remove(0, 4))];
                        if (referencedType == typeof(object)) array[k] = new EmptyReader(2*sizeof(int));
                        else
                        {
                            array[k] = (FieldReader)Activator.CreateInstance(typeof(MultiKeyReader<>).MakeGenericType(referencedType), dbm);
                            try
                            {
                                array[k].Name = types[i].GetProperties()
                                    .SingleOrDefault(p => MultiReference.IsReal(p)
                                        && MultiReference.SourceType(p) == array[k].Type).Name;
                            }
                            catch (Exception e) { array[k].Name = "unknown"; }
                        }
                    }
                    else
                    {
                        array[k] = new ColumnReader(dbm, parts[0]) { Name = parts[1] };
                    }
                }
                i++;
            }
            this.lines = null;
        }

        public bool ContainsTable(Type entityType)
        {
            return ContainsKey(entityType);
        }

        public static Stream GetDefaultStream()
        {
            MemoryStream stream = new MemoryStream();
            StreamWriter writer = new StreamWriter(stream);
            writer.Write(defaultMetaData);
            writer.Flush();
            stream.Position = 0;
            return stream;
        }

        private static string defaultMetaData = @"0;BkvMenetrend.Storage.Stop, BkvEngine, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null;String Name,Double Latitude,Double Longitude,Int32 GroupID,ref1 Group
1;BkvMenetrend.Storage.StopGroup, BkvEngine, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null;String Name
2;BkvMenetrend.Storage.RouteGroup, BkvEngine, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null;String Name,String Description,Byte Type,UInt32 BgColor,UInt32 FontColor
3;BkvMenetrend.Storage.Route, BkvEngine, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null;String Name,Int32 RouteGroupID,ref2 RouteGroup
4;BkvMenetrend.Storage.Service, BkvEngine, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null;Int32 IDays,DateTime startDate,DateTime endDate
5;BkvMenetrend.Storage.CalendarException, BkvEngine, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null;DateTime Date,Int32 Type,ref4 Service
6;BkvMenetrend.Storage.TripType, BkvEngine, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null;ref3 Route
7;BkvMenetrend.Storage.Trip, BkvEngine, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null;TimeSpan StartTime orderby,ref6 TripType,ref4 Service";

        internal string Convert(string def)
        {
            return Regex.Replace(def, @"\[.*\]", match => convert(match.Value));
        }

        private string convert(string value)
        {
            string val = value.Replace("[", "").Replace("]", "");
            int i = types.ToList().IndexOf(Type.GetType(ConvertNamespace(val)));
            return "" + i;
        }

        private string ConvertNamespace(string typeString)
        {
            return dbm.RootNamespace.Replace("%", typeString.Split(',').First().Split('.').Last());
        }
    }
}

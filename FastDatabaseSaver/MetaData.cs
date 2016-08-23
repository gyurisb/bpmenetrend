using FastDatabaseLoader.Attributes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FastDatabaseSaver
{
    public class MetaData
    {
        public static void SerializeDatabase(StaticDBCreater db, Stream outStream)
        {
            int i = 0;
            var typeIndex = db.Tables.ToDictionary(x1 => x1, x2 => i++);

            using (StreamWriter writer = new StreamWriter(outStream))
            {
                foreach (Type table in db.Tables)
                {
                    writer.Write(typeIndex[table] + ";" + table.AssemblyQualifiedName + ";");
                    var members = createMembers(table, typeIndex);
                    writer.Write(String.Join(",", members));
                    writer.WriteLine();
                }
            }
        }

        private static IEnumerable<string> createMembers(Type table, Dictionary<Type, int> typeIndex)
        {
            foreach (var property in table.GetProperties())
            {
                var attrs = property.GetCustomAttributes(true);
                if (attrs.Any(a => a is Column))
                {
                    string orderBy = (attrs.Single(a => a is Column) as Column).OrderBy ? " orderby" : "";
                    string type;
                    if (property.PropertyType.IsGenericType)
                         type = property.PropertyType.Name + "<" + String.Join("+", property.PropertyType.GetGenericArguments().Select(x => x.Name)) + ">";
                    else type = property.PropertyType.Name;
                    yield return type + " " + property.Name + orderBy;
                }
                else if (attrs.Any(a => a is ForeignKey && !(a as ForeignKey).Hidden))
                {
                    yield return "ref" + typeIndex[property.PropertyType] + " " + property.Name;
                }
                else if (MultiReference.IsReal(property))
                {
                    yield return "sub=" + typeIndex[property.PropertyType.GetGenericArguments()[0]];
                }
            }
        }
    }
}

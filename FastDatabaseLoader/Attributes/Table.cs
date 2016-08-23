using FastDatabaseLoader.Tables;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace FastDatabaseLoader.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class Table : Attribute
    {
        public String Version { get; set; }
        public Type Factory { get; set; }
        public int BigTable { get; set; }

        public static IEntityFactory GetFactory(Type entityType)
        {
            var attr = entityType.GetCustomAttributes<FastDatabaseLoader.Attributes.Table>(true);
            Type factType = (attr[0] as FastDatabaseLoader.Attributes.Table).Factory;

            if (factType != null)
                return Activator.CreateInstance(factType) as IEntityFactory;
            return null;
        }
        public static IEntityFactory GetFactory(ITable table)
        {
            return GetFactory(table.GetType().GetGenericArguments()[0]);
        }
    }
}

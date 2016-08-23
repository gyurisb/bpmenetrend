using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace FastDatabaseLoader.Attributes
{
    [AttributeUsage(AttributeTargets.Property)]
    public class ForeignKey : Attribute
    {
        public bool Hidden { get; set; }
        public bool PostSet { get; set; }

        public static bool Is(PropertyInfo prop)
        {
            return prop.GetCustomAttributes<ForeignKey>().Any();
        }
        public static bool IsCategory(PropertyInfo prop)
        {
            if (prop.GetCustomAttributes<ForeignKey>().Any())
            {
                Type type = prop.DeclaringType;
                return prop.PropertyType.GetProperties()
                    .Any(p => MultiReference.IsReal(p) && p.PropertyType.GetGenericArguments()[0] == type);
            }
            return false;
        }
        public static bool IsHidden(PropertyInfo prop)
        {
            return prop.GetCustomAttribute<ForeignKey>().Hidden;
        }
        public static bool IsPostSet(PropertyInfo prop)
        {
            return prop.GetCustomAttribute<ForeignKey>().PostSet;
        }
    }
}

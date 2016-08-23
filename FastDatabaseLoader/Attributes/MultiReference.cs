using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace FastDatabaseLoader.Attributes
{
    [AttributeUsage(AttributeTargets.Property)]
    public class MultiReference : Attribute
    {
        public bool Real { get; set; }

        public MultiReference() { Real = true; }

        public static bool IsReal(PropertyInfo property)
        {
            var attrs = property.GetCustomAttributes<MultiReference>(true);
            if (!attrs.Any()) return false;
            return (attrs.Single() as MultiReference).Real;
        }

        public static Type SourceType(PropertyInfo prop)
        {
            return prop.PropertyType.GetGenericArguments()[0];
        }
    }
}

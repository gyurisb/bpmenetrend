using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace FastDatabaseLoader
{
    internal static class ReflectionExtensions
    {
        internal static PropertyInfo[] GetProperties(this Type type)
        {
            return type.GetTypeInfo().DeclaredProperties.ToArray();
        }

        internal static Type[] GetGenericArguments(this Type type)
        {
            return type.GetTypeInfo().GenericTypeArguments.ToArray();
        }

        internal static TAttr[] GetCustomAttributes<TAttr>(this Type type, bool inherit) where TAttr : System.Attribute
        {
            return type.GetTypeInfo().GetCustomAttributes<TAttr>(inherit).ToArray();
        }

        internal static FieldInfo[] GetFields(this Type type)
        {
            return type.GetTypeInfo().DeclaredFields.ToArray();
        }

        internal static Type[] GetInterfaces(this Type type)
        {
            return type.GetTypeInfo().ImplementedInterfaces.ToArray();
        }

        internal static void Close(this Stream stream)
        {
            stream.Dispose();
        }
    }
}

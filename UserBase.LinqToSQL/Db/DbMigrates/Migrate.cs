using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace UserBase.LinqToSQL
{
    [AttributeUsage(AttributeTargets.Method)]
    public class Migrate : Attribute
    {
        public int InitialVersion { get; set; }
        public int TargetVersion { get; set; }

        public static bool AreVersionsSame(MethodInfo method, int initial, int target)
        {
            return method.GetCustomAttributes<Migrate>().Any(m => m.InitialVersion == initial && m.TargetVersion == target);
        }
        public static bool MigrationEnabled(MethodInfo method, int initialVersion)
        {
            return method.GetCustomAttributes<Migrate>().Any(m => m.InitialVersion == initialVersion);
        }
        public static Migrate Get(MethodInfo method)
        {
            return method.GetCustomAttribute<Migrate>();
        }
    }
}

using FastDatabaseLoader.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FastDatabaseLoader
{
    public interface IEntity
    {
        int ID { get; set; }
        IEntity Category { get; }
        Type CategoryType { get; }
    }

    public class Entity : IEntity
    {
        public int ID { get; set; }

        public IEntity Category
        {
            get
            {
                var prop = GetType().GetProperties().FirstOrDefault(p => ForeignKey.IsCategory(p));
                if (prop == null) return null;
                return (IEntity)prop.GetValue(this);
            }
        }

        public Type CategoryType
        {
            get
            {
                var prop = GetType().GetProperties().FirstOrDefault(p => ForeignKey.IsCategory(p));
                if (prop == null) return null;
                return prop.PropertyType;
            }
        }

        public override int GetHashCode()
        {
            if (ID == 0) return base.GetHashCode();
            return ID;
        }
        public override bool Equals(object obj)
        {
            if (ID == 0) return base.Equals(obj);
            return obj is Entity && (obj as Entity).ID == ID;
        }
        public static bool operator ==(Entity a, Entity b)
        {
            if ((a as object) == null) return (b as object) == null;
            if ((b as object) == null) return false;
            if (a.ID == 0) return (a as object) == b;
            return a.ID == b.ID;
        }
        public static bool operator !=(Entity a, Entity b)
        {
            if ((a as object) == null) return (b as object) != null;
            if ((b as object) == null) return true;
            if (a.ID == 0) return (a as object) != b;
            return a.ID != b.ID;
        }
    }
}

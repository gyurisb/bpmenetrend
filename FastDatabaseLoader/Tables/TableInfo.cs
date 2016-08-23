using FastDatabaseLoader.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace FastDatabaseLoader.Tables
{
    public class TableInfo
    {
        private FastDatabase db;

        public Type EntityType;
        public PropertyInfo[] Columns;
        public PropertyInfo[] Keys;
        public PropertyInfo[] ReferenceSources;

        public Action<object, object> BuildColumnSetter(Type type, string name)
        {
            return buildSetter(type, name, Columns);
        }
        public Action<object, object> BuildKeySetter(Type type, string name)
        {
            return buildSetter(type, name, Keys);
        }
        private static Action<object, object> buildSetter(Type type, string name, PropertyInfo[] source)
        {
            var prop = source.SingleOrDefault(p => p.PropertyType == type && p.Name == name);
            if (prop == null) return null;
            return BuildSetAccessor(prop.SetMethod);
        }

        public TableInfo(ITable table, FastDatabase db)
        {
            this.db = db;
            EntityType = table.GetType().GetGenericArguments()[0];

            var properties = EntityType.GetProperties();
            Columns = properties.Where(p => p.GetCustomAttributes<Column>(true).Any()).ToArray();
            Keys = properties.Where(p => p.GetCustomAttributes<ForeignKey>(true).Any()).ToArray();
            ReferenceSources = properties.Where(p => p.GetCustomAttributes<MultiReference>(true).Any()).ToArray();
        }

        Dictionary<PropertyInfo, ITable> sourceTables = null;
        internal ITable getSourceTable(PropertyInfo prop)
        {
            if (sourceTables == null)
                sourceTables = ReferenceSources.ToDictionary(s => s, s => db.GetTable(s.PropertyType.GetGenericArguments()[0]));
            return sourceTables[prop];
        }

        private Func<object, object>[] keyGetters = null;
        internal bool HasReference(IEntity source, IEntity target)
        {
            if (keyGetters == null)
                keyGetters = Keys.Select(k => BuildGetAccessor(k.GetMethod)).ToArray();
            return keyGetters.Any(getter => getter(source) == target);
        }

        public static Action<object, object> BuildSetAccessor(MethodInfo method)
        {
            var obj = Expression.Parameter(typeof(object), "o");
            var value = Expression.Parameter(typeof(object));

            Expression<Action<object, object>> expr =
                Expression.Lambda<Action<object, object>>(
                    Expression.Call(
                        Expression.Convert(obj, method.DeclaringType),
                        method,
                        Expression.Convert(value, method.GetParameters()[0].ParameterType)),
                    obj, value
                );

            return expr.Compile();
        }

        public static Func<object, object> BuildGetAccessor(MethodInfo method)
        {
            var obj = Expression.Parameter(typeof(object), "o");

            Expression<Func<object, object>> expr =
                Expression.Lambda<Func<object, object>>(
                    Expression.Convert(
                        Expression.Call(
                            Expression.Convert(obj, method.DeclaringType),
                            method),
                            typeof(object)
                        ),
                    obj
                );

            return expr.Compile();
        }

        public static Func<object> BuildInstanceCreater(Type type)
        {
            return Expression.Lambda<Func<object>>(Expression.New(type)).Compile();
        }
        public static Func<T> BuildInstanceCreater<T>()
        {
            return Expression.Lambda<Func<T>>(Expression.New(typeof(T))).Compile();
        }
    }
}

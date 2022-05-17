using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Ancestor.Core
{
    public static class TableManager
    {
        private static readonly ConcurrentDictionary<Type, string> TableNames
             = new ConcurrentDictionary<Type, string>();
        private static readonly ConcurrentDictionary<PropertyInfo, string> FieldNames
             = new ConcurrentDictionary<PropertyInfo, string>();
        private static readonly ConcurrentDictionary<PropertyInfo, bool> Browsables
            = new ConcurrentDictionary<PropertyInfo, bool>();
        private static readonly ConcurrentDictionary<Type, PropertyInfo[]> BrowsableProps
            = new ConcurrentDictionary<Type, PropertyInfo[]>();
        public static string GetTableName(object value, bool raiseError = true)
        {
            if (value != null)
            {
                var tableName = value as string;
                if (tableName != null)
                    return tableName;
                var tableType = value as Type;
                if (tableType == null)
                    tableType = value.GetType();
                if (tableType != null && !InternalHelper.IsAnonymousType(tableType))
                    return GetName(tableType);
            }
            if (raiseError)
                RaiseTableNameNotFound();
            return null;
        }

        public static void RaiseTableNameNotFound()
        {
            throw new InvalidOperationException("can not get table name");
        }
        public static string GetName(Type type)
        {
            string name;
            if (!TableNames.TryGetValue(type, out name))
            {
#if NET40
                var attr = type.GetCustomAttributes(typeof(System.Data.Linq.Mapping.TableAttribute), false).FirstOrDefault() as System.Data.Linq.Mapping.TableAttribute;
                name = attr != null ? attr.Name : type.Name;
#elif NETSTANDARD2_0
                var attr = type.GetCustomAttributes(typeof(System.ComponentModel.DataAnnotations.Schema.TableAttribute), false).FirstOrDefault() as System.ComponentModel.DataAnnotations.Schema.TableAttribute;
                name = attr != null ? attr.Name.ToUpper() : type.Name.ToUpper();
#endif
                TableNames.AddOrUpdate(type, name, (k, v) => name);
            }
            return name;
        }

        public static string GetName(PropertyInfo property)
        {
            string name;
            if (!FieldNames.TryGetValue(property, out name))
            {
#if NET40
                var attr = property.GetCustomAttributes(typeof(System.Data.Linq.Mapping.ColumnAttribute), false).FirstOrDefault() as System.Data.Linq.Mapping.ColumnAttribute;
                name = attr != null ? attr.Name.ToUpper() : property.Name.ToUpper();
#elif NETSTANDARD2_0
                var attr = property.GetCustomAttributes(typeof(System.ComponentModel.DataAnnotations.Schema.ColumnAttribute), false).FirstOrDefault() as System.ComponentModel.DataAnnotations.Schema.ColumnAttribute;
                name = attr != null ? attr.Name.ToUpper() : property.Name.ToUpper();
#endif
                FieldNames.AddOrUpdate(property, name, (k, v) => name);
            }
            return name;
        }

        public static bool GetBrowsable(PropertyInfo property)
        {
            bool value;
            if (!Browsables.TryGetValue(property, out value))
            {
                var attr = property.GetCustomAttributes(typeof(System.ComponentModel.BrowsableAttribute), false).FirstOrDefault() as System.ComponentModel.BrowsableAttribute;
                value = attr != null ? attr.Browsable : true;
                Browsables.AddOrUpdate(property, value, (k, v) => value);
            }
            return value;
        }

        public static PropertyInfo[] GetBrowsableProperties(Type type)
        {
            PropertyInfo[] properties;
            if (!BrowsableProps.TryGetValue(type, out properties))
            {
                properties = type.GetProperties().Where(p => GetBrowsable(p)).ToArray();
                BrowsableProps.AddOrUpdate(type, properties, (k, v) => properties);
            }
            return properties;
        }
    }
}

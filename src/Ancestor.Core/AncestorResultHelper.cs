﻿using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;

namespace Ancestor.Core
{
    public static class AncestorResultHelper
    {
        public static IList MakeList(IList list, Type type)
        {
            var listType = typeof(List<>).MakeGenericType(type);
            var addItemMethod = listType.GetMethod("Add");
            var genericList = Activator.CreateInstance(listType, list.Count);
            foreach (var item in list)
                addItemMethod.Invoke(genericList, new object[] { item });
            return (IList)genericList;
        }
        public static object ResultFirst(IAncestorResult result, Type dataType, Delegate objectFactory, ResultListMode mode)
        {
            var list = InternalResultList(result, dataType, objectFactory, true, mode);
            return list.Count == 0 ? null : list[0];
        }
        public static object ResultScalar(IAncestorResult result)
        {
            object value = null;
            if (result.ReturnDataTable != null)
            {
                if (result.ReturnDataTable.Rows.Count > 0)
                    return result.ReturnDataTable.Rows[0][0];
            }
            else if (result.DataList != null)
            {
                if (result.DataList.Count > 0)
                {
                    var item = result.DataList[0];
                    var dataType = item.GetType();
                    var firstProperty = dataType.GetProperties().FirstOrDefault();
                    if (firstProperty != null)
                        return firstProperty.GetValue(item, null);
                }
            }
            return value;
        }

        public static List<T> ResultScalarList<T>(IAncestorResult result)
        {
            var list = new List<T>();
            if (result.ReturnDataTable != null)
            {
                if (result.ReturnDataTable.Rows.Count > 0)
                {
                    foreach (DataRow row in result.ReturnDataTable.Rows)
                        list.Add((T)row[0]);
                }
            }
            else if (result.DataList != null)
            {
                if (result.DataList.Count > 0)
                {
                    foreach (var item in result.DataList)
                        list.Add((T)item);
                }
            }
            return list;
        }


        public static IList ResultList(IAncestorResult result, Type dataType, Delegate objectFactory, ResultListMode mode)
        {
            return InternalResultList(result, dataType, objectFactory, false, mode);
        }
        internal static IList InternalResultList(IAncestorResult result, Type dataType, Delegate objectFactory, bool firstOnly, ResultListMode mode)
        {
            IList list = Activator.CreateInstance(typeof(List<>).MakeGenericType(dataType)) as IList;
            var hds = HardWordManager.Get(dataType);
            var factory = objectFactory as Func<object>;
            switch (result.DataType)
            {
                case AncestorResultDataType.List:
                    if (InternalHelper.IsAnonymousType(dataType) && factory != null)
                    {
                        if (result.DataList.Count > 0) // if has item than find anonymouse creation info
                        {
                            var targetProperties = dataType.GetProperties();
                            var sourceProperties = result.DataList[0].GetType().GetProperties();
                            var resolvers = new List<Func<object, object>>();
                            foreach (var targetProperty in targetProperties)
                            {
                                var property = sourceProperties.FirstOrDefault(p => p.Name == targetProperty.Name && p.PropertyType == targetProperty.PropertyType);
                                if (property != null)
                                    resolvers.Add(o => property.GetValue(o, null));
                                else if (targetProperty.PropertyType.IsValueType)
                                    resolvers.Add(o => Activator.CreateInstance(targetProperty.PropertyType));
                                else
                                    resolvers.Add(o => null);
                            }
                            foreach (var item in CloneByCunstructure(result.DataList, dataType, resolvers))
                            {
                                list.Add(item);
                                if (firstOnly)
                                    break;
                            }

                        }
                    }
                    else
                    {
                        Dictionary<PropertyInfo, Tuple<PropertyInfo, Func<object, object>>> propertyMap = null;
                        if (factory == null)
                            factory = () => Activator.CreateInstance(dataType);
                        foreach (var item in CastToItem(result.DataList, o => DeepCloneItem(o, factory(), ref propertyMap, hds, mode)))
                        {
                            list.Add(item);
                            if (firstOnly)
                                break;
                        }
                    }
                    break;
                case AncestorResultDataType.DataTable:
                    var rowObjectFactory = objectFactory as Func<DataRow, object>;
                    if (rowObjectFactory == null && factory != null)
                        rowObjectFactory = row => factory();
                    foreach (var item in TableToCollection(result.ReturnDataTable, dataType, rowObjectFactory, firstOnly, mode))
                        list.Add(item);
                    break;
            }
            return list;
        }

        private static object DeepCloneItem(object src, object dst, ref Dictionary<PropertyInfo, Tuple<PropertyInfo, Func<object, object>>> propertyMap, IDictionary<PropertyInfo, HardWordAttribute> hardWords, ResultListMode mode)
        {
            if (propertyMap == null)
            {
                var srcProps = src.GetType().GetProperties();
                propertyMap = new Dictionary<PropertyInfo, Tuple<PropertyInfo, Func<object, object>>>();
                var dstType = dst.GetType();
                foreach (var srcProp in srcProps)
                {
                    var dstProp = dstType.GetProperty(srcProp.Name);
                    if (dstProp != null)
                    {
                        Func<object, object> converter = null;
                        HardWordAttribute attr;
                        if (hardWords.TryGetValue(dstProp, out attr))
                        {
                            converter = o =>
                            {
                                var hex = o as string;
                                return GetValueFormHex(hex, attr.Encoding);
                            };
                        }
                        else
                            converter = v => v;
                        propertyMap.Add(srcProp, Tuple.Create(dstProp, converter));
                    }
                }
            }

            foreach (var srcProp in propertyMap.Keys)
            {
                var tuple = propertyMap[srcProp];
                var value = srcProp.GetValue(src, null);
                var isValued = tuple.Item1.GetValue(dst, null) != null;
                if (isValued && mode == ResultListMode.Value)
                    continue;
                if (srcProp.PropertyType == tuple.Item1.PropertyType)
                {
                    value = tuple.Item2(value);
                    tuple.Item1.SetValue(dst, value, null);
                }
                else
                {
                    value = Convert.ChangeType(value, tuple.Item1.PropertyType);
                    value = tuple.Item2(value);
                    tuple.Item1.SetValue(dst, value, null);
                }
            }
            return dst;
        }

        public static IEnumerable<byte> ConvertFromHex(string hex)
        {
            for (var index = 0; index < hex.Length; index += 2)
            {
                var h2 = hex.Substring(index, 2);
                var b = Convert.ToByte(h2, 16);
                yield return b;
            }
        }
        public static IEnumerable TableToCollection(DataTable table, Type instanceType, Func<DataRow, object> objectFactory, bool firstOnly, ResultListMode mode)
        {
            if (IsAnonymousType(instanceType))
            {
                var wrappers = GetWrappers(GetColumnNames(table.Columns), instanceType, true);
                foreach (DataRow row in table.Rows)
                {
                    yield return InternalRowToObjectByConstructor(row, instanceType, wrappers);
                    if (firstOnly)
                        yield break;
                }
            }
            else
            {
                var wrappers = GetWrappers(GetColumnNames(table.Columns), instanceType, false);
                if (objectFactory == null)
                    objectFactory = r => Activator.CreateInstance(instanceType);
                foreach (DataRow row in table.Rows)
                {
                    var instance = objectFactory(row);
                    yield return InternalRowToObjectBySetter(row, instance, wrappers, mode);
                    if (firstOnly)
                        yield break;
                }
            }
        }
        public static Type GetListItemType(IList list)
        {
            if (list != null && list.Count > 0)
            {
                var item = list[0];
                if (item != null)
                    return item.GetType();
            }
            return null;
        }

        public static IEnumerable<T> TableToCollection<T>(DataTable table, Func<DataRow, object> objectFactory = null, bool firstOnly = false, ResultListMode mode = ResultListMode.All)
        {
            return (IEnumerable<T>)TableToCollection(table, typeof(T), objectFactory, firstOnly, mode);
        }
        public static object RowToObject(DataRow row, Type instanceType, object instance, Func<DataRow, object> objectFactory = null, ResultListMode mode = ResultListMode.All)
        {
            if (IsAnonymousType(instanceType))
            {
                var wrappers = GetWrappers(GetColumnNames(row.Table.Columns), instanceType, true);
                return InternalRowToObjectByConstructor(row, instanceType, wrappers);
            }
            else
            {
                var wrappers = GetWrappers(GetColumnNames(row.Table.Columns), instanceType, false);
                if (instance != null)
                    return InternalRowToObjectBySetter(row, instance, wrappers, mode);
                if (objectFactory == null)
                    objectFactory = r => Activator.CreateInstance(instanceType);
                instance = objectFactory(row);
                return InternalRowToObjectBySetter(row, instance, wrappers, mode);
            }
        }

        public static T RowToObject<T>(DataRow row, T instance, Func<DataRow, T> objectFactory = null) where T : class
        {
            return (T)RowToObject(row, typeof(T), instance, objectFactory);
        }
        private static IEnumerable CastToItem(IEnumerable collection, Func<object, object> castDelegate)
        {
            foreach (var item in collection)
            {
                var newItem = castDelegate(item);
                yield return newItem;
            }
        }

        private static IEnumerable CloneByCunstructure(IEnumerable collection, Type type, List<Func<object, object>> resolvers)
        {
            foreach (var item in collection)
            {
                var arguments = resolvers.Select(r => r(item)).ToArray();
                yield return Activator.CreateInstance(type, arguments);
            }
        }

        internal static List<RowPropertyWrapper> GetWrappers(string[] columns, Type instanceType, bool ignoreWritable)
        {
            var result = instanceType.GetProperties().Aggregate(new
            {
                List = new List<RowPropertyWrapper>(),
                Columns = columns
            },
            (seed, property) =>
            {
                if (ignoreWritable || property.CanWrite) // property must can be written
                {
                    var columnName = columns.FirstOrDefault(r => string.Equals(r, property.Name, StringComparison.OrdinalIgnoreCase));
                    if (columnName != null) // must find in columns
                    {
                        var wrapper = new RowPropertyWrapper
                        {
                            Name = columnName,
                            PropertyInfo = property,
                        };
                        var attribute = HardWordManager.Get(property);
                        if (attribute != null)
                            wrapper.Encoding = attribute.Encoding;
                        seed.List.Add(wrapper);
                    }
                }
                return seed;
            },
            seed => seed.List);
            return result;
        }


        private static string[] GetColumnNames(DataColumnCollection columns)
        {
            var names = new string[columns.Count];
            for (var index = 0; index < columns.Count; index++)
                names[index] = columns[index].ColumnName;
            return names;
        }


        internal static object InternalRowToObjectBySetter(DataRow row, object instance, IEnumerable<RowPropertyWrapper> wrappers, ResultListMode mode)
        {
            foreach (var wrapper in wrappers)
                wrapper.SetValue(row, ref instance, mode);
            return instance;
        }
        internal static object InternalRowToObjectByConstructor(DataRow row, Type instanceType, IEnumerable<RowPropertyWrapper> wrappers)
        {
            var values = wrappers.Select(wrapper => wrapper.GetValue(row)).ToArray();
            return Activator.CreateInstance(instanceType, values);
        }
        private static bool IsAnonymousType(Type type)
        {
            var hasCompilerGeneratedAttribute = type.GetCustomAttributes(typeof(CompilerGeneratedAttribute), false).Count() > 0;
            var nameContainsAnonymousType = type.FullName.Contains("AnonymousType");
            var isAnonymousType = hasCompilerGeneratedAttribute && nameContainsAnonymousType;
            return isAnonymousType;
        }

        private static object GetSafeValue(object value, Type conversionType)
        {
            if (value == DBNull.Value || value == null)
            {
                if (conversionType.IsValueType)
                    return Activator.CreateInstance(conversionType);
                return null;
            }
            value = Convert.ChangeType(value, Nullable.GetUnderlyingType(conversionType) ?? conversionType);
            return value;
        }
        public static string GetValueFormHex(string hex, Encoding encoding)
        {
            if (hex == null) return null;
            var byteArray = ConvertFromHex(hex).ToArray();
            return encoding.GetString(byteArray);
        }
        internal struct RowPropertyWrapper
        {
            public string Name;
            public PropertyInfo PropertyInfo;
            public Encoding Encoding; // encoding convert from raw 
            public void SetValue(DataRow row, ref object item, ResultListMode mode)
            {
                var value = GetValue(row);
                var isValued = PropertyInfo.GetValue(item, null) != null;
                if (isValued && mode == ResultListMode.Value)
                    return;
                PropertyInfo.SetValue(item, value, null);
            }
            public object GetValue(DataRow row)
            {
                var raw = row[Name];
                var value = GetSafeValue(raw, PropertyInfo.PropertyType);
                if (Encoding != null && PropertyInfo.PropertyType == typeof(string))
                    value = GetValueFormHex((string)value, Encoding);
                return value;
            }
        }
    }
    public enum ResultListMode
    {
        All,
        Value,
    }
}

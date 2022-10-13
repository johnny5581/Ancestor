using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace Ancestor.Core
{
    internal static class InternalHelper
    {
        public static TReturn GetValue<T, TReturn>(Func<T, TReturn> action, IEnumerable<Attribute> attributes) where T : Attribute
        {
            foreach(var attr in attributes)
            {
                if (attr is T)
                    return action((T)attr);
            }
            return default(TReturn);
        }
        public static IList ToList(IEnumerable enumeration, Type dataType)
        {
            var listType = typeof(List<>).MakeGenericType(dataType);
            var list = Activator.CreateInstance(listType) as IList;
            foreach (var item in enumeration)
                list.Add(item);
            return list;
        }

        public static bool IsAnonymousType(Type type)
        {
            var hasCompilerGeneratedAttribute = type.GetCustomAttributes(typeof(CompilerGeneratedAttribute), false).Count() > 0;
            var nameContainsAnonymousType = type.FullName.Contains("AnonymousType");
            var isAnonymousType = hasCompilerGeneratedAttribute && nameContainsAnonymousType;
            return isAnonymousType;
        }

        public static bool IsUnderlyingType(Type type, Type comparionType)
        {
            var underlying = GetUnderlyingType(type);            
            return underlying == comparionType;
        }

        public static bool IsCollectionType(Type type)
        {
            return type.GetInterface("IEnumerable") != null && type != typeof(string);            
        }

        public static bool IsDictionary(Type type)
        {
            return typeof(IDictionary).IsAssignableFrom(type);
        }

        public static Type GetUnderlyingType(Type type)
        {
            var underlying = Nullable.GetUnderlyingType(type);
            return underlying ?? type;
        }
        public static Type GetSelectSourceType(Type modelType, object origin, Type dataType, bool raiseError)
        {
            var originType = origin as Type;
            if (originType != null)
                return originType;
            if (modelType != null)
                return modelType;
            if (dataType != null)
                return dataType;
            if(raiseError)
                throw new InvalidOperationException("can not detected source type");
            return null;
        }

        public static string GetTableName(Type modelType, object origin, Type dataType, bool raiseError)
        {
            var tableName = TableManager.GetTableName(origin, false) ?? TableManager.GetTableName(modelType, false) ?? TableManager.GetTableName(dataType, false);
            if (string.IsNullOrEmpty(tableName) && raiseError)
                TableManager.RaiseTableNameNotFound();
            return tableName;
        }

        public static bool IsDecimalType(Type type)
        {
            var code = Type.GetTypeCode(type);
            switch(code)
            {
                case TypeCode.Int16:
                case TypeCode.Int32:
                case TypeCode.Int64:
                case TypeCode.UInt16:
                case TypeCode.UInt32:
                case TypeCode.UInt64:
                case TypeCode.Decimal:
                case TypeCode.Single:
                case TypeCode.Double:
                    return true;
                default:
                    return false;
            }
        }
    }
    internal static class NativeMethods
    {
        // Declares managed prototypes for unmanaged functions.
        [DllImport("User32.dll", EntryPoint = "MessageBox",
            CharSet = CharSet.Auto)]
        internal static extern int MessageBox(
            IntPtr hWnd, string lpText, string lpCaption, uint uType);

        // Causes incorrect output in the message window.
        [DllImport("User32.dll", EntryPoint = "MessageBoxW",
            CharSet = CharSet.Ansi)]
        internal static extern int MessageBox2(
            IntPtr hWnd, string lpText, string lpCaption, uint uType);

        // Causes an exception to be thrown. EntryPoint, CharSet, and
        // ExactSpelling fields are mismatched.
        [DllImport("User32.dll", EntryPoint = "MessageBox",
            CharSet = CharSet.Ansi, ExactSpelling = true)]
        internal static extern int MessageBox3(
            IntPtr hWnd, string lpText, string lpCaption, uint uType);



        public static bool YesNo(string message, string caption = null)
        {
            return MessageBox(IntPtr.Zero, message, caption ?? "", (uint)0x00000001L) == 1;
        }
        public static void MsgBox(string message, string caption = null)
        {
            MessageBox(IntPtr.Zero, message, caption ?? "", (uint)0x00000000L);
        }
    }
}

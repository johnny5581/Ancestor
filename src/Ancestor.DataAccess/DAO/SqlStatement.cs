using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Ancestor.DataAccess.DAO
{
    public static class SqlStatement
    {
        public enum Joins
        {
            Inner, Outer, Left, Right
        }
        /// <summary>
        /// Join statement
        /// </summary>        
        public static bool JoinEquals<T>(T left, T right)
        {
            return true;
        }
        /// <summary>
        /// Join statement
        /// </summary>        
        public static bool JoinEquals<T>(T left, T right, Joins joins)
        {
            return true;
        }
        /// <summary>
        /// Betweem statememt
        /// </summary>        
        public static bool Between<T>(T obj, T from, T to)
        {
            return true;
        }

        /// <summary>
        /// NotNull statement
        /// </summary>        
        public static T NotNull<T>(T obj)
        {
            return obj;
        }
        /// <summary>
        /// NotNull statement
        /// </summary>        
        public static T NotNull<T>(T obj, T emptyValue)
        {
            return obj;
        }
        /// <summary>
        /// Truncate statement
        /// </summary>
        public static T Truncate<T>(T obj)
        {
            return obj;
        }
        /// <summary>
        /// ToString statement
        /// </summary>        
        public static string ToString<T>(T obj)
        {
            return ToString(obj, null);
        }
        /// <summary>
        /// ToString statement
        /// </summary>        
        public static string ToString<T>(T obj, string format)
        {
            return ToString(obj, format, true);
        }
        /// <summary>
        /// ToString statement
        /// </summary>        
        public static string ToString<T>(T obj, string format, bool useFormatConvert)
        {
            return Convert.ToString(obj);
        }
        /// <summary>
        /// use function
        /// </summary>        
        public static T Func<T>(string name, params object[] args)
        {
            return default(T);
        }
        /// <summary>
        /// use iif func
        /// </summary>        
        public static T If<TObj, T>(bool condition, T positive, T negative)
        {
            return default(T);
        }
        /// <summary>
        /// use like statement
        /// </summary>        
        public static bool Like(string value, string likePattern)
        {
            return true;
        }
        /// <summary>
        /// use not like statement
        /// </summary>        
        public static bool NotLike(string value, string likePattern)
        {
            return false;
        }
    }
}

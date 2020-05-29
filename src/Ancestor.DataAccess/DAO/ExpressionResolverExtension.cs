using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ancestor.DataAccess.DAO
{
    public static class ExpressionResolverExtension
    {
        /// <remark>Oracle Only</remark>
        /// <summary>
        /// Left join
        /// </summary>        
        /// <example>
        ///     // The result is X.PID = y.PID(+)
        ///     x.PID = y.PID.Plus()
        /// </example>
        public static T Plus<T>(this T t)
        {
            return t;
        }

        /// <summary>
        /// between Begin and End
        /// </summary>        
        /// <example>
        ///     var dateEnd = DateTime.Now;
        ///     var dateBgn = dateEnd.AddDays(-3);
        ///     
        ///     //The result is X.BIRTHDATE BETWEEN :dateBgn AND :dateEnd
        ///     x.BIRTHDATE.Between(dateBgn, dateEnd);
        /// </example>
        public static bool Between(this DateTime? dt, DateTime? begin, DateTime? end)
        {
            return dt >= begin && dt <= end;
        }
        /// <summary>
        /// between Begin and End
        /// </summary>        
        /// <example>
        ///     var dateEnd = DateTime.Now;
        ///     var dateBgn = dateEnd.AddDays(-3);
        ///     
        ///     //The result is X.BIRTHDATE BETWEEN :dateBgn AND :dateEnd
        ///     x.BIRTHDATE.Between(dateBgn, dateEnd);
        /// </example>
        public static bool Between(this DateTime dt, DateTime? begin, DateTime? end)
        {
            return dt >= begin && dt <= end;
        }
        /// <summary>
        /// between Begin and End
        /// </summary>        
        /// <example>
        ///     var dateEnd = "20170101";
        ///     var dateBgn = "20170201";
        ///     
        ///     //The result is X.BIRTHDATE BETWEEN '20170101' AND '20170201'
        ///     x.BIRTHDATE.Between("20170101", "20170201");
        /// </example>
        public static bool Between(this string dattm, string begin, string end)
        {
            return string.Compare(dattm, begin) >= 0 && string.Compare(dattm, end) <= 0;
        }

        /// <summary>
        /// Select all fields
        /// </summary>
        /// <example>
        ///     //Will Select X.F1, X.F2, .... , Y.F1
        ///     Query&#60X,Y&#62;((x, y)=>..., (x, y)=>new [] { x.SelectAll(), y.F1  }  ))
        /// </example>
        public static object SelectAll<T>(this T t)
        {
            return t;
        }
        /// <summary>
        /// Not Null
        /// </summary>        
        /// <example>
        ///     //In Oracle, the result is NVL(X.NAME, :defaultValue) = 'Alice'
        ///     x.NAME.NotNull() == "Alice"
        /// </example>
        public static T NotNull<T>(this T property, T defaultValue)
        {
            return Equals(property, default(T)) ? defaultValue : property;
        }
        /// <remark>Oracle Only</remark>
        /// <summary>
        /// Not Null
        /// </summary>        
        /// <example>
        ///     //In Oracle, the result is NVL(X.NAME, 'this field is null') = 'Alice'
        ///     x.NAME.NotNull() == "Alice"
        /// </example>
        public static T NotNull<T>(this T property)
        {
            return NotNull(property, default(T));
        }
        /// <summary>
        /// Truncate date 
        /// </summary>
        public static DateTime Truncate(this DateTime value)
        {
            if (value != null)
            {
                value = new DateTime(value.Year, value.Month, value.Day);
            }
            return value;
        }
        /// <summary>
        /// Truncate date
        /// </summary>
        public static DateTime? Truncate(this DateTime? value)
        {
            if (value != null)
            {
                var v = value.Value;
                value = new DateTime(v.Year, v.Month, v.Day);
            }
            return value;
        }
        /// <summary>
        /// Truncate decimal
        /// </summary>        
        public static decimal? Truncate(this decimal? value)
        {
            if (value != null)
            {
                value = Math.Truncate(value.Value);
            }
            return value;
        }
        /// <summary>
        /// Truncate double
        /// </summary>        
        public static double? Truncate(this double? value)
        {
            if (value != null)
            {
                value = Math.Truncate(value.Value);
            }
            return value;
        }

        /// <summary>
        /// Group count()
        /// </summary>        
        public static T GroupCount<T>(this T value)
        {
            return value;
        }
        /// <summary>
        /// Group max
        /// </summary>        
        public static T GroupMax<T>(this T value)
        {
            return value;
        }
        /// <summary>
        /// Group min
        /// </summary>        
        public static T GroupMin<T>(this T value)
        {
            return value;
        }
    }
}

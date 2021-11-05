using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;

namespace Ancestor.Core
{
    /// <summary>
    /// Extension of <see cref="AncestorResult"/>
    /// </summary>
    public static class AncestorResultExtensions
    {
        public static IList<T> ResultList<T>(this IAncestorResult result, Func<T> objectFactory, ResultListMode? mode = null, Encoding hardwordEncoding = null)
        {
            var m = mode ?? (objectFactory != null ? ResultListMode.Value : ResultListMode.All);
            return (IList<T>)AncestorResultHelper.ResultList(result, typeof(T), objectFactory, m, hardwordEncoding);
        }

        public static T ResultFirst<T>(this IAncestorResult result, Func<T> objectFactory, ResultListMode? mode = null, Encoding hardwordEncoding = null)
        {
            var m = mode ?? (objectFactory != null ? ResultListMode.Value : ResultListMode.All);
            return (T)AncestorResultHelper.ResultFirst(result, typeof(T), objectFactory, m, hardwordEncoding);
        }
        #region Tuple result
        public static IList<Tuple<T1, T2>> ResultList<T1, T2>(this IAncestorResult result, Func<Tuple<T1, T2>> objectFactory = null, ResultListMode? mode = null, Encoding hardwordEncoding = null)
        {
            var m = mode ?? (objectFactory != null ? ResultListMode.Value : ResultListMode.All);
            return (IList<Tuple<T1, T2>>)AncestorResultHelper.ResultList(result, new Type[] { typeof(T1), typeof(T2) }, objectFactory, m, hardwordEncoding);
        }
        public static Tuple<T1, T2> ResultFirst<T1, T2>(this IAncestorResult result, Func<Tuple<T1, T2>> objectFactory = null, ResultListMode? mode = null, Encoding hardwordEncoding = null)
        {
            var m = mode ?? (objectFactory != null ? ResultListMode.Value : ResultListMode.All);
            return (Tuple<T1, T2>)AncestorResultHelper.ResultFirst(result, new Type[] { typeof(T1), typeof(T2) }, objectFactory, m, hardwordEncoding);
        }
        #endregion

        public static T ResultScalar<T>(this IAncestorResult result, T defaultValue = default(T))
        {
            try
            {
                return (T)AncestorResultHelper.ResultScalar(result);
            }
            catch
            {
                return defaultValue;
            }
        }

        public static T ThrowIfError<T>(this T result) where T : IAncestorResult
        {
            if(!result.IsSuccess)
            {
                if (result.Exception != null)
                    throw result.Exception;
                else
                    throw new AncestorException(99999, "ancestor result is failure");
            }
            return result;
        }
    }
}

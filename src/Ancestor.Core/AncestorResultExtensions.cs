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
        public static IList<T> ResultList<T>(this IAncestorResult result, Func<T> objectFactory, ResultListMode? mode = null)
        {
            var m = mode ?? (objectFactory != null ? ResultListMode.Value : ResultListMode.All);
            return (IList<T>)AncestorResultHelper.InternalResultList(result, typeof(T), objectFactory, false, m);
        }





    }
}

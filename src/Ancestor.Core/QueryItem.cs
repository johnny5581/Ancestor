using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ancestor.Core
{
    /// <summary>
    /// Query object
    /// </summary>
    [Serializable]
    public struct QueryParameter
    {
        public QueryParameter(string commandText, DBParameterCollection parameters, object options, Type dataType)
        {
            CommandText = commandText;
            Parameters = parameters;
            Options = options;
            DataType = dataType;
        }

        public string CommandText { get; private set; }
        public DBParameterCollection Parameters { get; private set; }
        public object Options { get; private set; }
        public Type DataType { get; set; }

        public static QueryParameter Parse(object value)
        {
            if (value != null && value is QueryParameter)
                return (QueryParameter)value;
            return default(QueryParameter);
        }
    }
}

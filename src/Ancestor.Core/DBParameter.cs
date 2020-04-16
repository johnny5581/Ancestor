using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Ancestor.Core
{
    /// <summary>
    /// Database Parameter
    /// </summary>
    [DebuggerDisplay("{Name}: {Value} ({ParameterType})")]
    public class DBParameter : IEquatable<DBParameter>
    {
        private DBParameterType _parameterType;
        public DBParameter()
        {
        }
        protected DBParameter(string name, ParameterDirection direction)
            : this()
        {
            Name = name;
            ParameterDirection = direction;
        }
        public DBParameter(string name, DbType commonType, ParameterDirection direction)
            : this(name, direction)
        {
            _parameterType = new DBParameterType(commonType);
        }
        public DBParameter(string name, string type, ParameterDirection direction)
             : this(name, direction)
        {
            _parameterType = new DBParameterType(type);
        }

        public DBParameter(string name, object value)
            : this(name, "_LAZY_", ParameterDirection.Input)
        {
            Value = value;
        }
        /// <summary>
        /// Parameter name
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Parameter size
        /// </summary>
        public int? Size { get; set; }
        /// <summary>
        /// Parameter direction
        /// </summary>
        public ParameterDirection ParameterDirection { get; set; }
        /// <summary>
        /// Parameter raw value
        /// </summary>
        public object Value { get; set; }
        public Type ItemType { get; set; }


        public DBParameterType ParameterType
        {
            get { return _parameterType; }
        }

        /// <summary>
        /// Parameter type
        /// </summary>
        public string Type
        {
            get { return _parameterType.Name; }
            set { _parameterType = new DBParameterType(value); }
        }


        public int? GetInt()
        {
            return GetValue<int>();
        }

        public float? GetFloat()
        {
            return GetValue<float>();
        }

        public decimal? GetDecimal()
        {
            return GetValue<decimal>();
        }

        public Nullable<T> GetValue<T>() where T : struct
        {
            try
            {
                return (T)Value;
            }
            catch
            {
                return null;
            }
        }

        public override string ToString()
        {
            var sb = new StringBuilder().Append(Name);
            if (ParameterDirection == ParameterDirection.Input)
                sb.AppendFormat("={0}", Value);
            else
                sb.AppendFormat("({0})", ParameterType);
            return sb.ToString();
        }

        public bool Equals(DBParameter other)
        {
            if (other == null)
                return false;
            return Name.Equals(other.Name);
        }
    }
    [DebuggerDisplay("{Name} ({Code})")]
    public struct DBParameterType
    {
        public int Code;
        public string Name;


        public DBParameterType(DbType code)
        {
            Code = (int)code;
            Name = Enum.GetName(typeof(DbType), code).ToUpper();
        }
        public DBParameterType(string name)
        {
            Name = name;
            DbType parseType;
            if (Enum.TryParse(name, true, out parseType))
                Code = (int)parseType;
            else
                Code = 999; // 自定義類別
        }

        public override string ToString()
        {
            return string.Format("{0}<{1}>", Name, Code);
        }

        public bool IsLazy
        {
            get { return Name == "_LAZY_"; }
        }
        public bool IsDbType
        {
            get { return Code != 999; }
        }
    }

    public sealed class DBParameterCollection : KeyedCollection<string, DBParameter>
    {
        private int _index = 0;
        private string _returnValueParameterName;
        public DBParameterCollection() : base(StringComparer.OrdinalIgnoreCase)
        {
        }
        public DBParameterCollection(IEnumerable<DBParameter> collection)
        {
            AddRange(collection);
        }
        public int Current
        {
            get { return _index; }
        }
        public int Next
        {
            get { return ++_index; }
        }

        protected override string GetKeyForItem(DBParameter item)
        {
            return item.Name;
        }
        public new DBParameter this[string key]
        {
            get
            {
                if (key == _returnValueParameterName && _returnValueParameterName != null)
                    return base[""];
                return base[key];
            }
        }
        public new void Add(DBParameter item)
        {
            // clear return value's parameter name
            if (item.ParameterDirection == ParameterDirection.ReturnValue)
            {
                _returnValueParameterName = item.Name;
                item.Name = "";
            }
            base.Add(item);
        }
        public void AddRange(IEnumerable<DBParameter> collection)
        {
            foreach (var item in collection)
                Add(item);
        }
        public DBParameter Add(string name, DbType commonType, ParameterDirection direction)
        {
            var parameter = new DBParameter(name, commonType, direction);
            Add(parameter);
            return parameter;
        }
        public DBParameter Add(string name, string type, ParameterDirection direction)
        {
            var parameter = new DBParameter(name, type, direction);
            Add(parameter);
            return parameter;
        }

        public DBParameter Add(string name, object value)
        {
            var parameter = new DBParameter(name, value);
            Add(parameter);
            return parameter;
        }



    }
}

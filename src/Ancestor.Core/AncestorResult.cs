﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace Ancestor.Core
{
    /// <summary>
    /// Ancestor execute result interface
    /// </summary>
    public interface IAncestorResult
    {
        #region Properties
        /// <summary>
        /// Execute return Code
        /// </summary>
        int Code { get; }
        /// <summary>
        /// Execute success or not 
        /// </summary>
        bool IsSuccess { get; }
        /// <summary>
        /// Execute effect rows number
        /// </summary>
        int EffectRows { get; }
        /// <summary>
        /// Error message
        /// </summary>
        string Message { get; }
        /// <summary>
        /// Return Data (List)
        /// </summary>
        IList DataList { get; }
        /// <summary>
        /// Return Data (DataTable)
        /// </summary>
        DataTable ReturnDataTable { get; }
        #endregion Properties

        #region Extra Properties
        /// <summary>
        /// Query result
        /// </summary>
        object Data { get; }
        /// <summary>
        /// Error exception
        /// </summary>
        AncestorException Exception { get; }
        /// <summary>
        /// Query result data type
        /// </summary>
        AncestorResultDataType DataType { get; }
        #endregion Extra Properties
    }

    /// <summary>
    /// Ancestor query result data type
    /// </summary>
    public enum AncestorResultDataType
    {
        None,
        DataTable,
        List,
    }

    /// <summary>
    /// Ancestor operation result 
    /// </summary>
    public class AncestorResult : IAncestorResult
    {
        public const int CodeSuccess = 0;
        public const int CodeFailure = 99999;
        private string _errorMessage;
        private AncestorException _innerException;
        public AncestorResult()
        {
        }


        protected AncestorResult(bool success)
        {
            IsSuccess = success;
        }
        public AncestorResult(Exception exception)
            : this(false)
        {
            Code = 99999;
            SetException(exception);
            _errorMessage = exception.Message;
        }
        public AncestorResult(int code, Exception exception)
            : this(false)
        {
            Code = code;
            SetException(exception);
            _errorMessage = exception.Message;
        }
        public AncestorResult(IList list)
            : this(true)
        {
            DataList = list;
        }
        public AncestorResult(DataTable table)
            : this(true)
        {
            ReturnDataTable = table;
        }
        public AncestorResult(int rows)
          : this(true)
        {
            EffectRows = rows;
        }
        public int Code { get; private set; }
        public bool IsSuccess { get; set; }
        public IList DataList { get; set; }
        public DataTable ReturnDataTable { get; set; }
        public int EffectRows { get; set; }
        public string Message
        {
            get { return _errorMessage; }
            set { _errorMessage = value; }
        }
        public AncestorException Exception
        {
            get { return _innerException; }
        }
        
        public QueryParameter QueryParameter { get; set; }

        object IAncestorResult.Data
        {
            get
            {
                if (DataList != null)
                    return DataList;
                else if (ReturnDataTable != null)
                    return ReturnDataTable;
                return null;
            }
        }
        AncestorResultDataType IAncestorResult.DataType
        {
            get
            {
                if (ReturnDataTable != null)
                    return AncestorResultDataType.DataTable;
                else if (DataList != null)
                    return AncestorResultDataType.List;
                else
                    return AncestorResultDataType.None;
            }
        }



        public List<T> ResultList<T>() where T : class, new()
        {
            return (List<T>)AncestorResultHelper.ResultList(this, typeof(T), null, ResultListMode.All);
        }
        public T ResultFirst<T>() where T : class, new()
        {
            return (T)AncestorResultHelper.ResultFirst(this, typeof(T), null, ResultListMode.All);
        }
        public virtual object ResultScalar()
        {
            return AncestorResultHelper.ResultScalar(this);
        }

        private void SetException(Exception exception)
        {
            if (exception is AncestorException)
                _innerException = (AncestorException)exception;
            else
                _innerException = new AncestorException(99999, "something wrong when using ancestor");
        }
    }
    /// <summary>
    /// Ancestor execute sp result 
    /// </summary>
    public class AncestorExecuteResult : AncestorResult
    {
        private readonly Dictionary<string, DBParameter> _parameters
            = new Dictionary<string, DBParameter>(StringComparer.OrdinalIgnoreCase);

        public AncestorExecuteResult(Exception exception) : base(exception)
        {
        }

        public AncestorExecuteResult(int rows, IEnumerable<DBParameter> parameters) : base(rows)
        {
            BindParameters(parameters);
        }

        public AncestorExecuteResult(object value, IEnumerable<DBParameter> parameters) : base(true)
        {
            _parameters.Add(DBParameter.ReturnValueName, new DBParameter(DBParameter.ReturnValueName, value));
            BindParameters(parameters);
        }

        public AncestorExecuteResult(int code, Exception exception) : base(code, exception)
        {
        }
        public DBParameter GetParameter(string name = DBParameter.ReturnValueName)
        {
            DBParameter p;
            _parameters.TryGetValue(name, out p);
            return p;
        }
        public object GetValue(string name = DBParameter.ReturnValueName)
        {
            DBParameter p;
            if (_parameters.TryGetValue(name, out p))
                return p.Value;
            throw new ArgumentOutOfRangeException("name", "parameter name not found: " + name);
        }
        public T? GetValue<T>(string name = DBParameter.ReturnValueName, bool useDefault = false) where T : struct
        {
            return (T?)GetStructValue<T>(name, useDefault);
        }
        public T GetValue<T>(string name = DBParameter.ReturnValueName) where T : class
        {
            return (T)GetClassValue<T>(name);
        }

        public override object ResultScalar()
        {
            return GetValue();
        }

        public bool TryGetValue(string name, out object value)
        {
            try
            {
                value = GetValue(name);
                return true;
            }
            catch
            {
                value = null;
                return false;
            }
        }
        public bool TryGetValue<T>(string name, out T value)
        {
            try
            {
                object v;
                if (typeof(T).IsValueType)
                {
                    v = GetStructValue<T>(name, false);
                    if (v == null)
                        throw new FormatException("value is empty");
                }
                else
                    v = GetClassValue<T>(name);
                value = (T)v;
                return true;
            }
            catch
            {
                value = default(T);
                return false;
            }
        }
        private void BindParameters(IEnumerable<DBParameter> parameters)
        {
            if (parameters != null)
            {
                foreach (var dbParameter in parameters)
                {
                    if (dbParameter.ParameterDirection == ParameterDirection.Output
                        || dbParameter.ParameterDirection == ParameterDirection.InputOutput)
                        _parameters.Add(dbParameter.Name, dbParameter);
                }
            }
        }
        private object GetStructValue<T>(string name, bool useDefault) 
        {
            var value = GetValue(name);
            if (value == null)
            {
                if (useDefault)
                    return Activator.CreateInstance(typeof(T));
                else
                    return null;
            }
            return (T)value;
        }

        private object GetClassValue<T>(string name) 
        {
            var value = GetValue(name);
            return (T)value;
        }
    }
}

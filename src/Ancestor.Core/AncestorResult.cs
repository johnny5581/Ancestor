using System;
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
        Exception Exception { get; }
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
        private string _errorMessage;
        private Exception _error;
        private string _command;
        private DBParameterCollection _parameters;
        private object _option;

        private static bool _enableSaveCommand = false;
        /// <summary>
        /// Enable to save command text and parameter list to result
        /// </summary>
        public static bool EnableSaveCommand
        {
            get { return _enableSaveCommand; }
            set { _enableSaveCommand = value; }
        }
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
            _error = exception;
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

        public AncestorResult(Exception exception, string command, DBParameterCollection parameters, object option)
            : this(exception)
        {
            SaveCommand(command, parameters, option);
        }
        public AncestorResult(IList list, string command, DBParameterCollection parameters, object option)
            : this(list)
        {
            SaveCommand(command, parameters, option);
        }
        public AncestorResult(DataTable table, string command, DBParameterCollection parameters, object option)
            : this(table)
        {
            SaveCommand(command, parameters, option);
        }
        public AncestorResult(int rows, string command, DBParameterCollection parameters, object option)
          : this(rows)
        {
            _command = command;
            _parameters = parameters;
            _option = option;
        }
        public bool IsSuccess { get; set; }
        public IList DataList { get; set; }
        public DataTable ReturnDataTable { get; set; }
        public int EffectRows { get; set; }
        public string Message
        {
            get { return _errorMessage; }
            set { _errorMessage = value; }
        }

        internal Exception InnerException
        {
            get { return _error; }
            set { _error = value; }
        }

        public string Command
        {
            get { return _command; }
        }
        public DBParameterCollection Parameters
        {
            get { return _parameters; }
        }
        public object Option
        {
            get { return _option; }
        }


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
        Exception IAncestorResult.Exception
        {
            get { return _error; }
        }


        public List<T> ResultList<T>() where T : class, new()
        {
            return (List<T>)AncestorResultHelper.ResultList(this, typeof(T), null, ResultListMode.All);
        }
        public T ResultFirst<T>() where T : class, new()
        {
            return (T)AncestorResultHelper.ResultFirst(this, typeof(T), null, ResultListMode.All);
        }
        protected void SaveCommand(string command, DBParameterCollection parameters, object option)
        {
            if (_enableSaveCommand)
            {
                _command = command;
                _parameters = parameters;
                _option = option;
            }
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

        public AncestorExecuteResult(int rows, string command, DBParameterCollection parameters, object option) : base(rows, command, parameters, option)
        {
            if (parameters != null)
            {
                foreach (var dbParameter in parameters)
                {
                    if (dbParameter.ParameterDirection != ParameterDirection.Input)
                        _parameters.Add(dbParameter.Name, dbParameter);
                }
            }
        }

        public AncestorExecuteResult(object value, string command, DBParameterCollection parameters, object option) : base(true)
        {
            _parameters.Add("", new DBParameter("", value));
            SaveCommand(command, parameters, option);
        }

        public AncestorExecuteResult(Exception exception, string command, DBParameterCollection parameters, object option) : base(exception, command, parameters, option)
        {
        }

        public DBParameter GetParameter(string name = "")
        {
            DBParameter p;
            _parameters.TryGetValue(name, out p);
            return p;
        }
        public object GetValue(string name = "")
        {
            DBParameter p;
            if (_parameters.TryGetValue(name, out p))
                return p.Value;
            throw new ArgumentOutOfRangeException("name", "parameter name not found: " + name);
        }
        public T GetValue<T>(string name = "")
        {
            return (T)GetValue(name);
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
                value = GetValue<T>(name);
                return true;
            }
            catch
            {
                value = default(T);
                return false;
            }
        }
    }
}

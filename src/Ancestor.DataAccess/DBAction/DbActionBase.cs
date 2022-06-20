using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using Ancestor.Core;
using Ancestor.DataAccess.DAO;
using Ancestor.DataAccess.DBAction.Mapper;

namespace Ancestor.DataAccess.DBAction
{
    public abstract class DbActionBase : IDbAction
    {
        private static readonly Dictionary<DbType, Type> TypeMap = new Dictionary<DbType, Type>
        {
            [DbType.Byte] = typeof(byte),
            [DbType.SByte] = typeof(sbyte),
            [DbType.Int16] = typeof(short),
            [DbType.UInt16] = typeof(ushort),
            [DbType.Int32] = typeof(int),
            [DbType.UInt32] = typeof(uint),
            [DbType.Int64] = typeof(long),
            [DbType.UInt64] = typeof(ulong),
            [DbType.Single] = typeof(float),
            [DbType.Double] = typeof(double),
            [DbType.Decimal] = typeof(decimal),
            [DbType.Boolean] = typeof(bool),
            [DbType.String] = typeof(string),
            [DbType.StringFixedLength] = typeof(char),
            [DbType.Guid] = typeof(Guid),
            [DbType.DateTime] = typeof(DateTime),
            [DbType.DateTimeOffset] = typeof(DateTimeOffset),
            [DbType.Time] = typeof(TimeSpan),
            [DbType.Binary] = typeof(byte[]),
            [DbType.Byte] = typeof(byte),
            [DbType.SByte] = typeof(sbyte),
            //[DbType.Object] = typeof(object)
        };
        private readonly IDbConnection _connection;
        private IDbTransaction _transaction;
        private bool _disposed;
        protected readonly object Locker = new object();
        private readonly DataAccessObjectBase _dao;
        private string _lastSqlCommand;
        internal Core.Logging.ILogger logger;
        internal Core.Logging.ILogger sqlLogger;
        private bool? _autoCloseConnection;

        public DbActionBase(DataAccessObjectBase dao)
        {
            var loggerName = "Ancestor." + GetType().Name;
            logger = Core.Logging.Logger.CreateInstance(loggerName);
            sqlLogger = Core.Logging.Logger.CreateInstance(loggerName + ".Sql");
            string dsn = null;

            switch (dao.Factory.Mode)
            {
                case Factory.DAOFactoryEx.SourceMode.DBObject:
                    _connection = CreateConnection((DBObject)dao.Factory.Source, out dsn);
                    break;
                case Factory.DAOFactoryEx.SourceMode.ConnectionString:
                    _connection = CreateConnection((string)dao.Factory.Source, out dsn);
                    break;
                case Factory.DAOFactoryEx.SourceMode.Connection:
                    _connection = CreateConnection((string)dao.Factory.Source, out dsn);
                    break;                
            }

            if (_connection == null)
                throw new InvalidOperationException("no connection found");
            DataSource = dsn;
            _dao = dao;

        }
        public DbActionBase(DataAccessObjectBase dao, string connStr)
        {
            var loggerName = "Ancestor." + GetType().Name;
            logger = Core.Logging.Logger.CreateInstance(loggerName);
            sqlLogger = Core.Logging.Logger.CreateInstance(loggerName + ".Sql");
            string dsn;
            _connection = CreateConnection(connStr, out dsn);
            if (_connection == null)
                throw new InvalidOperationException("no connection found");
            DataSource = dsn;
            _dao = dao;
        }

        #region Property
        /// <summary>
        /// Is transacting
        /// </summary>
        public bool IsTransacting
        {
            get { return _transaction != null; }
        }
        /// <summary>
        /// Auto close connection after used
        /// </summary>
        public bool AutoCloseConnection
        {
            get { return _autoCloseConnection ?? !"manual".Equals(AncestorGlobalOptions.GetString("option.close"), StringComparison.OrdinalIgnoreCase); }
            set { _autoCloseConnection = value; }
        }
        internal IDbTransaction Transaction
        {
            get { return _transaction; }
        }
        public virtual string DataSource
        {
            get; set;
        }
        #endregion Property

        #region Public 
        public void BeginTransaction()
        {
            OpenConnection();
            _transaction = _connection.BeginTransaction();
        }

        public void BeginTransaction(IsolationLevel isolationLevel)
        {
            OpenConnection();
            _transaction = _connection.BeginTransaction(isolationLevel);
        }

        public void Commit()
        {
            lock (Locker)
            {
                _transaction.Commit();
                _transaction.Dispose();
                _transaction = null;
                CloseConnection();
            }
        }

        public void Rollback()
        {
            lock (Locker)
            {
                _transaction.Rollback();
                _transaction.Dispose();
                _transaction = null;
                CloseConnection();
            }
        }
        void IDbAction.OpenConnection()
        {
            OpenConnection(_connection);
        }
        void IDbAction.CloseConnection()
        {
            CloseConnection(_connection, _transaction, true);
        }
        public IDbConnection Connection
        {
            get { return _connection; }
        }


        public virtual DbActionResult<IList> Query(string sql, DBParameterCollection dbParameters, Type dataType, DbActionOptions options = null)
        {
            var queryParameter = new QueryParameter(sql, dbParameters, options, dataType);
            return ActionWithConnectionHandler(() =>
            {
                Log("QueryList", sql, dbParameters);
                var dynamicParameter = CreateDynamicParameters(dbParameters);
                IList list = _connection.Query(dataType, sql, dynamicParameter, _transaction).ToList();
                if (dataType != null)
                    list = AncestorResultHelper.MakeList(list, dataType);
                LogRes("QueryList", list.Count);
                return list;
            }, queryParameter);
        }


        public virtual DbActionResult<DataTable> Query(string sql, DBParameterCollection dbParameters, DbActionOptions options = null)
        {
            var queryParameter = new QueryParameter(sql, dbParameters, options, null);
            return ActionWithConnectionHandler(() =>
            {
                Log("QueryTable", sql, dbParameters);
                using (var cmd = _connection.CreateCommand())
                {
                    cmd.CommandText = sql;
                    cmd.CommandType = CommandType.Text;
                    cmd.Transaction = _transaction;
                    BindParameters(cmd, dbParameters, options);
                    PreQuery(cmd, options);
                    var table = CreateDataTable(cmd, 0, 0);
                    LogRes("QueryTable", table.Rows.Count);
                    return table;
                }
            }, queryParameter);
        }

        public virtual DbActionResult<object> QueryFirst(string sql, DBParameterCollection dbParameters, Type dataType, DbActionOptions options = null)
        {
            var queryParameter = new QueryParameter(sql, dbParameters, options, dataType);
            return ActionWithConnectionHandler(() =>
            {
                Log("QueryFirst", sql, dbParameters);
                var dynamicParameter = CreateDynamicParameters(dbParameters);
                var data = _connection.QueryFirstOrDefault(dataType, sql, dynamicParameter, _transaction);
                LogRes("QueryFirst", data == null ? 0 : 1);
                return data;
            }, queryParameter);
        }
        public virtual DbActionResult<DataTable> QueryFirst(string sql, DBParameterCollection dbParameters, DbActionOptions options = null)
        {
            var queryParameter = new QueryParameter(sql, dbParameters, options, null);
            return ActionWithConnectionHandler(() =>
            {
                Log("QueryFirstRow", sql, dbParameters);
                using (var cmd = _connection.CreateCommand())
                {
                    cmd.CommandText = sql;
                    cmd.CommandType = CommandType.Text;
                    cmd.Transaction = _transaction;
                    BindParameters(cmd, dbParameters, options);
                    PreQuery(cmd, options);
                    var table = CreateDataTable(cmd, 0, 1);
                    LogRes("QueryFirstRow", table.Rows.Count);
                    return table;
                }
            }, queryParameter);
        }
        public virtual DbActionResult<int> ExecuteNonQuery(string sql, DBParameterCollection dbParameters, DbActionOptions options = null)
        {
            var queryParameter = new QueryParameter(sql, dbParameters, options, null);
            return ActionWithConnectionHandler(() =>
            {
                Log("ExecuteNonQuery", sql, dbParameters);
                using (var cmd = _connection.CreateCommand())
                {
                    cmd.CommandText = sql;
                    cmd.CommandType = CommandType.Text;
                    cmd.Transaction = _transaction;
                    BindParameters(cmd, dbParameters, options);
                    PreExecute(cmd, options);
                    var effectRows = cmd.ExecuteNonQuery();
                    RestoreParameters(cmd, dbParameters);
                    LogRes("ExecuteNonQuery", effectRows);
                    return effectRows;
                }
            }, queryParameter);
        }
        public virtual DbActionResult<object> ExecuteStoreProcedure(string name, DBParameterCollection dbParameters, DbActionOptions options = null)
        {
            var queryParameter = new QueryParameter(name, dbParameters, options, null);
            return ActionWithConnectionHandler(() =>
            {
                Log("ExecuteStorePrecedure", name, dbParameters);
                using (var cmd = _connection.CreateCommand())
                {
                    cmd.CommandText = name;
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Transaction = _transaction;
                    BindParameters(cmd, dbParameters, options);
                    PreExecute(cmd, options);
                    var effectRows = cmd.ExecuteNonQuery();
                    LogRes("ExecuteStorePrecedure", effectRows);
                    return RestoreParameters(cmd, dbParameters);
                }
            }, queryParameter);
        }

        public virtual DbActionResult<object> ExecuteScalar(string sql, DBParameterCollection dbParameters, DbActionOptions options = null)
        {
            var queryParameter = new QueryParameter(sql, dbParameters, options, null);
            return ActionWithConnectionHandler(() =>
            {
                Log("ExecuteScalar", sql, dbParameters);
                var dynamicParameter = CreateDynamicParameters(dbParameters);
                var scalarValue = _connection.ExecuteScalar(sql, dynamicParameter, _transaction);
                LogRes("ExecuteScalar", scalarValue);
                return scalarValue;
            }, queryParameter);
        }
        DbActionOptions IDbAction.CreateOptions()
        {
            return CreateOption();
        }
        #endregion Public

        #region Protected / Private
        protected abstract IDbConnection CreateConnection(DBObject dbObject, out string dataSource);
        protected abstract IDbConnection CreateConnection(string connStr, out string dataSource);
        protected abstract IDbConnection CreateConnection(IDbConnection conn, out string dataSource);
        protected abstract IDbDataAdapter CreateAdapter(IDbCommand command);
        protected abstract IDbDataParameter CreateParameter(DBParameter parameter, DbActionOptions options);
        protected abstract DbActionOptions CreateOption();
        protected virtual void BindParameters(IDbCommand command, DBParameterCollection dbParameters, DbActionOptions options)
        {
            if (dbParameters != null)
            {
                foreach (var dbParameter in dbParameters)
                {
                    var dbDataParameter = CreateParameter(dbParameter, options);
                    command.Parameters.Add(dbDataParameter);
                }
            }
        }
        protected virtual object RestoreParameters(IDbCommand command, DBParameterCollection dbParameters)
        {
            object returnValue = null;
            if (dbParameters != null)
            {
                for (var index = 0; index < dbParameters.Count; index++)
                {
                    var direction = dbParameters[index].ParameterDirection;
                    if (direction != ParameterDirection.Input)
                    {
                        var dbDataParameter = (IDbDataParameter)command.Parameters[dbParameters[index].Name];
                        RestoreParameter(dbDataParameter, dbParameters[index]);
                        if (direction == ParameterDirection.ReturnValue)
                            returnValue = dbParameters[index].Value;
                    }
                }
            }
            return returnValue;
        }
        protected virtual void RestoreParameter(IDbDataParameter dbDataParameter, DBParameter dbParameter)
        {
            dbParameter.Value = GetDbValue(dbDataParameter.Value);
        }
        protected virtual DataTable CreateDataTable(IDbCommand command, int startIndex = 0, int length = 0)
        {
            var adapter = CreateAdapter(command);
            var dataSet = new DataSet();
            adapter.Fill(dataSet);
            DataTable table = dataSet.Tables[0];
            if (startIndex != 0 || length != 0)
            {
                var newTable = dataSet.Tables[0].Clone();
                if (table.Rows.Count > 0)
                {
                    var maxLength = length == 0 ? int.MaxValue : length;
                    for (int index = 0, count = 0; index < table.Rows.Count && count < maxLength; index++, count++)
                    {
                        newTable.Rows.Add(table.Rows[index]);
                    }
                }
                table = newTable;
            }
            return table;
        }
        protected virtual object GetDbValue(object dbValue)
        {
            if (IsDbNull(dbValue))
            {
                return null;
            }
            return dbValue;
        }
        protected virtual void PreQuery(IDbCommand command, DbActionOptions options)
        {

        }
        protected virtual void PreExecute(IDbCommand command, DbActionOptions options)
        {

        }
        protected virtual AncestorException CreateAncestorException(Exception innerException, QueryParameter parameter)
        {
            return CreateAncestorException(9999, "database exception", innerException, parameter);
        }
        protected AncestorException CreateAncestorException(int code, string message, Exception innerException, QueryParameter parameter)
        {
            var exception = new AncestorException(code, message, innerException);
            exception.Data["QueryParameter"] = parameter;
            return exception;
        }
        protected virtual SqlMapper.IDynamicParameters CreateDynamicParameters(IEnumerable<DBParameter> parameters)
        {
            var dynamicParameters = new DynamicParameters();
            foreach (var parameter in parameters)
            {
                var pType = parameter.ParameterType;
                DbType? type = null;
                if (!pType.IsLazy)
                    type = (DbType)pType.Code;
                dynamicParameters.Add(parameter.Name, parameter.Value, type, parameter.ParameterDirection, parameter.Size);
            }
            return dynamicParameters;
        }
        protected DbActionResult<T> ActionWithConnectionHandler<T>(Func<T> action, QueryParameter parameter)
        {
            lock (Locker)
            {
                try
                {
                    OpenConnection();
                    var result = action();
                    return new DbActionResult<T>
                    {
                        Result = result,
                        Parameter = parameter,
                    };
                }
                catch (Exception ex)
                {
                    throw CreateAncestorException(ex, parameter);
                }
                finally
                {
                    CloseConnection();
                }
            }
        }

        protected void Log(string action, string sql, IEnumerable<DBParameter> parameters)
        {
            string args = null;
            if (parameters != null)
                args = string.Join(",", parameters);
            var message = string.Format("action={0} sql=\"{1}\" args=[{2}]", action, sql, args);
            sqlLogger.WriteLog(System.Diagnostics.TraceEventType.Verbose, message);
            _lastSqlCommand = sql;
        }
        protected void LogRes(string action, int effectRows)
        {
            var message = string.Format("action={0} effectRows={1}", action, effectRows);
            sqlLogger.WriteLog(System.Diagnostics.TraceEventType.Verbose, message);
        }
        protected void LogRes(string action, object scalar)
        {
            var message = string.Format("action={0} scalar={1}", action, scalar);
            sqlLogger.WriteLog(System.Diagnostics.TraceEventType.Verbose, message);
        }
        private void OpenConnection()
        {
            OpenConnection(_connection);
        }
        private void CloseConnection()
        {
            if (AutoCloseConnection)
            {
                CloseConnection(_connection, _transaction, false);
            }
        }
        private bool IsDbNull(object value)
        {
            return value == null || value.GetType() == typeof(DBNull);
        }
        private static void CloseConnection(IDbConnection connection, IDbTransaction transaction, bool throwOnTransactionNotEmpty)
        {
            if (transaction == null)
            {
                if (connection.State.HasFlag(ConnectionState.Open))
                    connection.Close();
            }
            else if (throwOnTransactionNotEmpty)
                throw new InvalidOperationException("transaction is not null");
        }
        private static void OpenConnection(IDbConnection connection)
        {
            if (!connection.State.HasFlag(ConnectionState.Open))
                connection.Open();
        }
        #endregion Protected / Private

        #region Dispose
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        ~DbActionBase()
        {
            Dispose(false);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                    Disposing();
                _disposed = true;
            }
        }
        protected virtual void Disposing()
        {
            if (_transaction != null)
                Rollback();
            if (_connection != null)
                CloseConnection(_connection, null, false);
        }


        #endregion
    }
}

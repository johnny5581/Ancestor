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
        private bool _autoCloseConnection = true;

        public DbActionBase(DataAccessObjectBase dao, DBObject dbObject)
        {
            _connection = CreateConnection(dbObject);
            if (_connection == null)
                throw new InvalidOperationException("no connection found");
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
            get { return _autoCloseConnection; }
            set { _autoCloseConnection = value; }
        }
        #endregion Property

        #region Public 
        public void BeginTransaction()
        {
            _transaction = _connection.BeginTransaction();
        }

        public void BeginTransaction(IsolationLevel isolationLevel)
        {
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
            _autoCloseConnection = false;
        }
        void IDbAction.CloseConnection()
        {
            CloseConnection(_connection, _transaction);
            _autoCloseConnection = true;
        }


        public virtual DbActionResult<IList> Query(string sql, DBParameterCollection dbParameters, Type dataType, DbActionOptions options = null)
        {
            return ActionWithConnectionHandler(() =>
            {
                Log("QueryList", sql, dbParameters);
                var dynamicParameter = CreateDynamicParameters(dbParameters);
                var list = _connection.Query(dataType, sql, dynamicParameter, _transaction).ToList();
                return (IList)list;
            }, sql, dbParameters, options);
        }


        public virtual DbActionResult<DataTable> Query(string sql, DBParameterCollection dbParameters, DbActionOptions options = null)
        {
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
                    return CreateDataTable(cmd, 0, 0);
                }
            }, sql, dbParameters, options);
        }

        public virtual DbActionResult<object> QueryFirst(string sql, DBParameterCollection dbParameters, Type dataType, DbActionOptions options = null)
        {
            return ActionWithConnectionHandler(() =>
            {
                Log("QueryFirst", sql, dbParameters);
                var dynamicParameter = CreateDynamicParameters(dbParameters);
                var data = _connection.QueryFirstOrDefault(dataType, sql, dynamicParameter, _transaction);
                return data;
            }, sql, dbParameters, options);
        }
        public virtual DbActionResult<DataTable> QueryFirst(string sql, DBParameterCollection dbParameters, DbActionOptions options = null)
        {
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
                    return CreateDataTable(cmd, 0, 1);
                }
            }, sql, dbParameters, options);
        }
        public virtual DbActionResult<int> ExecuteNonQuery(string sql, DBParameterCollection dbParameters, DbActionOptions options = null)
        {
            return ActionWithConnectionHandler(() =>
            {
                Log("QueryTable", sql, dbParameters);
                using (var cmd = _connection.CreateCommand())
                {
                    cmd.CommandText = sql;
                    cmd.CommandType = CommandType.Text;
                    cmd.Transaction = _transaction;
                    BindParameters(cmd, dbParameters, options);
                    PreExecute(cmd, options);
                    var effectRows = cmd.ExecuteNonQuery();
                    RestoreParameters(cmd, dbParameters);
                    return effectRows;
                }
            }, sql, dbParameters, options);
        }
        public virtual DbActionResult<int> ExecuteStoreProcedure(string name, DBParameterCollection dbParameters, DbActionOptions options = null)
        {
            return ActionWithConnectionHandler(() =>
            {
                using (var cmd = _connection.CreateCommand())
                {
                    cmd.CommandText = name;
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Transaction = _transaction;
                    BindParameters(cmd, dbParameters, options);
                    PreExecute(cmd, options);
                    var effectRows = cmd.ExecuteNonQuery();
                    RestoreParameters(cmd, dbParameters);
                    return effectRows;
                }
            }, name, dbParameters, options);
        }

        public virtual DbActionResult<object> ExecuteScalar(string sql, DBParameterCollection dbParameters, DbActionOptions options = null)
        {
            return ActionWithConnectionHandler(() =>
            {
                var dynamicParameter = CreateDynamicParameters(dbParameters);
                return _connection.ExecuteScalar(sql, dynamicParameter, _transaction);
            }, sql, dbParameters, options);
        }
        DbActionOptions IDbAction.CreateOptions()
        {
            return CreateOption();
        }
        #endregion Public

        #region Protected / Private
        protected abstract IDbConnection CreateConnection(DBObject dbObject);
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
        protected virtual void RestoreParameters(IDbCommand command, DBParameterCollection dbParameters)
        {
            if (dbParameters != null)
            {
                for (var index = 0; index < dbParameters.Count; index++)
                {
                    if (dbParameters[index].ParameterDirection == ParameterDirection.Output || dbParameters[index].ParameterDirection == ParameterDirection.ReturnValue)
                    {
                        var dbDataParameter = (IDbDataParameter)command.Parameters[dbParameters[index].Name];
                        RestoreParameter(dbDataParameter, dbParameters[index]);
                    }
                }
            }
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
        protected DbActionResult<T> ActionWithConnectionHandler<T>(Func<T> action, string commandText, DBParameterCollection parameters, DbActionOptions option)
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
                        Command = commandText,
                        Parameters = parameters,
                        Options = option,
                    };
                }
                catch (Exception ex)
                {
                    throw new AncestorException
                    {
                        Exception = ex,
                        CommandText = commandText,
                        Parameters = parameters,
                        Options = option,
                    };
                }
                finally
                {
                    CloseConnection();
                }
            }
        }
        protected void Log(string action, string sql, IEnumerable<DBParameter> parameters)
        {
            var message = string.Format("sql=\"{1}\" args=[{2}]", action, sql, string.Join(",", parameters));
            GlobalSetting.Log(_dao, GetType().Name, action, message);
        }
        private void OpenConnection()
        {
            OpenConnection(_connection);
        }
        private void CloseConnection()
        {
            if (_autoCloseConnection)
            {
                CloseConnection(_connection, _transaction);
            }
        }
        private bool IsDbNull(object value)
        {
            return value == null || value.GetType() == typeof(DBNull);
        }
        private static void CloseConnection(IDbConnection connection, IDbTransaction transaction)
        {
            if (transaction != null)
            {
                if (connection.State.HasFlag(ConnectionState.Open))
                    connection.Close();
            }
        }
        private static void OpenConnection(IDbConnection connection)
        {
            if (connection.State == ConnectionState.Closed)
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
                CloseConnection(_connection, null);
        }


        #endregion
    }
}

using Ancestor.Core;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace Ancestor.DataAccess.DBAction
{
    public interface IDbAction : IDisposable
    {
        bool IsTransacting { get; }
        bool AutoCloseConnection { get; set; }
        IDbConnection Connection { get; }
        void BeginTransaction();
        void BeginTransaction(IsolationLevel isolationLevel);
        void Commit();
        void Rollback();
        void OpenConnection();
        void CloseConnection();

        DbActionOptions CreateOptions();

        DbActionResult<IList> Query(string sql, DBParameterCollection dbParameters, Type dataType, DbActionOptions options = null);
        DbActionResult<object> QueryFirst(string sql, DBParameterCollection dbParameters, Type dataType, DbActionOptions options = null);
        DbActionResult<DataTable> Query(string sql, DBParameterCollection dbParameters, DbActionOptions options = null);
        DbActionResult<DataTable> QueryFirst(string sql, DBParameterCollection dbParameters, DbActionOptions options = null);
        DbActionResult<int> ExecuteNonQuery(string sql, DBParameterCollection dbParameters, DbActionOptions options = null);
        DbActionResult<object> ExecuteStoreProcedure(string name, DBParameterCollection dbParameter, DbActionOptions options = null);
        DbActionResult<object> ExecuteScalar(string sql, DBParameterCollection dbParameters, DbActionOptions options = null);
    }

    public abstract class DbActionOptions
    {
        private readonly Dictionary<string, object> _storages
            = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
        public object this[string key]
        {
            get
            {
                object value;
                _storages.TryGetValue(key, out value);
                return value;
            }
            set
            {
                if (_storages.ContainsKey(key))
                    _storages[key] = value;
                else
                    _storages.Add(key, value);
            }
        }

        public T Get<T>(string key, T defaultValue = default(T))
        {
            try
            {
                return (T)this[key];
            }
            catch
            {
                return defaultValue;
            }
        }

    }

}

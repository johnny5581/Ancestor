using Ancestor.Core;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Ancestor.DataAccess.DAO
{
    public interface IDataAccessObjectEx : IDisposable
    {
        #region Property
        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        IDbConnection DBConnection { get; }

        string ParameterPrefix { get; set; }
        string ParameterPostfix { get; set; }
        bool IsTransacting { get; }
        #endregion Property

        #region Connection
        void BeginTransaction();
        void BeginTransaction(IsolationLevel isoLationLevel);
        void Commit();
        void Rollback();
        void Open();
        void Close();

        IDataAccessObjectEx Clone();
        #endregion Connection


        #region Query
        AncestorResult QueryFromSqlString(string sql, object parameter, Type dataType, bool firstOnly, AncestorOptions options);     
        AncestorResult QueryFromModel(object model, Type dataType, object origin, bool firstOnly, AncestorOptions options);
        AncestorResult QueryFromLambda(LambdaExpression predicate, LambdaExpression selector, IDictionary<Type, object> proxyMap, bool firstOnly, AncestorOptions options);
        AncestorResult GroupFromLambda(LambdaExpression predicate, LambdaExpression selector, LambdaExpression groupBy, IDictionary<Type, object> proxyMap, AncestorOptions options);
        #endregion

        #region Insert
        AncestorExecuteResult InsertEntity(object model, object origin, AncestorOptions options);
        AncestorExecuteResult BulkInsertEntities<T>(IEnumerable<T> models, object origin, AncestorOptions options);
        #endregion Insert

        #region Update
        AncestorExecuteResult UpdateEntity(object model, object whereObject, UpdateMode mode, object origin, int exceptRows, AncestorOptions options);
        AncestorExecuteResult UpdateEntity(object model, LambdaExpression predicate, UpdateMode mode, object origin, int exceptRows, AncestorOptions options);        
        #endregion Update


        #region Delete 
        AncestorExecuteResult DeleteEntity(object whereObject, object origin, int exceptRows, AncestorOptions options);
        AncestorExecuteResult DeleteEntity(LambdaExpression predicate, object origin, int exceptRows, AncestorOptions options);        
        #endregion Delete

        #region Execute
        AncestorExecuteResult ExecuteNonQuery(string sql, object parameter, int exceptRows, AncestorOptions options);
        AncestorExecuteResult ExecuteStoredProcedure(string name, object parameter, AncestorOptions options);
        AncestorExecuteResult ExecuteScalar(string sql, object parameter, AncestorOptions options);        
        #endregion
    }
    /// <summary>
    /// DataAccessObject command options
    /// </summary>
    public class AncestorOptions
    {
        public AncestorOptions()
        {
            HasRowId = false;
            BulkStopWhenError = false;
            IgnoreNullCondition = true;
        }
        /// <summary>
        /// Allow append RowID (Oracle)
        /// </summary>
        public bool HasRowId { get; set; }
        /// <summary>
        /// Bind parameter by name(Oracle)
        /// </summary>
        public bool BindByName { get; set; }
        /// <summary>
        /// Stop bulk insert when insert fail
        /// </summary>
        public bool BulkStopWhenError { get; set; }
        /// <summary>
        /// Ignore null condition in predicate
        /// </summary>
        public bool IgnoreNullCondition { get; set; }

    }
}

using Ancestor.Core;
using Ancestor.DataAccess.DBAction;
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
        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        IDbAction DbAction { get; }
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
        AncestorResult QueryFromSqlString(string sql, object parameter, Type dataType, bool firstOnly, AncestorOption option);
        AncestorResult QueryFromModel(object model, Type dataType, object origin, bool firstOnly, AncestorOrderOption orderOpt, AncestorOption option);
        AncestorResult QueryFromLambda(LambdaExpression predicate, LambdaExpression selector, IDictionary<Type, object> proxyMap, bool firstOnly, AncestorOrderOption orderOpt, AncestorOption option);
        AncestorResult GroupFromLambda(LambdaExpression predicate, LambdaExpression selector, LambdaExpression groupBy, IDictionary<Type, object> proxyMap, AncestorOption option);
        #endregion

        #region Insert
        AncestorExecuteResult InsertEntity(object model, object origin, AncestorOption option);
        AncestorExecuteResult BulkInsertEntities<T>(IEnumerable<T> models, object origin, AncestorOption options);
        #endregion Insert

        #region Update
        AncestorExecuteResult UpdateEntity(object model, object whereObject, UpdateMode mode, object origin, int exceptRows, AncestorOption option);
        AncestorExecuteResult UpdateEntity(object model, LambdaExpression predicate, UpdateMode mode, object origin, int exceptRows, AncestorOption option);
        AncestorExecuteResult UpdateEntityRef(object model, object whereObject, object refModel, object origin, int exceptRows, AncestorOption option);
        AncestorExecuteResult UpdateEntityRef(object model, LambdaExpression predicate, object refModel, object origin, int exceptRows, AncestorOption option);
        #endregion Update

        #region Delete 
        AncestorExecuteResult DeleteEntity(object whereObject, object origin, int exceptRows, AncestorOption option);
        AncestorExecuteResult DeleteEntity(LambdaExpression predicate, object origin, int exceptRows, AncestorOption option);
        #endregion Delete

        #region Execute
        AncestorExecuteResult ExecuteNonQuery(string sql, object parameter, int exceptRows, AncestorOption option);
        AncestorExecuteResult ExecuteStoredProcedure(string name, object parameter, AncestorOption option);
        AncestorExecuteResult ExecuteScalar(string sql, object parameter, AncestorOption option);
        AncestorExecuteResult GetSequenceValue(string name, bool moveToNext, AncestorOption option);
        #endregion
    }
    /// <summary>
    /// DataAccessObject command options
    /// </summary>
    public class AncestorOption : Dictionary<string, object>
    {
        public AncestorOption() : base(StringComparer.OrdinalIgnoreCase)
        {
            BulkStopWhenError = false;
            IgnoreNullCondition = true;
        }
        /// <summary>
        /// Stop bulk insert when insert fail
        /// </summary>
        public bool BulkStopWhenError { get; set; }
        /// <summary>
        /// Ignore null condition in predicate
        /// </summary>
        public bool IgnoreNullCondition { get; set; }
    }
    /// <summary>
    /// DataAccessObject query order options
    /// </summary>
    public class AncestorOrderOption
    {
        private readonly bool _desc;
        private int _orderType;
        private object _order;

        public int OrderType
        {
            get { return _orderType; }
        }

        public object OrderItem
        {
            get { return _order; }
        }
        public bool IsDescending
        {
            get { return _desc; }
        }

        protected AncestorOrderOption(bool desc = false)
        {
            _desc = desc;
        }

        public AncestorOrderOption(string[] fields, bool desc = false) : this(desc)
        {
            _orderType = 1;
            _order = fields;
        }
        public AncestorOrderOption(LambdaExpression orderExp, bool desc = false) : this(desc)
        {
            _orderType = 2;
            _order = orderExp;
        }
        public AncestorOrderOption(object model, bool desc = false) : this(desc)
        {
            _orderType = 9;
            _order = model;
        }



    }
}

using Ancestor.Core;
using Ancestor.DataAccess.DBAction;
using Ancestor.DataAccess.Factory;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;

namespace Ancestor.DataAccess.DAO
{
    /// <summary>
    /// DataAccessObject base class
    /// </summary>
    public abstract class DataAccessObjectBase : IDataAccessObjectEx, IInternalDataAccessObject, IIdentifiable
    {
        private readonly Guid _id = Guid.NewGuid();
        private IDbAction _dbAction;
        private bool _disposed = false;
        private bool? _raiseExp;
        public const string DefaultParameterPrefix = "P_";
        public const string DefaultParameterPostfix = "_P";
        public const string DefaultUpdateParameterPrefix = "U_";
        public const string DefaultMergeParameterPostfix = "_R";
        private string _parameterPrefix;
        private string _updateParameterPrefix;
        private string _parameterPostfix;
        private string _mergeParameterPostfix;

        private DAOFactoryEx _factory;

        public DataAccessObjectBase(DAOFactoryEx factory)
        {
            _factory = factory;
        }


        #region Property

        public string ParameterPrefix
        {
            get { return _parameterPrefix ?? DefaultParameterPrefix; }
            set { _parameterPrefix = value; }
        }
        public string UpdateParameterPrefix
        {
            get { return _updateParameterPrefix ?? DefaultUpdateParameterPrefix; }
            set { _updateParameterPrefix = value; }
        }
        public string ParameterPostfix
        {
            get { return _parameterPostfix ?? DefaultParameterPostfix; }
            set { _parameterPostfix = value; }
        }
        public string MergeParameterPostfix
        {
            get { return _mergeParameterPostfix ?? DefaultMergeParameterPostfix; }
            set { _mergeParameterPostfix = value; }
        }

        public bool IsTransacting
        {
            get { return DbAction.IsTransacting; }
        }
        Guid IIdentifiable.Guid
        {
            get { return _id; }
        }
        /// <summary>
        /// Parameter symbol string
        /// </summary>
        public abstract string ParameterSymbol { get; }
        /// <summary>
        /// Connection symbol string
        /// </summary>
        public abstract string ConnectorSymbol { get; }
        /// <summary>
        /// Condition FALSE sql string
        /// </summary>
        public virtual string ConditionFALSE
        {
            get { return " 1 <> 1 "; }
        }
        /// <summary>
        /// Dummy table name
        /// </summary>
        public virtual string DummyTable
        {
            get { return "Dual"; }
        }
        /// <summary>
        /// Date time symbol
        /// </summary>
        public abstract string DateTimeSymbol { get; }

        /// <summary>
        /// Update mode setting
        /// </summary>
        public UpdateMode UpdateMode { get; set; }
        /// <summary>
        /// Flag to raise exception if error
        /// </summary>
        public bool RaiseException
        {
            get { return _raiseExp ?? AncestorGlobalOptions.GetBoolean("ThrowOnError"); }
            set { _raiseExp = value; }
        }

        IDbConnection IDataAccessObjectEx.DBConnection
        {
            get { return DbAction.Connection; }
        }
        public IDbAction DbAction
        {
            get { return _dbAction ?? (_dbAction = CreateDbAction()); }
        }

        public DAOFactoryEx Factory
        {
            get { return _factory; }
        }
        #endregion Property

        IDataAccessObjectEx IDataAccessObjectEx.Clone()
        {
            // recreate from factory
            return new DAOFactoryEx(Factory).GetDataAccessObjectFactory();
        }
        protected abstract IDbAction CreateDbAction(DBObject dbObject);
        protected abstract IDbAction CreateDbAction(string connStr);
        protected abstract IDbAction CreateDbAction(IDbConnection conn);
        protected virtual IDbAction CreateDbAction()
        {
            var factory = Factory;
            switch (factory.Mode)
            {
                case DAOFactoryEx.SourceMode.DBObject:
                    return CreateDbAction((DBObject)factory.Source);
                case DAOFactoryEx.SourceMode.ConnectionString:
                    return CreateDbAction((string)factory.Source);
                case DAOFactoryEx.SourceMode.Connection:
                    return CreateDbAction((IDbConnection)factory.Source);
                default:
                    if (factory.Database == DBObject.DataBase.Custom)
                        return factory.CustomDbFactory(factory, this);
                    throw new InvalidOperationException("invalid factory mode:" + factory.Mode);
            }
        }
        protected DbActionOptions CreateDbOptions(AncestorOption options)
        {
            return CreateDbOptions(options ?? new AncestorOption(), DbAction.CreateOptions());
        }
        protected virtual DbActionOptions CreateDbOptions(AncestorOption options, DbActionOptions dbOptions)
        {
            if (options != null)
                dbOptions.Parse(options);
            return dbOptions;
        }
        protected abstract ExpressionResolver CreateExpressionResolver(ReferenceInfo reference, ExpressionResolver.ExpressionResolveOption option);
        protected TResult TryCatch<T, TResult>(Func<T> action, Func<object, TResult> resultFactory, int exceptRows) where TResult : AncestorResult
        {
            try
            {
                object data = action();
                if (exceptRows != -1)
                {
                    int actualRows = -1;
                    if (data is int)
                        actualRows = (int)data;
                    else if (data is DbActionResult<int>)
                        actualRows = ((DbActionResult<int>)data).Result;

                    if (actualRows != exceptRows)
                        throw new AncestorException(90001, "effect rows(" + data + ") not except(" + exceptRows + ")");
                }
                return resultFactory(data);
            }
            catch (AncestorException ex)
            {
                var code = ex.Code;
                var innerException = ex.InnerException;
                if (RaiseException)
                    throw innerException;
                var result = (TResult)Activator.CreateInstance(typeof(TResult), innerException);
                result.QueryParameter = QueryParameter.Parse(ex.Data["QueryParameter"]);
                return result;
            }
            catch (Exception ex)
            {
                if (RaiseException)
                    throw;
                return (TResult)Activator.CreateInstance(typeof(TResult), ex);
            }
        }
        protected TResult TryCatch<T, TResult>(Func<T> action, Func<object, TResult> resultFactory) where TResult : AncestorResult
        {
            try
            {
                var data = action();
                return resultFactory(data);
            }
            catch (AncestorException ex)
            {
                var code = ex.Code;
                var innerException = ex.InnerException;
                if (RaiseException)
                    throw innerException;
                var result = (TResult)Activator.CreateInstance(typeof(TResult), innerException);
                result.QueryParameter = QueryParameter.Parse(ex.Data["QueryParameter"]);
                return result;
            }
            catch (Exception ex)
            {
                if (RaiseException)
                    throw;
                return (TResult)Activator.CreateInstance(typeof(TResult), ex);
            }
        }
        protected AncestorResult ReturnAncestorResult(object result)
        {
            QueryParameter parameters = new QueryParameter();
            var dbResult = result as DbActionResult;
            if (dbResult != null)
            {
                result = dbResult.Result;
                parameters = dbResult.Parameter;
            }

            var list = result as IList;
            if (list != null)
                return new AncestorResult(list) { QueryParameter = parameters };

            var table = result as DataTable;
            if (table != null)
                return new AncestorResult(table) { QueryParameter = parameters };

            if (result is int)
                return new AncestorResult((int)result) { QueryParameter = parameters };

            if (result != null)
            {
                var anonymousList = Activator.CreateInstance(typeof(List<>).MakeGenericType(result.GetType())) as IList;
                anonymousList.Add(result);
                return new AncestorResult(anonymousList) { QueryParameter = parameters };
            }
            else
            {
                var emptyList = new List<object>();
                return new AncestorResult(emptyList) { QueryParameter = parameters };
            }
        }
        protected AncestorExecuteResult ReturnEffectRowResult(object result)
        {
            QueryParameter parameters = new QueryParameter();
            var dbResult = result as DbActionResult;
            if (dbResult != null)
            {
                result = dbResult.Result;
                parameters = dbResult.Parameter;
            }
            if (result is int)
                return new AncestorExecuteResult((int)result, parameters.Parameters) { QueryParameter = parameters };
            return new AncestorExecuteResult(result, parameters.Parameters) { QueryParameter = parameters };
        }

        protected AncestorExecuteResult ReturnAncestorExecuteResult(object result)
        {
            QueryParameter parameters = new QueryParameter();
            var dbResult = result as DbActionResult;
            if (dbResult != null)
            {
                result = dbResult.Result;
                parameters = dbResult.Parameter;
            }
            return new AncestorExecuteResult(result, parameters.Parameters) { QueryParameter = parameters };
        }
        public void BeginTransaction()
        {
            DbAction.BeginTransaction();
        }

        public void BeginTransaction(IsolationLevel isoLationLevel)
        {
            DbAction.BeginTransaction(isoLationLevel);
        }

        public void Commit()
        {
            DbAction.Commit();
        }

        public void Rollback()
        {
            DbAction.Rollback();
        }

        public void Open()
        {
            DbAction.OpenConnection();
        }

        public void Close()
        {
            DbAction.CloseConnection();
        }
        public virtual AncestorResult QueryFromSqlString(string sql, object parameter, Type dataType, bool firstOnly, AncestorOption option)
        {
            return TryCatch(() =>
            {
                var dbParameters = CreateDBParameters(parameter, option);
                if (option == null)
                    option = new AncestorOption { };
                var dbOpts = CreateDbOptions(option);
                return InternalQuery(sql, dbParameters, dataType, firstOnly, dbOpts);
            }, ReturnAncestorResult);
        }

        public virtual AncestorResult QueryFromModel(object model, Type dataType, object origin, bool firstOnly, AncestorOrderOption orderOpt, AncestorOption option)
        {
            return TryCatch(() =>
            {
                var dbParameters = new DBParameterCollection();
                var reference = GetReferenceInfo(model, null, dataType, origin);
                var selector = CreateSelectCommand(reference);
                var tableName = reference.GetReferenceName();
                var ignoreNull = true;
                if (option != null)
                    ignoreNull = option.IgnoreNullCondition;
                var where = CreateWhereCommand(model, tableName, ignoreNull, dbParameters);
                var order = CreateOrderCommand(orderOpt);
                var opt = CreateDbOptions(option);
                var sql = string.Format("Select {0} From {1} {2} {3}", selector, tableName, where, order);
                return InternalQuery(sql, dbParameters, dataType, firstOnly, opt);
            }, ReturnAncestorResult);
        }

        public virtual AncestorResult QueryFromLambda(LambdaExpression predicate, LambdaExpression selector, IDictionary<Type, object> proxyMap, bool firstOnly, AncestorOrderOption orderOpt, AncestorOption option)
        {
            return TryCatch(() =>
            {
                var predicateParameterTypes = predicate == null ? new Type[0] : predicate.Parameters.Select(p => p.Type);
                var selectorParameterTypes = selector == null ? new Type[0] : selector.Parameters.Select(p => p.Type);
                var parameterTypes = predicateParameterTypes.Concat(selectorParameterTypes).Distinct();
                var reference = GetReferenceInfo(parameterTypes, proxyMap);

                ExpressionResolver.ExpressionResolveResult predicateResult = null;
                ExpressionResolver.ExpressionResolveResult selectorResult = null;
                if (predicate != null)
                {
                    var resolver = CreateExpressionResolver(reference, null);
                    predicateResult = resolver.Resolve(predicate);
                }
                if (selector != null)
                {
                    var selectResolver = CreateExpressionResolver(reference, ExpressionResolver.ExpressionResolveOption.Selector);
                    selectorResult = selectResolver.Resolve(selector);
                }
                if (predicateResult == null && selectorResult == null)
                    throw new ArgumentNullException("no predicate or selector");

                var mergeResult = ExpressionResolver.CombineResult(selectorResult, predicateResult);

                var selectorText = mergeResult.Sql1;
                var tuples = mergeResult.Reference.GetStructs();
                var tableText = string.Join(", ", tuples.Select(r => GetReferenceStructName(r)));
                if (selectorText == null)
                {
                    selectorText = CreateSelectCommand(mergeResult.Reference);
                }

                var whereText = mergeResult.Sql2 != null ? ("Where " + mergeResult.Sql2) : "";
                var orderText = CreateOrderCommand(orderOpt, reference);
                var sql = string.Format("Select {0} From {1} {2} {3}", selectorText, tableText, whereText, orderText);
                var dataType = selector == null ? mergeResult.Reference.GetReferenceType() : null;
                var opt = CreateDbOptions(option);
                return InternalQuery(sql, mergeResult.Parameters, dataType, firstOnly, opt);
            }, ReturnAncestorResult);
        }

        public AncestorResult GroupFromLambda(LambdaExpression predicate, LambdaExpression selector, LambdaExpression groupBy, IDictionary<Type, object> proxyMap, AncestorOption option)
        {
            return TryCatch(() =>
            {
                var predicateParameterTypes = predicate == null ? new Type[0] : predicate.Parameters.Select(p => p.Type);
                var selectorParameterTypes = selector == null ? new Type[0] : selector.Parameters.Select(p => p.Type);
                var parameterTypes = predicateParameterTypes.Concat(selectorParameterTypes).Distinct();
                var reference = GetReferenceInfo(parameterTypes, proxyMap);

                ExpressionResolver.ExpressionResolveResult predicateResult = null;
                ExpressionResolver.ExpressionResolveResult selectorResult = null;
                ExpressionResolver.ExpressionResolveResult groupResult = null;
                if (groupBy != null)
                {
                    var grpResolver = CreateExpressionResolver(reference, ExpressionResolver.ExpressionResolveOption.GroupBy);
                    groupResult = grpResolver.Resolve(groupBy);
                }
                if (predicate != null)
                {
                    var resolver = CreateExpressionResolver(reference, null);
                    predicateResult = resolver.Resolve(predicate);
                }
                if (selector == null)
                    throw new ArgumentNullException("no selector statement");
                var selectResolver = CreateExpressionResolver(reference, ExpressionResolver.ExpressionResolveOption.Selector);
                selectorResult = selectResolver.Resolve(selector);

                var mergeResult = ExpressionResolver.CombineResult(selectorResult, predicateResult);

                var selectorText = mergeResult.Sql1;
                var tuples = mergeResult.Reference.GetStructs();
                var tableText = string.Join(", ", tuples.Select(r => GetReferenceStructName(r)));
                if (selectorText == null)
                {
                    selectorText = CreateSelectCommand(mergeResult.Reference);
                }

                var whereText = mergeResult.Sql2 != null ? ("Where " + mergeResult.Sql2) : "";
                var groupText = groupResult != null ? groupResult.Sql : selectorResult.GroupBy;
                var sql = string.Format("Select {0} From {1} {2} Group By {3}", selectorText, tableText, whereText, groupText);
                var dataType = mergeResult.Reference.GetReferenceType();
                var opt = CreateDbOptions(option);
                return InternalQuery(sql, mergeResult.Parameters, null, false, opt);
            }, ReturnAncestorResult);
        }

        public AncestorExecuteResult InsertEntity(object model, object origin, AncestorOption options)
        {
            return TryCatch(() =>
            {
                var dbParameters = new DBParameterCollection();
                var reference = GetReferenceInfo(model, null, null, origin);
                var name = reference.GetReferenceName();
                var insertInfo = CreateInsertCommand(reference, model, dbParameters);
                var fields = insertInfo.Item1;
                if (!string.IsNullOrEmpty(fields))
                    fields = "(" + fields + ")";
                var values = insertInfo.Item2;
                var sql = string.Format("Insert Into {0} {1} Values ({2})", name, fields, values);
                var opt = CreateDbOptions(options);
                return DbAction.ExecuteNonQuery(sql, dbParameters, opt);
            }, ReturnEffectRowResult);
        }

        public AncestorBulkExecuteResult BulkInsertEntities<T>(IEnumerable<T> models, object origin, AncestorOption options)
        {
            return TryCatch(() =>
            {
                var reference = GetReferenceInfo(null, typeof(T), null, origin);
                var insertInfos = CreateInsertCommands(reference, models, typeof(T));
                var name = reference.GetReferenceName();
                var connCloseFlag = DbAction.AutoCloseConnection;
                var raiseError = false;
                if (options != null)
                    raiseError = options.BulkStopWhenError;
                var successed = 0;
                string field = null;
                var transacting = false;
                var bulked = false;
                var faileds = new Dictionary<T, Exception>();
                try
                {
                    // if transaction isn't begin, begin it
                    if (!DbAction.IsTransacting)
                    {
                        DbAction.BeginTransaction();
                        transacting = true;
                    }
                    //DbAction.OpenConnection();
                    var index = 0;
                    foreach (var insertInfo in insertInfos)
                    {
                        if (field == null)
                        {
                            field = insertInfo.Item1 ?? "";
                            if (!string.IsNullOrEmpty(field))
                                field = "(" + field + ")";
                        }
                        string insertSql = string.Format("Insert Into {0} {1} Values ({2})", name, field, insertInfo.Item2);
                        try
                        {
                            DbAction.ExecuteNonQuery(insertSql, insertInfo.Item3);
                            successed++;
                        }
                        catch (Exception ex)
                        {
                            if (raiseError)
                                throw;
                            faileds.Add(models.ElementAt(index), ex);
                        }
                        index++;
                    }
                    bulked = true;
                    return Tuple.Create(successed, faileds);
                }
                finally
                {
                    if (transacting)
                    {
                        if (bulked)
                            DbAction.Commit();
                        else
                            DbAction.Rollback();
                    }
                }
            }, result =>
            {
                QueryParameter parameters = new QueryParameter();
                var dbResult = result as DbActionResult;
                if (dbResult != null)
                {
                    result = dbResult.Result;
                    parameters = dbResult.Parameter;
                }
                if (result is int)
                    return new AncestorBulkExecuteResult((int)result, parameters.Parameters) { QueryParameter = parameters };

                var tuple = result as Tuple<int, Dictionary<T, Exception>>;
                if (tuple != null)                
                    return new AncestorBulkExecuteResult(tuple.Item1, tuple.Item2.ToDictionary(r => (object)r.Key, r => r.Value), parameters.Parameters) { QueryParameter = parameters };

                return new AncestorBulkExecuteResult(result, parameters.Parameters) { QueryParameter = parameters };
            });
        }
        public AncestorExecuteResult UpdateEntity(object model, object whereObject, UpdateMode mode, object origin, int exceptRows, AncestorOption options)
        {
            return TryCatch(() =>
            {
                var dbParameters = new DBParameterCollection();
                var reference = GetReferenceInfo(model, null, null, origin);
                var updateCommand = CreateUpdateCommand(reference, model, mode, dbParameters);
                var name = reference.GetReferenceName();
                var ignoreNull = true;
                if (options != null)
                    ignoreNull = options.IgnoreNullCondition;
                var whereCommand = CreateWhereCommand(whereObject, null, ignoreNull, dbParameters);
                var sql = string.Format("Update {0} Set {1} {2}", name, updateCommand, whereCommand);
                var opt = CreateDbOptions(options);
                return DbAction.ExecuteNonQuery(sql, dbParameters, opt);
            }, ReturnEffectRowResult, exceptRows);
        }
        public AncestorExecuteResult UpdateEntity(object model, LambdaExpression predicate, UpdateMode mode, object origin, int exceptRows, AncestorOption options)
        {
            return TryCatch(() =>
            {
                var dbParameters = new DBParameterCollection();
                var reference = GetReferenceInfo(model, null, predicate.Parameters[0].Type, origin);
                var updateCommand = CreateUpdateCommand(reference, model, mode, dbParameters);
                var name = reference.GetReferenceName();
                ExpressionResolver.ExpressionResolveResult result = null;
                if (predicate != null)
                {
                    var resolver = CreateExpressionResolver(reference, null);
                    result = resolver.Resolve(predicate);
                    dbParameters.AddRange(result.Parameters);
                }

                var whereCommand = result != null ? "Where " + result.Sql : "";
                var opt = CreateDbOptions(options);
                var sql = string.Format("Update {0} Set {1} {2}", name, updateCommand, whereCommand);
                return DbAction.ExecuteNonQuery(sql, dbParameters, opt);
            }, ReturnEffectRowResult, exceptRows);
        }
        public AncestorExecuteResult UpdateEntityRef(object model, object whereObject, object refModel, object origin, int exceptRows, AncestorOption options)
        {
            return TryCatch(() =>
            {
                var dbParameters = new DBParameterCollection();
                var modelType = (model ?? refModel).GetType();
                var reference = GetReferenceInfo(model, modelType, null, origin);
                var updateCommand = CreateUpdateCommand(reference, model, refModel, modelType, dbParameters);
                var name = reference.GetReferenceName();
                var ignoreNull = true;
                if (options != null)
                    ignoreNull = options.IgnoreNullCondition;
                var whereCommand = CreateWhereCommand(whereObject, null, ignoreNull, dbParameters);
                var sql = string.Format("Update {0} Set {1} {2}", name, updateCommand, whereCommand);
                var opt = CreateDbOptions(options);
                return DbAction.ExecuteNonQuery(sql, dbParameters, opt);
            }, ReturnEffectRowResult, exceptRows);
        }
        public AncestorExecuteResult UpdateEntityRef(object model, LambdaExpression predicate, object refModel, object origin, int exceptRows, AncestorOption options)
        {
            return TryCatch(() =>
            {
                var dbParameters = new DBParameterCollection();
                var modelType = (model ?? refModel).GetType();
                var reference = GetReferenceInfo(model, modelType, null, origin);
                var updateCommand = CreateUpdateCommand(reference, model, refModel, modelType, dbParameters);
                var name = reference.GetReferenceName();
                ExpressionResolver.ExpressionResolveResult result = null;
                if (predicate != null)
                {
                    var resolver = CreateExpressionResolver(reference, null);
                    result = resolver.Resolve(predicate);
                    dbParameters.AddRange(result.Parameters);
                }

                var whereCommand = result != null ? "Where " + result.Sql : "";
                var opt = CreateDbOptions(options);
                var sql = string.Format("Update {0} Set {1} {2}", name, updateCommand, whereCommand);
                return DbAction.ExecuteNonQuery(sql, dbParameters, opt);
            }, ReturnEffectRowResult, exceptRows);
        }
        public AncestorExecuteResult DeleteEntity(object whereObject, object origin, int exceptRows, AncestorOption options)
        {
            return TryCatch(() =>
            {
                var dbParameters = new DBParameterCollection();
                var reference = GetReferenceInfo(whereObject, null, null, origin);
                var tableName = reference.GetReferenceName();
                var ignoreNull = true;
                if (options != null)
                    ignoreNull = options.IgnoreNullCondition;
                var whereCommand = CreateWhereCommand(whereObject, null, ignoreNull, dbParameters);
                var opt = CreateDbOptions(options);
                var sql = string.Format("Delete From {0} {1}", tableName, whereCommand);
                return DbAction.ExecuteNonQuery(sql, dbParameters, opt);
            }, ReturnEffectRowResult, exceptRows);
        }
        public AncestorExecuteResult DeleteEntity(LambdaExpression predicate, object origin, int exceptRows, AncestorOption options)
        {
            return TryCatch(() =>
            {
                var dbParameters = new DBParameterCollection();
                var reference = GetReferenceInfo(null, null, predicate.Parameters[0].Type, origin);
                var name = reference.GetReferenceName();
                var ignoreNull = true;
                if (options != null)
                    ignoreNull = options.IgnoreNullCondition;
                ExpressionResolver.ExpressionResolveResult result = null;
                if (predicate != null)
                {
                    var resolver = CreateExpressionResolver(reference, null);
                    result = resolver.Resolve(predicate);
                    dbParameters.AddRange(result.Parameters);
                }

                var whereCommand = result != null ? "Where " + result.Sql : "";
                var opt = CreateDbOptions(options);
                var sql = string.Format("Delete From {0} {1}", name, whereCommand);
                return DbAction.ExecuteNonQuery(sql, dbParameters, opt);
            }, ReturnEffectRowResult, exceptRows);
        }
        public AncestorExecuteResult ExecuteNonQuery(string sql, object parameter, int exceptRows, AncestorOption options)
        {
            return TryCatch(() =>
            {
                var dbParameters = CreateDBParameters(parameter, options);
                var dbOpt = CreateDbOptions(options);
                return DbAction.ExecuteNonQuery(sql, dbParameters, dbOpt);
            }, ReturnEffectRowResult, exceptRows);
        }

        public AncestorExecuteResult ExecuteStoredProcedure(string name, object parameter, AncestorOption options)
        {
            return TryCatch(() =>
            {
                var dbParameters = CreateDBParameters(parameter, options);
                var dbOpt = CreateDbOptions(options);
                return DbAction.ExecuteStoreProcedure(name, dbParameters, dbOpt);
            }, ReturnAncestorExecuteResult);
        }
        public AncestorExecuteResult ExecuteScalar(string sql, object parameter, AncestorOption options)
        {
            return TryCatch(() =>
            {
                var dbParameters = CreateDBParameters(parameter, options);
                var dbOpt = CreateDbOptions(options);
                return DbAction.ExecuteScalar(sql, dbParameters, dbOpt);
            }, ReturnAncestorExecuteResult);
        }

        public AncestorExecuteResult GetSequenceValue(string name, bool moveToNext, AncestorOption options)
        {
            return TryCatch(() =>
            {
                var sql = GetSequenceCommand(name, moveToNext);
                var dbParameters = CreateDBParameters(null, options);
                var dbOpt = CreateDbOptions(options);
                return DbAction.ExecuteScalar(sql, dbParameters, dbOpt);
            }, ReturnAncestorExecuteResult);
        }

        protected abstract string GetSequenceCommand(string name, bool moveToNext);

        protected DBParameterCollection CreateDBParameters(object parameterObject, AncestorOption options)
        {
            var parameters = parameterObject as DBParameterCollection;
            if (parameters != null)
                return parameters;

            var parameterEnumerable = parameterObject as IEnumerable<DBParameter>;
            if (parameterEnumerable != null)
                return new DBParameterCollection(parameterEnumerable);

            parameters = new DBParameterCollection();
            var parameterDictionary = parameterObject as IDictionary<string, object>;
            if (parameterDictionary != null) // is dictionary 
                CreateDBParameterFromDictionary(parameterDictionary, ref parameters, options);
            else if (parameterObject != null)
                CreateDBParameterFromProperty(parameterObject, ref parameters, options);
            return parameters;
        }


        protected virtual ParameterInfo CreateParameter(object value, string parameterNameSeed, bool symbol, string prefix = null, string postfix = null, HardWordAttribute hardWord = null)
        {
            var pName = CreateParameterName(parameterNameSeed, symbol, prefix, postfix);
            string vName = null;
            var sysDate = false;
            if (value is DateTime && value != null && (DateTime)value == Server.SysDate)
            {
                vName = DateTimeSymbol;
                value = null;
                sysDate = true;
            }
            else if (hardWord != null && UseHardword())
            {
                vName = ConvertToHardWord(pName, hardWord);
                var str = value as string;
                if (str != null)
                {
                    value = hardWord.Encoding.GetBytes(str);
                }
            }
            return new ParameterInfo(pName, vName, value, sysDate, hardWord);
        }


        public virtual string ConvertFromHardWord(string name, HardWordAttribute attribute)
        {
            return name;
        }
        public virtual string ConvertToHardWord(string name, HardWordAttribute attribute)
        {
            return name;
        }
        protected virtual bool UseHardword()
        {
            return true;
        }
        protected string GetReferenceStructName(Tuple<Type, Type, string> tuple)
        {
            // 按照順序回傳: ProxyType => ProxyName => DataType
            if (tuple.Item2 != null)
                return TableManager.GetTableName(tuple.Item2) ?? tuple.Item2.Name;
            else if (tuple.Item3 != null)
                return tuple.Item3;
            else if (tuple.Item1 != null)
                return TableManager.GetTableName(tuple.Item1) ?? tuple.Item1.Name;
            else
                return null;
        }

        protected virtual string CreateWhereCommand(object model, string tableName, bool ignoreNull, DBParameterCollection collection)
        {
            if (model == null) return string.Empty;
            var wheres = new List<string>();
            var modelType = model.GetType();
            var infos = modelType.GetProperties().Select(r => new { Property = r, Value = r.GetValue(model, null) });
            foreach (var info in infos)
            {
                bool? nullValue = null;
                if (info.Value != null)
                    nullValue = false;
                else if (!ignoreNull)
                    nullValue = true;

                if (nullValue != null)
                {
                    var name = TableManager.GetName(info.Property);
                    var pname = CreateParameterName(name, true);
                    if (!string.IsNullOrEmpty(tableName))
                        name = string.Format("{0}.{1}", tableName, name);

                    if (nullValue.Value)
                    {
                        wheres.Add(string.Format("{0} Is Null", name));
                    }
                    else if (info.Value is DateTime && (DateTime)info.Value == Server.SysDate)
                        wheres.Add(string.Format("{0} = {1}", name, DateTimeSymbol));
                    else
                    {
                        collection.Add(pname, info.Value);
                        wheres.Add(string.Format("{0} = {1}", name, pname));
                    }
                }
            }
            var sql = string.Join(" And ", wheres);
            if (sql.Length > 0)
                sql = "Where " + sql;
            return sql;
        }
        protected virtual string CreateSelectCommand(ReferenceInfo info)
        {
            var selecteds = new Dictionary<int, string>();
            var structs = info.GetStructs();

            int index = 0;
            foreach (var item in structs)
            {
                index++;
                var referenceName = item.Item3;
                var referenceType = item.Item2 ?? item.Item1;
                if (referenceType != null)
                {
                    if (referenceName == null)
                        referenceName = TableManager.GetTableName(referenceType);
                    var hds = HardWordManager.Get(referenceType);
                    if (hds.Count > 0)
                    {
                        foreach (var hd in hds)
                        {
                            var field = TableManager.GetName(hd.Key);
                            var name = string.Format("{0}.{1}", referenceName, field);
                            name = ConvertFromHardWord(name, hd.Value);
                            selecteds.Add(index++, string.Format("{0} As {1}", name, field));
                        }
                    }
                }
                var asterisk = referenceName == null ? "*" : string.Format("{0}.*", referenceName);
                selecteds.Add(index + 100, asterisk);
            }
            return string.Join(", ", selecteds.Values);
        }
        protected virtual string CreateSelectCommand(ReferenceInfo info, Type sourceType)
        {
            var selecteds = new Dictionary<int, string>();
            var item = info.GetStructs(sourceType);
            int index = 0;
            var referenceName = item.Item3;
            var referenceType = item.Item2;
            if (referenceType != null)
            {
                if (referenceName == null)
                    referenceName = TableManager.GetTableName(referenceType);
                var hds = HardWordManager.Get(referenceType);
                if (hds.Count > 0)
                {
                    foreach (var hd in hds)
                    {
                        var field = TableManager.GetName(hd.Key);
                        var name = string.Format("{0}.{1}", referenceName, field);
                        name = ConvertFromHardWord(name, hd.Value);
                        selecteds.Add(index++, string.Format("{0} As {1}", name, field));
                    }
                }
            }
            var asterisk = referenceName == null ? "*" : string.Format("{0}.*", referenceName);
            selecteds.Add(index + 100, asterisk);

            return string.Join(", ", selecteds.Values);
        }
        protected virtual Tuple<string, string> CreateInsertCommand(ReferenceInfo info, object model, DBParameterCollection parameters)
        {
            var fields = new List<string>();
            var values = new List<string>();
            var referenceType = info.GetReferenceType();
            var dataType = model.GetType();
            var refProperties = TableManager.GetBrowsableProperties(referenceType ?? dataType);
            foreach (var refProperty in refProperties)
            {
                var dataProperty = dataType.GetProperty(refProperty.Name);
                if (dataProperty != null)
                {
                    var value = dataProperty.GetValue(model, null);
                    if (value != null)
                    {
                        var hd = HardWordManager.Get(refProperty);
                        var fname = TableManager.GetName(refProperty);
                        var parameter = CreateParameter(value, fname, true, hardWord: hd);
                        fields.Add(fname);
                        values.Add(parameter.ValueName);
                        if (!parameter.IsSysDateConverted)
                            BindParameter(parameters, parameter);
                    }
                }

            }
            return Tuple.Create(string.Join(", ", fields), string.Join(", ", values));
        }
        protected virtual IEnumerable<Tuple<string, string, DBParameterCollection>> CreateInsertCommands(ReferenceInfo info, IEnumerable models, Type dataType)
        {
            var fields = new List<string>();
            var referenceType = info.GetReferenceType();
            var refProperties = TableManager.GetBrowsableProperties(referenceType ?? dataType);
            var cache = new Dictionary<PropertyInfo, Tuple<string, HardWordAttribute>>();
            string field = null;
            foreach (var model in models)
            {
                var values = new List<string>();
                var parameters = new DBParameterCollection();
                foreach (var refProperty in refProperties)
                {
                    var dataProperty = dataType.GetProperty(refProperty.Name);
                    if (dataProperty != null)
                    {
                        var value = dataProperty.GetValue(model, null);
                        if (value != null)
                        {
                            Tuple<string, HardWordAttribute> tuple;
                            if (!cache.TryGetValue(refProperty, out tuple))
                            {
                                var hd = HardWordManager.Get(refProperty);
                                var fname = TableManager.GetName(refProperty);
                                tuple = Tuple.Create(fname, hd);
                                cache.Add(refProperty, tuple);
                                fields.Add(fname);
                            }
                            var parameter = CreateParameter(value, tuple.Item1, true, hardWord: tuple.Item2);
                            values.Add(parameter.ValueName);
                            if (!parameter.IsSysDateConverted)
                                BindParameter(parameters, parameter);
                        }
                    }
                }
                if (field == null)
                    field = string.Join(", ", fields);
                yield return Tuple.Create(field, string.Join(", ", values), parameters);
            }
        }
        protected virtual string CreateUpdateCommand(ReferenceInfo info, object model, UpdateMode mode, DBParameterCollection parameters)
        {
            var fieldMap = new Dictionary<string, string>();
            var referenceType = info.GetReferenceType();
            IDictionary<string, object> map = model as IDictionary<string, object>;
            if (map != null)
            {
                if (referenceType != null && typeof(IDictionary<string, object>).IsAssignableFrom(referenceType))
                {
                    foreach (var key in map.Keys)
                    {
                        var value = map[key];
                        var fname = key;
                        if (value != null)
                        {
                            var parameter = CreateParameter(value, fname, true, UpdateParameterPrefix);
                            fieldMap.Add(fname, parameter.ValueName);
                            if (!parameter.IsSysDateConverted)
                                BindParameter(parameters, parameter);
                        }
                        else if (mode == UpdateMode.All)
                        {
                            fieldMap.Add(fname, "NULL");
                        }
                    }
                }
                else
                {
                    var properties = TableManager.GetBrowsableProperties(referenceType);
                    foreach (var key in map.Keys)
                    {
                        var value = map[key];
                        var fname = key;
                        var property = properties.FirstOrDefault(p => string.Equals(p.Name, fname, StringComparison.OrdinalIgnoreCase));
                        HardWordAttribute hd = null;
                        if (property != null)
                            hd = HardWordManager.Get(property);

                        if (value != null)
                        {
                            var parameter = CreateParameter(value, fname, true, UpdateParameterPrefix, null, hd);
                            fieldMap.Add(fname, parameter.ValueName);
                            if (!parameter.IsSysDateConverted)
                                BindParameter(parameters, parameter);
                        }
                        else if (mode == UpdateMode.All)
                        {
                            fieldMap.Add(fname, "NULL");
                        }
                    }
                }
            }
            else if (referenceType != null)
            {
                if (referenceType != model.GetType())
                {
                    var properties = TableManager.GetBrowsableProperties(referenceType);
                    var srcProperties = model.GetType().GetProperties();
                    foreach (var property in properties)
                    {
                        var srcProperty = srcProperties.FirstOrDefault(r => r.Name == property.Name && r.PropertyType == property.PropertyType);
                        if (srcProperty != null)
                        {
                            var value = srcProperty.GetValue(model, null);
                            var fname = TableManager.GetName(property);
                            var hd = HardWordManager.Get(property);

                            if (value != null)
                            {
                                var parameter = CreateParameter(value, fname, true, UpdateParameterPrefix, null, hd);
                                fieldMap.Add(fname, parameter.ValueName);
                                if (!parameter.IsSysDateConverted)
                                    BindParameter(parameters, parameter);
                            }
                            else if (mode == UpdateMode.All)
                            {
                                fieldMap.Add(fname, "NULL");
                            }
                        }
                    }
                }
                else
                {
                    var properties = TableManager.GetBrowsableProperties(referenceType);
                    foreach (var property in properties)
                    {
                        var value = property.GetValue(model, null);
                        var fname = TableManager.GetName(property);
                        var hd = HardWordManager.Get(property);

                        if (value != null)
                        {
                            var parameter = CreateParameter(value, fname, true, UpdateParameterPrefix, null, hd);
                            fieldMap.Add(fname, parameter.ValueName);
                            if (!parameter.IsSysDateConverted)
                                BindParameter(parameters, parameter);
                        }
                        else if (mode == UpdateMode.All)
                        {
                            fieldMap.Add(fname, "NULL");
                        }
                    }
                }
            }
            else
            {
                var properties = TableManager.GetBrowsableProperties(model.GetType());
                foreach (var property in properties)
                {
                    var value = property.GetValue(model, null);
                    var fname = TableManager.GetName(property);
                    var hd = HardWordManager.Get(property);

                    if (value != null)
                    {
                        var parameter = CreateParameter(value, fname, true, UpdateParameterPrefix, null, hd);
                        fieldMap.Add(fname, parameter.ValueName);
                        if (!parameter.IsSysDateConverted)
                            BindParameter(parameters, parameter);
                    }
                    else if (mode == UpdateMode.All)
                    {
                        fieldMap.Add(fname, "NULL");
                    }
                }
            }

            return string.Join(", ", fieldMap.Select(kv => string.Format("{0} = {1}", kv.Key, kv.Value)));
        }
        protected virtual string CreateUpdateCommand(ReferenceInfo info, object model, object refModel, Type modelType, DBParameterCollection parameters)
        {
            var refProperties = TableManager.GetBrowsableProperties(modelType);
            var fieldMap = new Dictionary<string, string>();
            foreach (var refProperty in refProperties)
            {
                var value = refProperty.GetValue(model, null);
                var valueRef = refProperty.GetValue(refModel, null);
                if (value != valueRef)
                {
                    var fname = TableManager.GetName(refProperty);
                    var hd = HardWordManager.Get(refProperty);
                    if (value != null)
                    {
                        var parameter = CreateParameter(value, fname, true, UpdateParameterPrefix, null, hd);
                        fieldMap.Add(fname, parameter.ValueName);
                        if (!parameter.IsSysDateConverted)
                            BindParameter(parameters, parameter);
                    }
                    else
                    {
                        fieldMap.Add(fname, "NULL");
                    }
                }
            }
            return string.Join(", ", fieldMap.Select(kv => string.Format("{0} = {1}", kv.Key, kv.Value)));
        }
        protected virtual string CreateOrderCommand(AncestorOrderOption opt, ReferenceInfo reference = null)
        {
            string order = null;
            if (opt != null)
            {
                switch (opt.OrderType)
                {
                    case 1:
                        var fields = opt.OrderItem as string[];
                        order = string.Join(", ", fields);
                        break;
                    case 2:
                        var exp = opt.OrderItem as LambdaExpression;
                        var resolver = CreateExpressionResolver(reference, ExpressionResolver.ExpressionResolveOption.Selector);
                        var result = resolver.Resolve(exp);
                        if (result.Parameters != null && result.Parameters.Count > 0)
                            throw new InvalidOperationException("order notyet support parameters");
                        order = result.Sql;
                        break;
                    case 9:
                        break;
                }
                if (!string.IsNullOrEmpty(order))
                {
                    order = "Order By " + order;
                    if (opt.IsDescending)
                        order += " Desc";
                }
            }
            return order;
        }
        protected void BindParameter(DBParameterCollection parameters, ParameterInfo parameter)
        {
            if (parameter.IsHardword && parameter.Hardword.IgnorePrefix)
                parameters.Add(parameter.ParameterName, parameter.Value, "Long");
            else
                parameters.Add(parameter.ParameterName, parameter.Value);
        }

        protected virtual void CreateDBParameterFromDictionary(IDictionary<string, object> dic, ref DBParameterCollection collection, AncestorOption options)
        {
            collection.AddRange(dic.Select(r => new DBParameter(r.Key.ToUpper(), r.Value)));
        }
        protected virtual void CreateDBParameterFromProperty(object model, ref DBParameterCollection collection, AncestorOption options)
        {
            var modelType = model.GetType();
            var properties = modelType.GetProperties();
            Func<string, string> nameResolver = null;
            object resolver;
            if (options.TryGetValue("NameResolver", out resolver))
                nameResolver = resolver as Func<string, string>;
            foreach (var property in properties)
            {
                if (property.CanRead)
                {
                    var value = property.GetValue(model, null);
                    var pname = property.Name;
                    if (nameResolver != null)
                        pname = nameResolver(pname);
                    var parameter = new DBParameter(pname, value);
                    collection.Add(parameter);
                }
            }
        }
        protected string CreateParameterName(string name, bool symbol, string prefix = null, string postfix = null)
        {
            return string.Format("{1}{2}{0}{3}", name, symbol ? ParameterSymbol : "", prefix ?? ParameterPrefix, postfix ?? ParameterPostfix);
        }
        private DbActionResult InternalQuery(string sql, DBParameterCollection dbParameters, Type dataType, bool firstOnly, DbActionOptions options)
        {
            if (dataType != null)
            {
                if (firstOnly)
                    return DbAction.QueryFirst(sql, dbParameters, dataType, options);
                else
                    return DbAction.Query(sql, dbParameters, dataType, options);
            }
            else
            {
                if (firstOnly)
                    return DbAction.QueryFirst(sql, dbParameters, options);
                else
                    return DbAction.Query(sql, dbParameters, options);
            }
        }

        private ReferenceInfo GetReferenceInfo(object model, Type modelType, Type dataType, object origin)
        {
            var reference = new ReferenceInfo();
            var tuple = CreateReferenceTuple(model, modelType, dataType, origin);
            reference.Add(tuple.Item1, tuple.Item2, tuple.Item3);
            return reference;
        }

        private ReferenceInfo GetReferenceInfo(IEnumerable<Type> types, IDictionary<Type, object> proxyMap)
        {
            var reference = new ReferenceInfo();
            Tuple<Type, Type, string> tuple = null;
            foreach (var type in types)
            {
                object origin = null;
                if (proxyMap != null)
                {
                    if (proxyMap.TryGetValue(type, out origin))
                    {
                        tuple = CreateReferenceTuple(null, null, type, origin);
                        reference.Add(tuple.Item1, tuple.Item2, tuple.Item3);
                        continue;
                    }
                }
                else
                    origin = type;
                tuple = CreateReferenceTuple(null, null, type, origin);
                reference.Add(type, tuple.Item2, tuple.Item3);
            }
            return reference;
        }
        private static Tuple<Type, Type, string> CreateReferenceTuple(object model, Type modelType, Type dataType, object origin)
        {
            if (modelType == null && model != null)
                modelType = model.GetType();
            var originType = origin as Type;
            var originName = origin as string;
            //var refType = GetReferenceType(modelType, dataType, originType);
            //var refName = GetReferenceName(modelType, dataType, originType, originName);
            return Tuple.Create(dataType ?? modelType, originType, originName);
        }
        /// <summary>
        /// Reference type order: origin(Type), modelType, dataType
        /// </summary>
        private static Type GetReferenceType(Type modelType, Type dataType, Type originType)
        {
            return originType ?? modelType ?? dataType;
        }
        /// <summary>
        /// Reference name order: origin(Name), origin(Type), dataType, modelType
        /// </summary>        
        private static string GetReferenceName(Type modelType, Type dataType, Type originType, string origin)
        {
            var modelName = modelType != null ? modelType.Name : null;
            var dataName = dataType != null ? dataType.Name : null;
            var originName = originType != null ? originType.Name : null;
            return origin ?? originName ?? dataName ?? modelName;
        }


        string IInternalDataAccessObject.GetServerTime()
        {
            return DateTimeSymbol;
        }

        string IInternalDataAccessObject.GetDummyTable()
        {
            return DummyTable;
        }
        DBParameterCollection IInternalDataAccessObject.CreateDBParameters(object parameterObject, AncestorOption options)
        {
            return CreateDBParameters(parameterObject, options);
        }


        #region Expression Resolver
        /// <summary>
        /// Expression Resolver
        /// </summary>
        public abstract class ExpressionResolver : ExpressionVisitor
        {
            public ExpressionResolver(DataAccessObjectBase dao, ReferenceInfo reference, ExpressionResolveOption option)
            {
                _dao = dao;
                _reference = reference;
                _option = option ?? new ExpressionResolveOption();
                //if (_proxyMap == null)
                //    _proxyMap = new Dictionary<Type, object>();
            }

            protected abstract ExpressionResolver CreateInstance(DataAccessObjectBase dao, ReferenceInfo reference, ExpressionResolveOption option);

            private StringBuilder _sb;
            private DBParameterCollection _dbParameters;
            private int _index;
            private readonly DataAccessObjectBase _dao;
            private readonly ReferenceInfo _reference;
            private readonly ExpressionResolveOption _option;
            private List<string> _groupBy;
            //private readonly IDictionary<Type, object> _proxyMap;
            private ExpressionScope _scope;
            private bool _resolved = false;
            private bool _initialized = false;
            //private List<string> _tables;
            //private List<Type> _dataTypes;
            private bool _groupByFlag = false;
            protected StringBuilder StringBuilder
            {
                get
                {
                    if (_scope != null)
                        return _scope.StringBuilder;
                    return _sb;
                }
            }

            protected string ParameterSymbol
            {
                get { return _dao.ParameterSymbol; }
            }

            protected string ConnectorSymbol
            {
                get { return _dao.ConnectorSymbol; }
            }
            protected DataAccessObjectBase DataAccessObject
            {
                get { return _dao; }
            }

            protected void InitializeResolver(bool force = false)
            {
                if (_resolved || !_initialized || force)
                {
                    _scope = null;
                    _sb = new StringBuilder();
                    _groupBy = new List<string>();
                    _dbParameters = new DBParameterCollection();
                    _index = 0;
                    _resolved = false;
                    _initialized = true;
                    //_tables = new List<string>();
                    //_dataTypes = new List<Type>();
                }
            }
            private void InitializeDataTypes(Expression expression)
            {
                var node = expression as LambdaExpression;
                if (node != null)
                {
                    Type proxyType;
                    foreach (var p in node.Parameters)
                    {
                        Tuple<Type, string> tuple;
                        if (_reference.TryGetStruct(p.Type, out tuple))
                            proxyType = tuple.Item1;

                        //if (proxyType != null && _dataTypes.Contains(proxyType))
                        //    _dataTypes.Add(proxyType);
                        //else if (!_dataTypes.Contains(p.Type))
                        //    _dataTypes.Add(p.Type);
                    }
                }
            }

            public ExpressionResolveResult Resolve(Expression expression)
            {
                InitializeResolver();
                InitializeDataTypes(expression);
                this.Visit(expression);
                var result = new ExpressionResolveResult
                {
                    Dao = _dao,
                    Sql = _sb.ToString().Trim(),
                    Parameters = _dbParameters,
                    Reference = _reference,
                    GroupBy = string.Join(",", _groupBy),
                };
                _resolved = true;
                return result;
            }

            private void Continue(ExpressionResolveResult result)
            {
                _index += result.Parameters.Count;
            }


            protected ExpressionResolveResult ResolveContinue(Expression expression, int parameterIndex)
            {
                InitializeResolver();
                InitializeDataTypes(expression);
                _index = parameterIndex;
                this.Visit(expression);
                var result = new ExpressionResolveResult
                {
                    Dao = _dao,
                    Sql = _sb.ToString().Trim(),
                    Parameters = _dbParameters,
                    Reference = _reference,
                    GroupBy = string.Join(",", _groupBy),
                };
                _resolved = true;
                return result;
            }

            #region Unary
            protected override Expression VisitUnary(UnaryExpression node)
            {
                var operand = node.Operand;
                switch (node.NodeType)
                {
                    case ExpressionType.Not:
                        if (operand.NodeType == ExpressionType.Call)
                            return VisitUnaryMethodNot((MethodCallExpression)operand);
                        ProcessUnaryNot(operand);
                        return node;
                    case ExpressionType.Convert:
                    case ExpressionType.ConvertChecked:
                        if (node.Type == typeof(object) || Nullable.GetUnderlyingType(node.Type) == null || node.Type != typeof(string))
                            Visit(node.Operand);
                        else
                            ProcessTypeConvert(node.Operand.Type, node.Type, operand, null);
                        return node;
                }
                return node;
            }

            protected virtual void ProcessParameters(ReadOnlyCollection<Expression> nodes)
            {
                for (var i = 0; i < nodes.Count; i++)
                {
                    Visit(nodes[i]);
                    if (i != nodes.Count - 1)
                        Write(",");
                }
            }

            protected virtual void ProcessUnaryNot(Expression node)
            {
                using (var scope = CreateScope())
                {
                    Write("NOT");
                    Visit(node);
                }
            }

            protected virtual Expression VisitUnaryMethodNot(MethodCallExpression node)
            {
                //Process: Unary Not Method
                //
                //1.string.Contains/StartsWith/EndsWith (convert to Like)
                //2.Enumerable.Contains (static method)
                //3.ICollection<T>.Contains 

                if (node.Object == null) // is static method
                {
                    // Enumerable.Contains() extends method
                    if (node.Method.DeclaringType == typeof(Enumerable) && node.Method.Name == "Contains")
                    {
                        ProcessCollectionContains(node.Arguments[0], node.Arguments[1], false);
                    }
                    else if (node.Method.DeclaringType == typeof(string))
                    {
                        Write("NOT");
                        Write("(");
                        ProcessStringMethodCall(node.Arguments[0], node.Method, CreateReadOnlyCollection(node.Arguments.Skip(1)));
                        Write(")");
                    }
                }
                else if (node.Method.DeclaringType == typeof(string)) // is not static method, and is string method
                {
                    switch (node.Method.Name)
                    {
                        case "Contains":
                            ProcessStringLike(node.Object, node.Arguments[0], 0, false);
                            break;
                        case "StartsWith":
                            ProcessStringLike(node.Object, node.Arguments[0], 1, false);
                            break;
                        case "EndsWith":
                            ProcessStringLike(node.Object, node.Arguments[0], -1, false);
                            break;
                    }
                }
                else // not static method, and not string
                {
                    using (var scope = CreateScope())
                    {
                        Write("NOT");
                        Visit(node);
                    }
                }
                return node;
            }

            /// <summary>
            /// Collection contains
            /// </summary>
            protected virtual void ProcessCollectionContains(Expression collectionNode, Expression parameterNode, bool positive = true)
            {
                using (var scope = CreateScope())
                {
                    var subResolver = CreateInstance(_dao, _reference, _option);
                    var result = subResolver.ResolveContinue(collectionNode, _index);
                    Continue(result);
                    if (result.Parameters.Count > 0 || result.Sql.Length > 0)
                    {
                        Visit(parameterNode);
                        if (!positive)
                            Write(" NOT");
                        Write(" IN ");
                        Write(result);
                    }
                    else
                    {
                        Write(_dao.ConditionFALSE);
                    }
                }
            }
            #endregion

            #region Binary
            protected override Expression VisitBinary(BinaryExpression node)
            {
                switch (node.NodeType)
                {
                    case ExpressionType.And:
                    case ExpressionType.AndAlso:
                        ProcessBinaryComparison(node.Left, node.Right, "And");
                        break;
                    case ExpressionType.Or:
                    case ExpressionType.OrElse:
                        ProcessBinaryComparison(node.Left, node.Right, "Or");
                        break;
                    case ExpressionType.Equal:
                        ProcessBinaryComparison(node.Left, node.Right, "=");
                        break;
                    case ExpressionType.NotEqual:
                        ProcessBinaryComparison(node.Left, node.Right, "<>");
                        break;
                    case ExpressionType.LessThan:
                        ProcessBinaryComparison(node.Left, node.Right, "<");
                        break;
                    case ExpressionType.LessThanOrEqual:
                        ProcessBinaryComparison(node.Left, node.Right, "<=");
                        break;
                    case ExpressionType.GreaterThan:
                        ProcessBinaryComparison(node.Left, node.Right, ">");
                        break;
                    case ExpressionType.GreaterThanOrEqual:
                        ProcessBinaryComparison(node.Left, node.Right, ">=");
                        break;
                    case ExpressionType.Coalesce:
                        ProcessBinaryCoalesce(node.Left, node.Right);
                        break;
                    case ExpressionType.Add:
                    case ExpressionType.AddChecked:
                        ProcessBinaryAdd(node.Left, node.Right);
                        break;
                    default:
                        throw new NotSupportedException(string.Format("binary symbol '{0}' is not supported", node.NodeType));
                }
                return node;
            }
            private bool IsExpressionTypeString(Expression node)
            {
                var resolver = new ExpressionTypeResolver(node);
                return resolver.ResultType == typeof(string);
            }
            protected bool IsExpressionHardwordMember(Expression node, out HardWordAttribute hardWord)
            {
                var resolver = new ExpressionNodeResolver();
                resolver.Visit(node);
                hardWord = resolver.HardWord;
                return resolver.IsParameterMemberExpression && resolver.IsMemberHardword;
            }
            protected virtual void ProcessBinaryAdd(Expression left, Expression right)
            {
                string @operator = "+";
                if (IsExpressionTypeString(left) || IsExpressionTypeString(right))
                    @operator = ConnectorSymbol;
                Visit(left);
                Write(" {0} ", @operator);
                Visit(right);
            }
            /// <summary>
            /// Process comparison
            /// </summary>
            protected virtual void ProcessBinaryComparison(Expression left, Expression right, string @operator)
            {
                using (var scope = CreateScope())
                {
                    Visit(left);
                    // if right exp is constant
                    if (TryResolveValue(right, out object value))
                    {
                        if (value == null) // right is null
                        {
                            switch (@operator)
                            {
                                case "=":
                                    @operator = "Is";
                                    break;
                                case "<>":
                                    @operator = "Is Not";
                                    break;
                            }
                            Write(" {0} ", @operator);
                            ProcessConstantNull();
                            return; // process end
                        }
                        else if (IsExpressionHardwordMember(left, out HardWordAttribute hd))
                        {
                            Write(" {0} ", @operator);
                            ProcessConstant(value, hd);
                            return; // process end
                        }
                    }

                    Write(" {0} ", @operator);
                    Visit(right);

                }
            }
            /// <summary>
            /// process null 
            /// </summary>        
            protected virtual void ProcessBinaryCoalesce(Expression left, Expression right)
            {
                Write("Coalesce(");
                Visit(left);
                Write(",");
                Visit(right);
                Write(")");
            }

            protected virtual bool IsNullConstant(Expression node)
            {
                object value;
                if (TryResolveValue(node, out value))
                    return value == null;
                return false;
            }
            #endregion

            #region Constant
            protected override Expression VisitConstant(ConstantExpression node)
            {
                if (node.Value == null)
                {
                    ProcessConstantNull();
                    return node;
                }

                var valueType = node.Value.GetType();
                switch (Type.GetTypeCode(valueType))
                {
                    case TypeCode.Object:
                        if (typeof(IEnumerable).IsAssignableFrom(valueType))
                            // is IEnumerable
                            ProcessConstantEnumerable((IEnumerable)node.Value);
                        else
                            // not Enumerable, try constant (ToString)
                            ProcessConstant(node.Value.ToString());
                        break;
                    default:
                        ProcessConstant(node.Value);
                        break;
                }
                return node;
            }


            protected virtual void ProcessConstantEnumerable(IEnumerable list)
            {
                WriteParameters(list.OfType<object>().ToArray());
            }
            protected virtual void ProcessConstant(object value, HardWordAttribute hardWord = null)
            {
                WriteParameter(value, hardWord);
            }
            protected virtual void ProcessConstantNull()
            {
                Write("Null");
            }
            #endregion

            #region Methods
            protected override Expression VisitMethodCall(MethodCallExpression node)
            {
                if (node.Object == null) // is static method call
                {
                    return VisitStaticMethodCall(node);
                }
                else if (node.Method.DeclaringType == typeof(string))
                {
                    ProcessStringMethodCall(node.Object, node.Method, node.Arguments);
                }
                else if (IsUnderlyingType(node.Method.DeclaringType, typeof(DateTime)))
                {
                    ProcessDateTimeMethodCall(node.Object, node.Method, node.Arguments);
                }
                else if (IsCollectionType(node.Method.DeclaringType) && !IsDictionaryType(node.Method.DeclaringType))
                {
                    ProcessCollectionMethodCall(node.Object, node.Method, node.Arguments);
                }
                else
                {
                    object value;
                    if (TryResolveValue(node, out value))
                        ProcessConstant(value);
                    else
                        ProcessMethodCall(node.Object, node.Method, node.Arguments);
                }

                return node;
            }
            protected virtual Expression VisitStaticMethodCall(MethodCallExpression node)
            {
                if (node.Method.DeclaringType == typeof(Math))
                {
                    return VisitMathStaticMethodCall(node);
                }
                else if (node.Method.DeclaringType == typeof(Enumerable))
                {
                    return VisitCollectionStaticMethodCall(node);
                }
                else if (node.Method.DeclaringType == typeof(SqlStatement))
                {
                    return VisitSqlStatementMethodCall(node);
                }
                else if (node.Method.DeclaringType == typeof(string))
                {
                    return VisitStringStaticMethodCall(node);
                }
                else if (node.Method.DeclaringType == typeof(Queryable))
                {
                    return VisitQueryableStaticMethodCall(node);
                }
                else if (node.Method.Name == "Parse")
                {
                    ProcessTypeConvert(node.Arguments[0].Type, node.Method.DeclaringType, node.Arguments[0], CreateReadOnlyCollection(node.Arguments.Skip(1)));
                }
                else if (node.Method.Name == "Between")
                {
                    ProcessBetweenMethodCall(node.Arguments[0], node.Arguments[1], node.Arguments[2]);
                }
                else if (node.Method.Name == "Truncate")
                {
                    ProcessTruncateMethodCall(node.Arguments[0]);
                }
                else if (node.Method.Name == "GroupCount")
                {
                    ProcessGroupBy(node.Arguments[0], "Count");
                }
                else if (node.Method.Name == "GroupMax")
                {
                    ProcessGroupBy(node.Arguments[0], "Max");
                }
                else if (node.Method.Name == "GroupMin")
                {
                    ProcessGroupBy(node.Arguments[0], "Min");
                }
                else if (node.Method.Name == "SelectAll")
                {
                    var parameterType = node.Arguments[0].Type;
                    var command = DataAccessObject.CreateSelectCommand(_reference, parameterType);
                    Write(command);
                }
                else
                {
                    object value;
                    if (TryResolveValue(node, out value))
                        WriteParameter(value);
                }

                return node;
            }

            protected virtual Expression VisitSqlStatementMethodCall(MethodCallExpression node)
            {
                switch (node.Method.Name)
                {
                    case "ToString":
                        Type fromType = node.Arguments[0].Type;
                        fromType = Nullable.GetUnderlyingType(fromType) ?? fromType;
                        ReadOnlyCollection<Expression> arguments = null;
                        var parameters = node.Method.GetParameters();
                        bool flgFmt = false;
                        if (parameters.Length > 1)
                        {
                            flgFmt = true;
                            if (parameters.Length == 3)
                            {
                                object argStr2;
                                if (TryResolveValue(node.Arguments[2], out argStr2))
                                    flgFmt = Convert.ToBoolean(argStr2);
                            }
                            arguments = CreateReadOnlyCollection(node.Arguments[1]);
                        }
                        else
                        {
                            arguments = CreateReadOnlyCollection();
                        }
                        ProcessConvertToString(fromType, node.Arguments[0], arguments, flgFmt);
                        break;
                    case "Truncate":
                        ProcessTruncateMethodCall(node.Arguments[0]);
                        break;
                    case "NotNull":
                        Write("Nvl(");
                        Visit(node.Arguments[0]);
                        Write(",");
                        if (node.Arguments.Count > 1)
                            Visit(node.Arguments[1]);
                        else
                            Write("'!EMPTY!'");
                        Write(")");
                        break;
                    case "Between":
                        ProcessBetweenMethodCall(node.Arguments[0], node.Arguments[1], node.Arguments[2]);
                        break;
                    case "JoinEquals":
                        SqlStatement.Joins join = SqlStatement.Joins.Inner;
                        if (node.Arguments.Count > 2)
                            TryResolveValue(node.Arguments[2], out join);
                        ProcessJoinMethodCall(node.Arguments[0], node.Arguments[1], join);
                        break;
                    case "Func":
                        string name;
                        if (!TryResolveValue(node.Arguments[0], out name))
                            throw new InvalidOperationException("invalid func name");
                        var arrExp = node.Arguments[1] as NewArrayExpression;
                        ProcessFuncMethodCall(name, arrExp.Expressions);
                        break;
                }
                return node;
            }
            protected virtual Expression VisitMathStaticMethodCall(MethodCallExpression node)
            {
                Write(node.Method.Name.ToUpper());
                Write("(");
                Visit(node.Arguments[0]);
                foreach (var args in node.Arguments.Skip(1))
                {
                    Write(",");
                    Visit(args);
                }
                Write(")");
                return node;
            }

            protected virtual Expression VisitQueryableStaticMethodCall(MethodCallExpression node)
            {
                return node;
            }

            protected virtual Expression VisitCollectionStaticMethodCall(MethodCallExpression node)
            {
                switch (node.Method.Name)
                {
                    case "Contains":
                        ProcessCollectionContains(node.Arguments[0], node.Arguments[1]);
                        return node;
                }
                return node;
            }

            protected virtual Expression VisitStringStaticMethodCall(MethodCallExpression node)
            {
                switch (node.Method.Name)
                {
                    case "Compare":
                        ProcessStringMethodCall(node.Arguments[0], node.Method, CreateReadOnlyCollection(node.Arguments.Skip(1)));
                        break;
                    case "IsNullOrEmpty":
                        ProcessStringMethodCall(node.Arguments[0], node.Method, null);
                        break;
                }
                return node;
            }
            protected virtual void ProcessStringMethodCall(Expression objectNode, MethodInfo method, ReadOnlyCollection<Expression> args)
            {
                switch (method.Name)
                {
                    case "StartsWith":
                        ProcessStringLike(objectNode, args[0], 1);
                        break;
                    case "EndsWith":
                        ProcessStringLike(objectNode, args[0], -1);
                        break;
                    case "Contains":
                        ProcessStringLike(objectNode, args[0], 0);
                        break;
                    case "Substring":
                        object subStartIndex, length = null;
                        if (TryResolveValue(args[0], out subStartIndex))
                        {
                            Write("SubStr(");
                            Visit(objectNode);
                            Write(",");
                            WriteParameter((int)subStartIndex + 1);
                            if (args.Count == 2 && TryResolveValue(args[1], out length))
                            {
                                Write(",");
                                WriteParameter(length);
                            }
                            Write(")");
                        }
                        break;
                    case "Trim":
                        ProcessStringTrim(objectNode, args.ElementAtOrDefault(0), 0);
                        break;
                    case "TrimStart":
                        ProcessStringTrim(objectNode, args.ElementAtOrDefault(0), -1);
                        break;
                    case "TrimEnd":
                        ProcessStringTrim(objectNode, args.ElementAtOrDefault(0), 1);
                        break;
                    case "ToUpper":
                        Write("Upper(");
                        Visit(objectNode);
                        Write(")");
                        break;
                    case "ToLower":
                        Write("Lower(");
                        Visit(objectNode);
                        Write(")");
                        break;
                    case "PadLeft":
                    case "PadRight":
                        Write((method.Name == "PadLeft" ? "L" : "R") + "Pad(");
                        Visit(objectNode);
                        if (args.Count > 0)
                        {
                            Write(",");
                            Visit(args.ElementAtOrDefault(0));
                        }
                        Write(")");
                        break;

                        break;
                    case "IndexOf":
                        Write("CharIndex(");
                        Visit(args[0]);
                        Write(",");
                        Visit(objectNode);
                        object findStartIndex = null;
                        if (args.Count == 2 && TryResolveValue(args[1], out findStartIndex) && findStartIndex != null)
                        {
                            Write(",");
                            WriteParameter((int)findStartIndex + 1);
                        }
                        Write(")");
                        break;
                    case "Compare":
                    case "CompareTo":
                        ProcessCompareTo(objectNode, args[0]);
                        break;
                    case "IsNullOrEmpty":
                        var resolver = CreateInstance(_dao, _reference, _option);
                        var result = resolver.ResolveContinue(objectNode, _index);
                        if (result.Parameters.Count > 0 || result.Sql.Length > 0)
                        {
                            using (var scope = CreateScope())
                            {
                                if (result.Parameters.Count > 0)
                                {
                                    var pname = result.Parameters[0].Name;
                                    _dbParameters.Add(result.Parameters[0]);
                                    Write(pname);
                                    Write("Is Null Or");
                                    Write(pname);
                                    Write("= ''");
                                }
                                else
                                {
                                    Write(result.Sql);
                                    Write("Is Null Or");
                                    Write(result.Sql);
                                    Write("= ''");
                                }
                            }
                        }
                        break;
                }
            }

            protected virtual void ProcessBetweenMethodCall(Expression nodeObject, Expression from, Expression to)
            {
                Visit(nodeObject);
                Write("Between");
                Visit(from);
                Write("And");
                Visit(to);
            }
            protected abstract void ProcessJoinMethodCall(Expression left, Expression right, SqlStatement.Joins joins);
            protected abstract void ProcessTruncateMethodCall(Expression nodeObject);
            protected virtual void ProcessFuncMethodCall(string name, ReadOnlyCollection<Expression> parameters)
            {
                Write(name);
                Write("(");
                ProcessParameters(parameters);
                Write(")");
            }
            protected virtual void ProcessGroupBy(Expression nodeObject, string symbol)
            {
                _groupByFlag = true;
                Write(symbol);
                Write("(");
                Visit(nodeObject);
                Write(")");
                _groupByFlag = false;
            }

            protected virtual void ProcessCollectionMethodCall(Expression objectNode, MethodInfo method, ReadOnlyCollection<Expression> args)
            {
                switch (method.Name)
                {
                    case "Contains":
                        ProcessCollectionContains(objectNode, args[0]);
                        break;
                    default:
                        break;
                }
            }
            protected virtual void ProcessDateTimeMethodCall(Expression objectNode, MethodInfo method, ReadOnlyCollection<Expression> args)
            {
                switch (method.Name)
                {
                    case "ToString":
                        ProcessTypeConvert(typeof(DateTime), typeof(string), objectNode, args);
                        break;
                    case "CompareTo":
                        ProcessCompareTo(objectNode, args[0]);
                        break;
                    default:
                        break;
                }
            }
            protected virtual void ProcessMethodCall(Expression objectNode, MethodInfo method, ReadOnlyCollection<Expression> args)
            {
                switch (method.Name)
                {
                    case "ToString":
                        ProcessTypeConvert(typeof(object), typeof(string), objectNode, args);
                        break;
                    default:

                        break;
                }
            }
            protected virtual void ProcessCompareTo(Expression objectNode, Expression comparisonNode)
            {
                object src, arg;
                var srcFlag = TryResolveValue(objectNode, out src);
                var argFlag = TryResolveValue(comparisonNode, out arg);
                string lastSrc = null, lastArg = null;

                using (var scope = CreateScope())
                {
                    Write("Case");
                    Write("When");
                    if (srcFlag)
                    {
                        ProcessConstant(src);
                        lastSrc = GetLastParameterName(true);
                    }
                    else
                        Visit(objectNode);
                    Write("=");
                    if (argFlag)
                    {
                        ProcessConstant(arg);
                        lastArg = GetLastParameterName(true);
                    }
                    else
                        Visit(comparisonNode);
                    Write("Then 0");

                    Write("When");
                    if (srcFlag)
                        Write(lastSrc);
                    else
                        Visit(objectNode);
                    Write("<");
                    if (argFlag)
                        Write(lastArg);
                    else
                        Visit(comparisonNode);
                    Write("Then -1");
                    Write("Else 1 End");
                }
            }
            protected virtual void ProcessTypeConvert(Type fromType, Type toType, Expression objectNode, ReadOnlyCollection<Expression> args)
            {
                var code = Type.GetTypeCode(toType);
                switch (code)
                {
                    case TypeCode.String:
                        ProcessConvertToString(fromType, objectNode, args, true);
                        break;
                    case TypeCode.DateTime:
                        ProcessConvertToDateTime(fromType, objectNode, args);
                        break;
                    case TypeCode.Int16:
                    case TypeCode.Int32:
                    case TypeCode.Int64:
                    case TypeCode.UInt16:
                    case TypeCode.UInt32:
                    case TypeCode.UInt64:
                    case TypeCode.Decimal:
                    case TypeCode.Single:
                    case TypeCode.Double:
                        ProcessConvertToDecimal(fromType, toType, objectNode, args);
                        break;
                    default:
                        if (IsUnderlyingType(toType, fromType))
                            Visit(objectNode);
                        else
                            throw new InvalidCastException(string.Format("can not convert from '{0}' to '{1}'", fromType.Name, toType.Name));
                        break;
                }
            }

            protected abstract void ProcessConvertToString(Type fromType, Expression objectNode, ReadOnlyCollection<Expression> args, bool useFmtConvert);
            protected abstract void ProcessConvertToDateTime(Type fromType, Expression objectNode, ReadOnlyCollection<Expression> args);
            protected abstract void ProcessConvertToDecimal(Type fromType, Type toType, Expression objectNode, ReadOnlyCollection<Expression> args);
            /// <summary>
            /// process string contains (LIKE)
            /// </summary>
            /// <param name="stringNode">node</param>
            /// <param name="comparisonNode">comparison node</param>
            /// <param name="comparisonPosition">fuzzy position (0: both, 1:postfix, -1:prefix)</param>
            /// <param name="positive">NOT LIKE</param>
            protected virtual void ProcessStringLike(Expression stringNode, Expression comparisonNode, int comparisonPosition, bool positive = true)
            {
                using (var scope = CreateScope())
                {
                    Visit(stringNode);
                    if (!positive)
                        Write("Not");
                    Write("Like");
                    using (var innerScope = CreateScope())
                    {
                        if (comparisonPosition <= 0)
                        {
                            Write("'%'");
                            Write(ConnectorSymbol);
                        }
                        Visit(comparisonNode);
                        if (comparisonPosition >= 0)
                        {
                            Write(ConnectorSymbol);
                            Write("'%'");
                        }
                    }
                }
            }
            protected virtual void ProcessStringTrim(Expression stringNode, Expression trimTargetNode, int trimPosition)
            {
                switch (trimPosition)
                {
                    case 0:
                        Write("Trim");
                        break;
                    case 1:
                        Write("RTrim");
                        break;
                    case -1:
                        Write("LTrim");
                        break;
                }
                Write("(");
                Visit(stringNode);
                if (trimPosition != 0 && trimTargetNode != null)
                {
                    Write(",");
                    Visit(trimTargetNode);
                }
                Write(")");
            }
            #endregion

            #region Members
            protected override Expression VisitMember(MemberExpression node)
            {
                if (node.Expression != null)
                {
                    switch (node.Expression.NodeType)
                    {
                        case ExpressionType.Parameter:
                            ProcessParameterMember(node.Expression.Type, node.Member);
                            break;
                        case ExpressionType.Constant:
                            ProcessConstantMember(((ConstantExpression)node.Expression).Value, node.Member);
                            break;
                        case ExpressionType.MemberAccess:
                            ProcessMemberAccess(node);
                            break;
                    }
                }
                else if (node.Member.DeclaringType == typeof(Server))
                    ProcessServerMemberAccess(node);
                else if (node.Member.DeclaringType == typeof(DateTime))
                    ProcessDateTimeMemberAccess(node);
                else
                    ProcessConstantMember(null, node.Member);
                return node;
            }



            protected virtual void ProcessParameterMember(Type parameterType, MemberInfo member)
            {
                var property = member as PropertyInfo;
                var tableName = GetTableName(parameterType);
                var memberName = TableManager.GetName(member as PropertyInfo);
                var value = string.Format("{0}.{1}", tableName, memberName);
                if (!_groupByFlag)
                    _groupBy.Add(value);
                if (property != null && _option.UseHardWord)
                {
                    var hd = HardWordManager.Get(property);
                    if (hd != null)
                    {
                        value = _dao.ConvertFromHardWord(value, hd);
                        if (_option.AppendAs)
                            value = value + " AS " + memberName;
                    }
                }
                Write(value);
            }
            protected virtual void ProcessConstantMember(object constant, MemberInfo member)
            {
                object value = null;
                switch (member.MemberType)
                {
                    case MemberTypes.Field:
                        value = ((FieldInfo)member).GetValue(constant);
                        break;
                    case MemberTypes.Property:
                        value = ((PropertyInfo)member).GetValue(constant, null);
                        break;
                }
                var constantExpression = Expression.Constant(value);
                VisitConstant(constantExpression);
            }
            protected virtual void ProcessMemberAccess(MemberExpression node)
            {
                object value;
                // if parent can be resolved, use constant
                if (TryResolveValue(node, out value))
                {
                    var constantExpression = Expression.Constant(value);
                    VisitConstant(constantExpression);
                }
                else if (node.Member.DeclaringType == typeof(string))
                    ProcessCommonStringMemberAccess(node);
                else if (node.Member.DeclaringType == typeof(Server))
                    ProcessDateTimeMemberAccess(node);
            }
            /// <summary>
            /// string member access
            /// </summary>        
            protected virtual void ProcessCommonStringMemberAccess(MemberExpression node)
            {
                switch (node.Member.Name)
                {
                    case "Length": //計算長度
                        Write("Length(");
                        Visit(node.Expression);
                        Write(")");
                        break;
                }
            }
            /// <summary>
            /// Server member access
            /// <para>Depended by database type</para>
            /// </summary>        
            protected virtual void ProcessServerMemberAccess(MemberExpression node)
            {

            }
            /// <summary>
            /// DateTime member access
            /// </summary>
            protected virtual void ProcessDateTimeMemberAccess(MemberExpression node)
            {
                object value;
                if (!TryResolveValue(node, out value))
                    throw new InvalidOperationException("can not resolve DateTime member: " + node);
                ProcessConstant(value);
            }

            protected virtual string GetTableName(Type parameterExpressionType)
            {
                //object proxy;
                string tableName = null;
                Tuple<Type, string> proxy;
                if (_reference.TryGetStruct(parameterExpressionType, out proxy))
                //if (_proxyMap.TryGetValue(parameterExpressionType, out proxy))
                {
                    object proxyObj = proxy.Item1;
                    if (proxyObj == null)
                        proxyObj = proxy.Item2;
                    if (proxyObj == null)
                        proxyObj = parameterExpressionType;
                    tableName = TableManager.GetTableName(proxyObj);
                    //var dataType = proxyObj as Type;
                    //if (dataType != null && !_dataTypes.Contains(dataType))
                    //    _dataTypes.Add(dataType);
                }
                else
                {
                    tableName = TableManager.GetName(parameterExpressionType);
                    //if (!_dataTypes.Contains(parameterExpressionType))
                    //    _dataTypes.Add(parameterExpressionType);
                }
                //if (!_tables.Any(r => r.Equals(tableName, StringComparison.OrdinalIgnoreCase)))
                //    _tables.Add(tableName);
                return tableName;
            }

            protected bool IsConstantResolvable(Expression node)
            {
                var visitor = new ExpressionNodeResolver();
                visitor.Visit(node);
                return !visitor.IsParameterMemberExpression && !visitor.IsFuncExpression;
            }
            protected virtual object ResolveValue(Expression node)
            {
                try
                {
                    return Expression.Lambda(node).Compile().DynamicInvoke();
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException("Expression resolve fail: " + ex.Message, ex);
                }
            }

            protected virtual bool TryResolveValue(Expression node, out object value)
            {
                Exception error;
                return TryResolveValue(node, out value, out error);
            }
            protected virtual bool TryResolveValue(Expression node, out object value, out Exception error)
            {
                error = null;
                if (IsConstantResolvable(node))
                {
                    try
                    {
                        value = ResolveValue(node);
                        return true;
                    }
                    catch (Exception ex)
                    {
                        error = ex;
                    }
                }
                value = null;
                return false;
            }
            protected virtual bool TryResolveValue<T>(Expression node, out T value)
            {
                Exception error;
                return TryResolveValue(node, out value, out error);
            }
            protected virtual bool TryResolveValue<T>(Expression node, out T value, out Exception error)
            {
                object v;
                bool res;
                value = (res = TryResolveValue(node, out v, out error)) ? (T)v : default(T);
                return res;
            }
            #endregion

            #region 
            /// <summary>
            /// new object (selector used)
            /// </summary>
            protected override Expression VisitNew(NewExpression node)
            {
                if (IsAnonymousType(node.Type)) // anonymouse type 
                {
                    ProcessNewAnonymousObject(node);
                }
                return node;
            }

            protected override Expression VisitMemberInit(MemberInitExpression node)
            {
                var newExpression = node.NewExpression;
                var type = newExpression.Type;
                var properties = TableManager.GetBrowsableProperties(type);
                for (var index = 0; index < properties.Length; index++)
                {
                    var property = properties[index];
                    var binding = node.Bindings.FirstOrDefault(b => b.Member == property);
                    if (binding != null)
                    {
                        var memberAssignment = binding as MemberAssignment;
                        Action<StringBuilder, string> action = null;
                        if (_option.NewAs)
                        {
                            action = (sb, text) => ExpressionScope.AppendText(sb, string.Format("{0} AS {1}", text, property.Name));
                        }
                        using (var scope = CreateScope())
                        {
                            scope.DisposeAction = action;
                            Visit(memberAssignment.Expression);
                        }
                    }
                    else
                    {
                        var name = property.Name;
                        if (_option.UseHardWord)
                        {
                            var hd = HardWordManager.Get(property);
                            if (hd != null)
                                name = _dao.ConvertFromHardWord(name, hd);
                        }
                        Write(name);
                    }
                    if (index != properties.Length - 1)
                        Write(",");
                }
                //for (var index = 0; index < node.Bindings.Count; index++)
                //{
                //    var binding = node.Bindings[index];
                //    var member = binding.Member;
                //    var memberAssignment = binding as MemberAssignment;
                //    if (memberAssignment != null)
                //    {
                //        Action<StringBuilder, string> action = null;
                //        if (_option.UseHardWord)
                //        {
                //            var hd = HardWordManager.Get(member as PropertyInfo);
                //            if (hd != null) // hardword
                //                action = (sb, text) => ExpressionScope.AppendText(sb, _dao.ConvertFromHardWord(text, hd));
                //        }
                //        if (action == null && _option.NewAs)
                //        {
                //            action = (sb, text) => ExpressionScope.AppendText(sb, string.Format("({0}) AS {1}", text, member.Name));
                //        }
                //        using (var scope = CreateScope())
                //        {
                //            scope.DisposeAction = action;
                //            Visit(memberAssignment.Expression);
                //        }

                //    }

                //}
                return node;
            }
            /// <summary>
            /// new array (selector used)
            /// </summary>            
            protected override Expression VisitNewArray(NewArrayExpression node)
            {
                var flag = false;
                foreach (var expression in node.Expressions)
                {
                    if (flag)
                        Write(",");
                    else
                        flag = true;
                    Visit(expression);
                }
                return node;
            }

            protected virtual void ProcessNewAnonymousObject(NewExpression node)
            {
                for (int index = 0; index < node.Members.Count; index++)
                {
                    var argument = node.Arguments[index];
                    var member = node.Members[index];
                    if (_option.NewAs)
                    {
                        Visit(argument);
                        Write("As");
                        Write(member.Name);
                    }
                    else
                        Visit(argument);
                    if (index != node.Members.Count - 1)
                        Write(",");
                }
            }
            #endregion

            public static ReadOnlyCollection<Expression> CreateReadOnlyCollection(IEnumerable<Expression> expressions)
            {
                return new ReadOnlyCollection<Expression>(expressions.ToList());
            }
            public static ReadOnlyCollection<Expression> CreateReadOnlyCollection(params Expression[] expressions)
            {
                return new ReadOnlyCollection<Expression>(expressions.ToList());
            }
            protected virtual void Write(string text, bool? space = null)
            {
                var sb = StringBuilder;
                WriteToStringBuilder(sb, text, space);
            }
            protected virtual void Write(string format, bool? space, params string[] args)
            {
                var text = string.Format(format, args);
                Write(text, space);
            }
            protected virtual void Write(string format, params string[] args)
            {
                Write(format, null, args);
            }
            protected virtual ParameterInfo GetParameter(object value, HardWordAttribute hardWord)
            {
                var name = _index.ToString();
                var parameter = DataAccessObject.CreateParameter(value, name, true, hardWord: hardWord);
                if (!parameter.IsSysDateConverted)
                    MoveNextParameter();
                return parameter;
            }
            protected virtual void Write(ExpressionResolveResult result)
            {
                Write(result.Sql);
                _dbParameters.AddRange(result.Parameters);
            }
            protected virtual DBParameter WriteParameter(object value, HardWordAttribute hardWord = null)
            {
                var parameter = GetParameter(value, hardWord);
                return WriteParameter(parameter);
            }
            protected virtual DBParameter WriteParameter(ParameterInfo parameter)
            {
                DBParameter p = null;
                if (!parameter.IsSysDateConverted)
                    p = _dbParameters.Add(parameter.ParameterName, parameter.Value);
                Write(parameter.ValueName);
                return p;
            }
            protected virtual void WriteDbParameter(DBParameter parameter, bool addParameter = true)
            {
                if (addParameter)
                    _dbParameters.Add(parameter);
                Write(ParameterSymbol + parameter.Name);
                if (addParameter)
                    _index++;
            }
            protected virtual void WriteParameters(object[] values)
            {
                var parameters = values.Select(v => GetParameter(v, null)).ToArray();
                var appendTexts = string.Join(" , ", parameters.Select(p => p.ValueName));
                if (appendTexts.Length > 0)
                {
                    var dbParameters = parameters.Where(p => !p.IsSysDateConverted).Select(p => new DBParameter(p.ValueName, p.Value));
                    _dbParameters.AddRange(dbParameters);
                    Write("(");
                    Write(appendTexts);
                    Write(")");
                }
            }
            protected virtual string CreateParameterName()
            {
                var name = DataAccessObject.CreateParameterName(_index++.ToString(), false);
                //var name= GetParameterName(_index++);
                return name;
            }
            protected virtual string GetNextParameterName(bool symbol)
            {
                return DataAccessObject.CreateParameterName((_index + 1).ToString(), symbol);
            }
            protected virtual void MoveNextParameter()
            {
                _index++;
            }
            protected virtual string GetLastParameterName(bool symbol)
            {
                if (_index == 0) return null;
                return DataAccessObject.CreateParameterName((_index - 1).ToString(), symbol);
            }

            protected virtual ExpressionScope CreateScope()
            {
                var rb = _option.ScopeBucket;
                var scope = _scope == null ? new ExpressionScope(this, rb) : new ExpressionScope(this, _scope, rb);
                return _scope = scope;
            }
            protected ExpressionResolver Clone()
            {
                return (ExpressionResolver)((ICloneable)this).Clone();
            }

            protected bool IsCollectionType(Type type)
            {
                return InternalHelper.IsCollectionType(type);
            }
            protected bool IsDictionaryType(Type type)
            {
                return InternalHelper.IsDictionary(type);
            }
            protected bool IsUnderlyingType(Type type, Type comparionType)
            {
                return InternalHelper.IsUnderlyingType(type, comparionType);
            }

            protected bool IsAnonymousType(Type type)
            {
                return InternalHelper.IsAnonymousType(type);
            }
            protected Type GetUnderlyingType(Type type)
            {
                return InternalHelper.GetUnderlyingType(type);
            }

            public static MergedExpressionResolveResult CombineResult(ExpressionResolveResult result1, ExpressionResolveResult result2)
            {
                var merged = new MergedExpressionResolveResult();

                if (result1 != null)
                {
                    merged.Sql1 = result1.Sql;
                    merged.Parameters.AddRange(result1.Parameters);
                    merged.Reference.Add(result1.Reference);
                }
                if (result2 != null)
                {
                    var extraSql = result2.Sql;
                    var extraParameters = result2.Parameters;
                    var postfix = result2.Dao.MergeParameterPostfix;
                    for (int i = 0; i < extraParameters.Count; i++)
                    {
                        var name = extraParameters[i].Name;
                        name += postfix;
                        if (extraSql != null)
                        {
                            var pattern = Regex.Escape(extraParameters[i].Name) + "\\s?";
                            var parts = Regex.Split(extraSql, pattern);
                            var sb = new StringBuilder();
                            for (var j = 0; j < parts.Length; j++)
                            {
                                WriteToStringBuilder(sb, parts[j], null);
                                if (j != parts.Length - 1)
                                    WriteToStringBuilder(sb, name, null);
                            }
                            extraSql = sb.ToString();
                            //extraSql = Regex.Replace(extraSql, Regex.Escape(extraParameters[i].Name) + "\\s?", name + " ");
                            //extraSql = extraSql.Replace(extraParameters[i].Name + " ", name + " ");
                        }
                        extraParameters[i].Name = name;
                    }
                    merged.Sql2 = extraSql;
                    merged.Parameters.AddRange(extraParameters);
                    merged.Reference.Add(result2.Reference);
                }
                return merged;
            }
            public static void WriteToStringBuilder(StringBuilder sb, string text, bool? space)
            {
                var textChecklist = new char[] { ',', '(', ')', ' ' };
                var lastChecklist = new char[] { ' ', '(' };
                var flgSpace = false;
                if (space.HasValue)
                    flgSpace = space.Value;
                else if ((text != null && text.Length > 0 && !textChecklist.Contains(text[0]))  // text first char check
                    && (sb.Length > 0 && !lastChecklist.Contains(sb[sb.Length - 1]))) // last char check
                    flgSpace = true;
                if (flgSpace)
                    sb.Append(" ");
                sb.Append(text);
            }
            public class ExpressionResolveOption
            {

                public static readonly ExpressionResolveOption Default = new ExpressionResolveOption();
                public static readonly ExpressionResolveOption GroupBy = new ExpressionResolveOption { AppendAs = false, UseHardWord = false, NewAs = false, };
                public static readonly ExpressionResolveOption Selector = new ExpressionResolveOption { AppendAs = true, UseHardWord = true, NewAs = true, ScopeBucket = false };

                public ExpressionResolveOption()
                {
                    AppendAs = false;
                    UseHardWord = true;
                    NewAs = true;
                    ScopeBucket = true;
                }
                public bool AppendAs { get; set; }
                public bool UseHardWord { get; set; }
                public bool NewAs { get; set; }
                public bool ScopeBucket { get; set; }
            }

            /// <summary>
            /// Resolve result
            /// </summary>
            public class ExpressionResolveResult
            {
                public DataAccessObjectBase Dao { get; set; }
                /// <summary>
                /// sql string
                /// </summary>
                public string Sql { get; set; }
                /// <summary>
                /// parameter
                /// </summary>
                public DBParameterCollection Parameters { get; set; }
                /// <summary>
                /// Reference table/origin info
                /// </summary>
                public ReferenceInfo Reference { get; set; }
                /// <summary>
                /// Group by string
                /// </summary>
                public string GroupBy { get; set; }
            }
            public class MergedExpressionResolveResult
            {
                public MergedExpressionResolveResult()
                {
                    Reference = new ReferenceInfo();
                }
                private DBParameterCollection _parameters = new DBParameterCollection();
                public string Sql1 { get; set; }
                public string Sql2 { get; set; }
                public ReferenceInfo Reference { get; set; }
                public DBParameterCollection Parameters { get { return _parameters; } }
            }
            protected class ExpressionScope : IDisposable
            {
                private readonly ExpressionResolver _resolver;
                private readonly bool _rb;
                private StringBuilder _sb = new StringBuilder();
                private bool _disposed = false;

                public Action<StringBuilder, string> DisposeAction { get; set; }

                public ExpressionScope(ExpressionResolver resolver, bool rb = true)
                {
                    _resolver = resolver;
                    _rb = rb;
                    if (rb)
                        AppendText("(");

                }
                public ExpressionScope(ExpressionResolver resolver, ExpressionScope parent, bool rb = true)
                {
                    _resolver = resolver;
                    Parent = parent;
                    _rb = rb;
                    if (rb)
                        AppendText("(");
                }

                public StringBuilder StringBuilder
                {
                    get { return _sb; }
                }
                public ExpressionScope Parent
                {
                    get; private set;
                }
                public bool IsRoot
                {
                    get { return Parent == null; }
                }
                ~ExpressionScope()
                {
                    Dispose(false);
                }
                public void Dispose()
                {
                    Dispose(true);
                    GC.SuppressFinalize(this);
                }
                protected virtual void Dispose(bool disposing)
                {
                    if (!_disposed)
                    {
                        if (disposing)
                        {
                            if (_rb)
                                AppendText(")");
                            StringBuilder sb = IsRoot ? _resolver._sb : Parent.StringBuilder;
                            var text = _sb.ToString();
                            //if (sb.Length != 0 && sb[sb.Length - 1] != ' ')
                            //    sb.Append(" ");
                            //sb.Append(text);
                            if (DisposeAction != null)
                                DisposeAction(sb, text);
                            else
                                AppendText(sb, text);
                            _resolver._scope = !IsRoot ? Parent : null;
                        }
                        _disposed = true;
                    }
                }
                public static void AppendText(StringBuilder sb, string text, bool? space = null)
                {
                    WriteToStringBuilder(sb, text, space);
                }
                private void AppendText(string text, bool? space = null)
                {
                    AppendText(_sb, text, space);
                }
            }

        }

        protected sealed class ExpressionTypeResolver : ExpressionVisitor
        {
            private Type _resultType;


            public ExpressionTypeResolver(Expression node)
            {
                Visit(node);
            }
            public Type ResultType
            {
                get { return _resultType; }
            }


            protected override Expression VisitConstant(ConstantExpression node)
            {
                SetResultType(node.Type);
                return node;
            }

            protected override Expression VisitUnary(UnaryExpression node)
            {
                switch (node.NodeType)
                {
                    case ExpressionType.Convert:
                    case ExpressionType.ConvertChecked:
                        if (node.Type == typeof(object))
                            Visit(node.Operand);
                        else
                            SetResultType(node.Type);
                        break;
                }
                return node;
            }
            protected override Expression VisitMember(MemberExpression node)
            {
                var member = node.Member;
                if (member.MemberType == MemberTypes.Property)
                {
                    var property = member as PropertyInfo;
                    SetResultType(property.PropertyType);
                }
                else if (member.MemberType == MemberTypes.Field)
                {
                    var field = member as FieldInfo;
                    SetResultType(field.FieldType);
                }

                return node;
            }
            protected override Expression VisitMethodCall(MethodCallExpression node)
            {
                SetResultType(node.Method.ReturnType);
                return node;
            }
            private void SetResultType(Type type)
            {
                var underlying = InternalHelper.GetUnderlyingType(type);
                _resultType = underlying;
            }

        }

        protected sealed class ExpressionNodeResolver : ExpressionVisitor
        {
            public bool IsParameterMemberExpression { get; private set; }
            public bool IsMemberHardword { get; private set; }

            public bool IsFuncExpression { get; private set; }
            public HardWordAttribute HardWord { get; private set; }
            protected override Expression VisitMember(MemberExpression node)
            {
                if (node.Expression != null && node.Expression.NodeType == ExpressionType.Parameter)
                {
                    IsParameterMemberExpression = true;
                    var property = node.Member as PropertyInfo;
                    if (property != null)
                    {
                        var hd = HardWordManager.Get(property);
                        HardWord = hd;
                        IsMemberHardword = hd != null;
                    }
                    return node;
                }
                return base.VisitMember(node);
            }
            protected override Expression VisitMethodCall(MethodCallExpression node)
            {
                if (node.Object == null && node.Method.DeclaringType == typeof(SqlStatement) && node.Method.Name == "Func")
                {
                    IsFuncExpression = true;
                    return node;
                }
                return base.VisitMethodCall(node);
            }
        }
        #endregion

        #region Dispose
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        ~DataAccessObjectBase()
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
            if (_dbAction != null)
            {
                _dbAction.Dispose();
                _dbAction = null;
            }
        }



        #endregion
    }
}

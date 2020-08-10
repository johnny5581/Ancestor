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

namespace Ancestor.DataAccess.DAO
{
    /// <summary>
    /// DataAccessObject base class
    /// </summary>
    public abstract class DataAccessObjectBase : IDataAccessObjectEx, IInternalDataAccessObject, IIdentifiable
    {
        private readonly Guid _id = Guid.NewGuid();
        private readonly DBObject _dbObject;
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
        public DataAccessObjectBase(DBObject dbObject)
        {
            _dbObject = dbObject;
            ParameterPrefix = dbObject.ParameterPrefix;
            ParameterPostfix = dbObject.ParameterPostfix;
            _dbAction = CreateDbAction(dbObject);
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
            get { return _dbAction.IsTransacting; }
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
        public abstract string DateTimeSymbol{ get; }

        /// <summary>
        /// Update mode setting
        /// </summary>
        public UpdateMode UpdateMode { get; set; }
        /// <summary>
        /// Flag to raise exception if error
        /// </summary>
        public bool RaiseException
        {
            get { return _raiseExp ?? GlobalSetting.RaiseException; }
            set { _raiseExp = value; }
        }

        IDbConnection IDataAccessObjectEx.DBConnection
        {
            get { return _dbAction.Connection; }
        }
        public IDbAction DbAction
        {
            get { return _dbAction; }
        }
        public DBObject DbObject
        {
            get { return _dbObject; }
        }

        #endregion Property

        IDataAccessObjectEx IDataAccessObjectEx.Clone()
        {
            return new DAOFactoryEx(_dbObject).GetDataAccessObjectFactory();
        }
        protected abstract IDbAction CreateDbAction(DBObject dbObject);
        protected DbActionOptions CreateDbOptions(AncestorOptions options)
        {
            return CreateDbOptions(options ?? new AncestorOptions(), _dbAction.CreateOptions());
        }
        protected abstract DbActionOptions CreateDbOptions(AncestorOptions options, DbActionOptions dbOptions);
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
            throw new InvalidOperationException("can not get AncestorResult");
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
            _dbAction.BeginTransaction();
        }

        public void BeginTransaction(IsolationLevel isoLationLevel)
        {
            _dbAction.BeginTransaction(isoLationLevel);
        }

        public void Commit()
        {
            _dbAction.Commit();
        }

        public void Rollback()
        {
            _dbAction.Rollback();
        }

        public void Open()
        {
            _dbAction.OpenConnection();
        }

        public void Close()
        {
            _dbAction.CloseConnection();
        }
        public virtual AncestorResult QueryFromSqlString(string sql, object parameter, Type dataType, bool firstOnly, AncestorOptions options)
        {
            return TryCatch(() =>
            {
                var dbParameters = CreateDBParameters(parameter);
                if (options == null)
                    options = new AncestorOptions { HasRowId = false };
                var dbOpts = CreateDbOptions(options);
                return InternalQuery(sql, dbParameters, dataType, firstOnly, dbOpts);
            }, ReturnAncestorResult);
        }

        public virtual AncestorResult QueryFromModel(object model, Type dataType, object origin, bool firstOnly, AncestorOptions options)
        {
            return TryCatch(() =>
            {
                var dbParameters = new DBParameterCollection();

                var reference = GetReferenceInfo(model, null, dataType, origin);
                var selector = CreateSelectCommand(reference);
                var tableName = reference.GetReferenceName();
                var where = CreateWhereCommand(model, tableName, true, dbParameters);
                var opt = CreateDbOptions(options);
                var sql = string.Format("Select {0} From {1} {2}", selector, tableName, where);
                return InternalQuery(sql, dbParameters, dataType, firstOnly, opt);
            }, ReturnAncestorResult);
        }

        public virtual AncestorResult QueryFromLambda(LambdaExpression predicate, LambdaExpression selector, IDictionary<Type, object> proxyMap, bool firstOnly, AncestorOptions options)
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
                var tableText = string.Join(", ", tuples.Select(r => r.Item3));
                if (selectorText == null)
                {
                    selectorText = CreateSelectCommand(mergeResult.Reference);
                }

                var whereText = mergeResult.Sql2 != null ? ("Where " + mergeResult.Sql2) : "";
                var sql = string.Format("Select {0} From {1} {2}", selectorText, tableText, whereText);
                var dataType = mergeResult.Reference.GetReferenceType();
                var opt = CreateDbOptions(options);
                return InternalQuery(sql, mergeResult.Parameters, dataType, firstOnly, opt);
            }, ReturnAncestorResult);
        }

        public AncestorResult GroupFromLambda(LambdaExpression predicate, LambdaExpression selector, LambdaExpression groupBy, IDictionary<Type, object> proxyMap, AncestorOptions options)
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
                var tableText = string.Join(", ", tuples.Select(r => r.Item3));
                if (selectorText == null)
                {
                    selectorText = CreateSelectCommand(mergeResult.Reference);
                }

                var whereText = mergeResult.Sql2 != null ? ("Where " + mergeResult.Sql2) : "";
                var groupText = groupResult != null ? groupResult.Sql : selectorResult.GroupBy;
                var sql = string.Format("Select {0} From {1} {2} Group By {3}", selectorText, tableText, whereText, groupText);
                var dataType = mergeResult.Reference.GetReferenceType();
                var opt = CreateDbOptions(options);
                return InternalQuery(sql, mergeResult.Parameters, null, false, opt);
            }, ReturnAncestorResult);
        }

        public AncestorExecuteResult InsertEntity(object model, object origin, AncestorOptions options)
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
                return _dbAction.ExecuteNonQuery(sql, dbParameters, opt);
            }, ReturnEffectRowResult);
        }

        public AncestorExecuteResult BulkInsertEntities<T>(IEnumerable<T> models, object origin, AncestorOptions options)
        {
            return TryCatch(() =>
            {
                var reference = GetReferenceInfo(null, typeof(T), null, origin);
                var insertInfos = CreateInsertCommands(reference, models, typeof(T));
                var name = reference.GetReferenceName();
                var connCloseFlag = _dbAction.AutoCloseConnection;
                var raiseError = false;
                if (options != null)
                    raiseError = options.BulkStopWhenError;
                var successed = 0;
                string field = null;
                try
                {
                    _dbAction.OpenConnection();
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
                            _dbAction.ExecuteNonQuery(insertSql, insertInfo.Item3);
                            successed++;
                        }
                        catch
                        {
                            if (raiseError)
                                throw;
                        }
                    }
                }
                finally
                {
                    if (connCloseFlag)
                    {
                        _dbAction.CloseConnection();
                        _dbAction.AutoCloseConnection = connCloseFlag;
                    }
                }
                return successed;
            }, ReturnEffectRowResult);
        }
        public AncestorExecuteResult UpdateEntity(object model, object whereObject, UpdateMode mode, object origin, int exceptRows, AncestorOptions options)
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
                return _dbAction.ExecuteNonQuery(sql, dbParameters, opt);
            }, ReturnEffectRowResult, exceptRows);
        }
        public AncestorExecuteResult UpdateEntity(object model, LambdaExpression predicate, UpdateMode mode, object origin, int exceptRows, AncestorOptions options)
        {
            return TryCatch(() =>
            {
                var dbParameters = new DBParameterCollection();
                var reference = GetReferenceInfo(model, null, predicate.Parameters[0].Type, origin);
                var updateCommand = CreateUpdateCommand(reference, model, mode, dbParameters);
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
                var sql = string.Format("Update {0} Set {1} {2}", name, updateCommand, whereCommand);
                return _dbAction.ExecuteNonQuery(sql, dbParameters, opt);
            }, ReturnEffectRowResult, exceptRows);
        }
        // TODO
        public AncestorExecuteResult DeleteEntity(object whereObject, object origin, int exceptRows, AncestorOptions options)
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
                return _dbAction.ExecuteNonQuery(sql, dbParameters, opt);
            }, ReturnEffectRowResult, exceptRows);
        }
        public AncestorExecuteResult DeleteEntity(LambdaExpression predicate, object origin, int exceptRows, AncestorOptions options)
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
                }

                var whereCommand = result != null ? "Where " + result.Sql : "";
                var opt = CreateDbOptions(options);
                var sql = string.Format("Delete From {0} {1}", name, whereCommand);
                return _dbAction.ExecuteNonQuery(sql, dbParameters, opt);
            }, ReturnEffectRowResult, exceptRows);
        }
        public AncestorExecuteResult ExecuteNonQuery(string sql, object parameter, int exceptRows, AncestorOptions options)
        {
            return TryCatch(() =>
            {
                var dbParameters = CreateDBParameters(parameter);
                var dbOpt = CreateDbOptions(options);
                return _dbAction.ExecuteNonQuery(sql, dbParameters, dbOpt);
            }, ReturnEffectRowResult, exceptRows);
        }

        public AncestorExecuteResult ExecuteStoredProcedure(string name, object parameter, AncestorOptions options)
        {
            return TryCatch(() =>
            {
                var dbParameters = CreateDBParameters(parameter);
                var dbOpt = CreateDbOptions(options);
                return _dbAction.ExecuteStoreProcedure(name, dbParameters, dbOpt);
            }, ReturnAncestorExecuteResult);
        }
        public AncestorExecuteResult ExecuteScalar(string sql, object parameter, AncestorOptions options)
        {
            return TryCatch(() =>
            {
                var dbParameters = CreateDBParameters(parameter);
                var dbOpt = CreateDbOptions(options);
                return _dbAction.ExecuteScalar(sql, dbParameters, dbOpt);
            }, ReturnAncestorExecuteResult);
        }
        protected virtual DBParameterCollection CreateDBParameters(object parameterObject)
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
                CreateDBParameterFromDictionary(parameterDictionary, ref parameters);
            else if (parameterObject != null)
                CreateDBParameterFromProperty(parameterObject, ref parameters);
            return parameters;
        }


        protected ParameterInfo CreateParameter(object value, string parameterNameSeed, bool symbol, string prefix = null, string postfix = null, HardWordAttribute hardWord = null)
        {
            var pname = CreateParameterName(parameterNameSeed, symbol, prefix, postfix);
            var sysDate = false;
            if (value is DateTime && value != null && (DateTime)value == Server.SysDate)
            {
                pname = DateTimeSymbol;
                value = null;
                sysDate = true;
            }
            else if (hardWord != null)
                pname = ConvertToHardWord(pname, hardWord);
            return new ParameterInfo(pname, value, sysDate);
        }


        public virtual string ConvertFromHardWord(string name, HardWordAttribute attribute)
        {
            return name;
        }
        public virtual string ConvertToHardWord(string name, HardWordAttribute attribute)
        {
            return name;
        }
        //public virtual string GetServerTime()
        //{
        //    return "UNDEFINED_SERVER_TIME";
        //}

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
            var refProperties = TableManager.GetBrowsableProperties(referenceType);
            foreach (var refProperty in refProperties)
            {
                var dataProperty = dataType.GetProperty(refProperty.Name);
                if (dataProperty != null)
                {
                    var value = dataProperty.GetValue(model, null);
                    var hd = HardWordManager.Get(refProperty);
                    var fname = TableManager.GetName(refProperty);
                    var oarameter = CreateParameter(value, fname, true, hardWord: hd);
                    fields.Add(fname);
                    values.Add(oarameter.ValueName);
                    if (!oarameter.IsSysDateConverted)
                        parameters.Add(oarameter.ValueName, oarameter.Value);
                }

            }
            return Tuple.Create(string.Join(", ", fields), string.Join(", ", values));
        }
        protected virtual IEnumerable<Tuple<string, string, DBParameterCollection>> CreateInsertCommands(ReferenceInfo info, IEnumerable models, Type dataType)
        {
            var fields = new List<string>();
            var referenceType = info.GetReferenceType();
            var refProperties = TableManager.GetBrowsableProperties(referenceType);
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
                            parameters.Add(parameter.ValueName, parameter.Value);
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
            if (typeof(IDictionary<string, object>).IsAssignableFrom(referenceType) && map != null)
            {
                foreach(var key in map.Keys)
                {
                    var value = map[key];
                    var fname = key;
                    if(value != null)
                    {
                        var parameter = CreateParameter(value, fname, true, UpdateParameterPrefix);
                        fieldMap.Add(fname, parameter.ValueName);
                        if (!parameter.IsSysDateConverted)
                            parameters.Add(parameter.ValueName, parameter.Value);
                    }
                    else if(mode == UpdateMode.All)
                    {
                        fieldMap.Add(fname, "NULL");
                    }                    
                }
            }
            else if(map != null)
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
                            parameters.Add(parameter.ValueName, parameter.Value);
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
                            parameters.Add(parameter.ValueName, parameter.Value);
                    }
                    else if (mode == UpdateMode.All)
                    {
                        fieldMap.Add(fname, "NULL");
                    }
                }
            }
            
            return string.Join(", ", fieldMap.Select(kv => string.Format("{0} = {1}", kv.Key, kv.Value)));
        }
        private void CreateDBParameterFromDictionary(IDictionary<string, object> dic, ref DBParameterCollection collection)
        {
            collection.AddRange(dic.Select(r => new DBParameter(r.Key.ToUpper(), r.Value)));
        }
        private void CreateDBParameterFromProperty(object model, ref DBParameterCollection collection)
        {
            var modelType = model.GetType();
            var properties = modelType.GetProperties();
            foreach (var property in properties)
            {
                if (property.CanRead)
                {
                    var value = property.GetValue(model, null);
                    var parameter = new DBParameter(property.Name, value);
                    collection.Add(parameter);
                }
            }
        }
        private string CreateParameterName(string name, bool symbol, string prefix = null, string postfix = null)
        {
            return string.Format("{1}{2}{0}{3}", name, symbol ? ParameterSymbol : "", prefix ?? ParameterPrefix, postfix ?? ParameterPostfix);
        }
        private DbActionResult InternalQuery(string sql, DBParameterCollection dbParameters, Type dataType, bool firstOnly, DbActionOptions options)
        {
            if (dataType != null)
            {
                if (firstOnly)
                    return _dbAction.QueryFirst(sql, dbParameters, dataType, options);
                else
                    return _dbAction.Query(sql, dbParameters, dataType, options);
            }
            else
            {
                if (firstOnly)
                    return _dbAction.QueryFirst(sql, dbParameters, options);
                else
                    return _dbAction.Query(sql, dbParameters, options);
            }
        }

        private ReferenceInfo GetReferenceInfo(object model, Type modelType, Type dataType, object origin)
        {
            var reference = new ReferenceInfo();
            var tuple = CreateReferenceTuple(model, modelType, dataType, origin);
            reference.Add(null, tuple.Item1, tuple.Item2);
            return reference;
        }

        private ReferenceInfo GetReferenceInfo(IEnumerable<Type> types, IDictionary<Type, object> proxyMap)
        {
            var reference = new ReferenceInfo();
            Tuple<Type, string> tuple = null;
            foreach (var type in types)
            {
                object origin = null;
                if (proxyMap != null)
                {
                    if (proxyMap.TryGetValue(type, out origin))
                    {
                        tuple = CreateReferenceTuple(null, null, null, origin);
                        reference.Add(type, tuple.Item1, tuple.Item2);
                        continue;
                    }
                }
                tuple = CreateReferenceTuple(null, null, type, origin);
                reference.Add(type, tuple.Item1, tuple.Item2);
            }
            return reference;
        }
        private static Tuple<Type, string> CreateReferenceTuple(object model, Type modelType, Type dataType, object origin)
        {
            if (modelType == null && model != null)
                modelType = model.GetType();
            var originType = origin as Type;

            var refType = GetReferenceType(modelType, dataType, originType);
            var refName = GetReferenceName(modelType, dataType, originType, origin as string);
            return Tuple.Create(refType, refName);
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
            private List<string> _tables;
            private List<Type> _dataTypes;
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
                    _tables = new List<string>();
                    _dataTypes = new List<Type>();
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
                        proxyType = _reference.GetReferenceType(p.Type);
                        if (proxyType != null && _dataTypes.Contains(proxyType))
                            _dataTypes.Add(proxyType);
                        else if (!_dataTypes.Contains(p.Type))
                            _dataTypes.Add(p.Type);
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

            protected virtual void ProcessUnaryNot(Expression node)
            {
                using (var scope = CreateScope())
                {
                    Write("NOT ");
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
                        Write("NOT ");
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
                        ProcessBinaryComparison(node.Left, node.Right, IsNullConstant(node.Right) ? "Is" : "=");
                        break;
                    case ExpressionType.NotEqual:
                        ProcessBinaryComparison(node.Left, node.Right, IsNullConstant(node.Right) ? "Is Not" : "<>");
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
            protected virtual void ProcessConstant(object value)
            {
                WriteParameter(value);
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
            protected abstract void ProcessTruncateMethodCall(Expression nodeObject);
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
                        ProcessConvertToString(fromType, objectNode, args);
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

            protected abstract void ProcessConvertToString(Type fromType, Expression objectNode, ReadOnlyCollection<Expression> args);
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
                    tableName = TableManager.GetTableName(proxyObj);
                    var dataType = proxyObj as Type;
                    if (dataType != null && !_dataTypes.Contains(dataType))
                        _dataTypes.Add(dataType);
                }
                else
                {
                    tableName = TableManager.GetName(parameterExpressionType);
                    if (!_dataTypes.Contains(parameterExpressionType))
                        _dataTypes.Add(parameterExpressionType);
                }
                if (!_tables.Any(r => r.Equals(tableName, StringComparison.OrdinalIgnoreCase)))
                    _tables.Add(tableName);
                return tableName;
            }

            protected bool IsParameterMemberExpression(Expression node)
            {
                var visitor = new ExpressionMemberResolver();
                visitor.Visit(node);
                return visitor.IsParameterMemberExpression;
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
                if (!IsParameterMemberExpression(node))
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
                for(var index= 0; index < node.Bindings.Count; index++)
                {
                    var binding = node.Bindings[index];
                    var member = binding.Member;
                    var memberAssignment = binding as MemberAssignment;
                    if(memberAssignment != null)
                    {
                        if (_option.NewAs)
                        {
                            Write("(");
                            Visit(memberAssignment.Expression);
                            Write(") As ");
                            Write(member.Name);
                        }
                        else
                            Visit(memberAssignment.Expression);
                        if (index != node.Bindings.Count - 1)
                            Write(", ");
                    }

                }
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
                        Write(", ");
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
                        Write("(");
                        Visit(argument);
                        Write(") As ");
                        Write(member.Name);
                    }
                    else
                        Visit(argument);                    
                    if (index != node.Members.Count - 1)
                        Write(", ");
                }
            }
            #endregion

            private static ReadOnlyCollection<Expression> CreateReadOnlyCollection(IEnumerable<Expression> expressions)
            {
                return new ReadOnlyCollection<Expression>(expressions.ToList());
            }

            protected virtual void Write(string text)
            {
                var sb = StringBuilder;
                if (sb.Length > 0 && sb[sb.Length - 1] != ' ' && !text.StartsWith(" "))
                    sb.Append(" ");
                sb.Append(text);
            }
            protected virtual void Write(string format, params string[] args)
            {
                var text = string.Format(format, args);
                Write(text);
            }
            protected virtual ParameterInfo GetParameter(object value)
            {
                var name = _index.ToString();
                var parameter = DataAccessObject.CreateParameter(value, name, true);
                if (!parameter.IsSysDateConverted)
                    MoveNextParameter();
                return parameter;
            }
            protected virtual void Write(ExpressionResolveResult result)
            {
                Write(result.Sql);
                _dbParameters.AddRange(result.Parameters);
            }
            protected virtual DBParameter WriteParameter(object value)
            {
                var parameter = GetParameter(value);
                return WriteParameter(parameter);
            }
            protected virtual DBParameter WriteParameter(ParameterInfo parameter)
            {
                DBParameter p = null;
                if (!parameter.IsSysDateConverted)
                    p = _dbParameters.Add(parameter.ValueName, parameter.Value);
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
                var parameters = values.Select(v => GetParameter(v)).ToArray();
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
                var scope = _scope == null ? new ExpressionScope(this) : new ExpressionScope(this, _scope);
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
                            extraSql = extraSql.Replace(extraParameters[i].Name + " ", name + " ");
                        extraParameters[i].Name = name;
                    }
                    merged.Sql2 = extraSql;
                    merged.Parameters.AddRange(extraParameters);
                    merged.Reference.Add(result2.Reference);
                }
                return merged;
            }
            public class ExpressionResolveOption
            {

                public static readonly ExpressionResolveOption Default = new ExpressionResolveOption();
                public static readonly ExpressionResolveOption GroupBy = new ExpressionResolveOption { AppendAs = false, UseHardWord = false, NewAs = false, };
                public static readonly ExpressionResolveOption Selector = new ExpressionResolveOption { AppendAs = false, UseHardWord = true, NewAs = true };

                public ExpressionResolveOption()
                {
                    AppendAs = true;
                    UseHardWord = true;
                    NewAs = true;
                }
                public bool AppendAs { get; set; }
                public bool UseHardWord { get; set; }
                public bool NewAs { get; set; }
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
                private StringBuilder _sb = new StringBuilder().Append("(");
                private bool _disposed = false;



                public ExpressionScope(ExpressionResolver resolver)
                {
                    _resolver = resolver;
                }
                public ExpressionScope(ExpressionResolver resolver, ExpressionScope parent)
                {
                    _resolver = resolver;
                    Parent = parent;
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
                            AppendText(")");
                            StringBuilder sb = IsRoot ? _resolver._sb : Parent.StringBuilder;
                            var text = _sb.ToString();
                            //if (sb.Length != 0 && sb[sb.Length - 1] != ' ')
                            //    sb.Append(" ");
                            //sb.Append(text);
                            AppendText(sb, text);
                            _resolver._scope = !IsRoot ? Parent : null;
                        }
                        _disposed = true;
                    }
                }

                public string GetText()
                {
                    return _sb.ToString() + ")";
                }
                private void AppendText(StringBuilder sb, string text)
                {
                    if (sb.Length > 0 && sb[sb.Length - 1] != ' ' && !text.StartsWith(" "))
                        sb.Append(" ");
                    sb.Append(text);
                }
                private void AppendText(string text)
                {
                    AppendText(_sb, text);
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

        protected sealed class ExpressionMemberResolver : ExpressionVisitor
        {
            public bool IsParameterMemberExpression { get; private set; }
            protected override Expression VisitMember(MemberExpression node)
            {
                if (node.Expression != null && node.Expression.NodeType == ExpressionType.Parameter)
                {
                    IsParameterMemberExpression = true;
                    return node;
                }
                return base.VisitMember(node);
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

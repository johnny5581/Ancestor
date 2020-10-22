using Ancestor.Core;
using Ancestor.DataAccess.DAO;
using Ancestor.DataAccess.DBAction.Mapper;
using Ancestor.DataAccess.DBAction.Options;
using Microsoft.Win32;
using Oracle.ManagedDataAccess.Client;
using Oracle.ManagedDataAccess.Types;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Ancestor.DataAccess.DBAction
{
    public class ManagedOracleAction : DbActionBase
    {
        private static string _LastTnsLocation;
        private static readonly ConcurrentDictionary<string, string> TnsNamesMap
            = new ConcurrentDictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        private static readonly Dictionary<string, OracleDbType> TypeNameMap
           = new Dictionary<string, OracleDbType>(StringComparer.OrdinalIgnoreCase)
           {
                { "BYTE[]", OracleDbType.Blob },
                { "CHAR[]", OracleDbType.Clob },
                { "NUMBER", OracleDbType.Decimal },
                { "VARCHAR2", OracleDbType.Varchar2 },
                { "SYSTEM.STRING", OracleDbType.Varchar2 },
                { "STRING", OracleDbType.Varchar2 },
                { "SYSTEM.DATETIME", OracleDbType.Date },
                { "DATETIME", OracleDbType.Date },
                { "DATE", OracleDbType.Date },
                { "INT64", OracleDbType.Int64 },
                { "INT32", OracleDbType.Int32 },
                { "INT16", OracleDbType.Int16 },
                { "BYTE", OracleDbType.Byte },
                { "DECIMAL", OracleDbType.Decimal },
                { "FLOAT", OracleDbType.Single },
                { "DOUBLE", OracleDbType.Double },
                { "CHAR", OracleDbType.Char },
                { "TIMESTAMP", OracleDbType.TimeStamp },
                { "REFCURSOR", OracleDbType.RefCursor },
                { "CLOB", OracleDbType.Clob },
                { "LONG", OracleDbType.Long },
           };
        public ManagedOracleAction(DataAccessObjectBase dao, DBObject dbObject) : base(dao, dbObject)
        {
        }

        #region Protected / Private
        protected override IDbDataAdapter CreateAdapter(IDbCommand command)
        {
            return new OracleDataAdapter((OracleCommand)command);
        }
        protected override DbActionOptions CreateOption()
        {
            return new OracleOptions();
        }

        protected override IDbConnection CreateConnection(DBObject dbObject)
        {
            var dsn = dbObject.IP ?? dbObject.Node;
            var connStrBuilder = new OracleConnectionStringBuilder();
            if (dbObject.ConnectedMode == DBObject.Mode.Direct)
                connStrBuilder.DataSource = string.Format(@"(DESCRIPTION = (CONNECT_DATA = (SERVER=DEDICATED)(SERVICE_NAME = {0}))(ADDRESS_LIST = (ADDRESS =  (COMMUNITY = tcp.world)(PROTOCOL = TCP)(Host = {1})(Port = 1521))(ADDRESS = (COMMUNITY = tcp.world)(PROTOCOL = TCP)(Host = {1})(Port = 1526))))", dbObject.Node, dsn);
            else if (dbObject.ConnectedMode == DBObject.Mode.DSN)
                connStrBuilder.DataSource = dbObject.Node;
            else if (dbObject.ConnectedMode == DBObject.Mode.TNSNAME)
            {
                if (GlobalSetting.TnsnamesPath == null && GlobalSetting.SystemTnsnamesPath == null)
                {
                    string tnsnamesPath = "";
                    // try to find tnsnames.ora file for resolve name alias
                    var oracleHome = FindOracleHome();
                    if (oracleHome != null)
                    {
                        var path = Path.Combine(oracleHome, "NETWORK", "ADMIN", "TNSNAMES.ora");
                        if (File.Exists(path))
                            tnsnamesPath = path;
                    }
                    GlobalSetting.SystemTnsnamesPath = tnsnamesPath;
                }
                lock (TnsNamesMap)
                {
                    if (!string.IsNullOrEmpty(GlobalSetting.ManagedOracleTnsNamesLocation) && _LastTnsLocation != GlobalSetting.ManagedOracleTnsNamesLocation)
                    {
                        TnsNamesMap.Clear();
                        // parse tnsnames.ora to TnsNamesMap
                        var stack = new Stack<StringBuilder>();
                        var sb = new StringBuilder();
                        int c;
                        using (var fs = File.OpenRead(GlobalSetting.ManagedOracleTnsNamesLocation))
                        using (var sr = new StreamReader(fs))
                        {
                            while ((c = sr.Read()) != -1)
                            {
                                switch ((char)c)
                                {
                                    case '#':
                                        sr.ReadLine();
                                        continue;
                                    case '(':
                                        stack.Push(sb);
                                        sb = new StringBuilder();
                                        break;
                                    case ')':
                                        var t = stack.Pop();
                                        if (stack.Count == 0)
                                        {
                                            var s = t.ToString();
                                            var splited = s.Split(new[] { '=' }, StringSplitOptions.RemoveEmptyEntries);
                                            if (splited.Length > 0)
                                            {
                                                var name = splited[0].Trim();
                                                var dns = sb.Insert(0, "(").Append(")").ToString();
                                                dns = Regex.Replace(dns, @"[\r|\n|\s]", string.Empty);
                                                TnsNamesMap.AddOrUpdate(name, dns, (n, v) => dns);
                                            }
                                            sb.Clear();

                                        }
                                        else
                                        {
                                            t.Append(sb.Insert(0, "(").Append(")").ToString());
                                            sb = t;
                                        }
                                        break;
                                    case '\r':
                                    case '\n':
                                        continue;
                                    default:
                                        sb.Append((char)c);
                                        break;
                                }

                            }
                        }

                        _LastTnsLocation = GlobalSetting.ManagedOracleTnsNamesLocation;
                    }
                }
                var key = TnsNamesMap.Keys.FirstOrDefault(k => k.StartsWith(dbObject.Node, StringComparison.OrdinalIgnoreCase));
                if (key != null)
                    connStrBuilder.DataSource = TnsNamesMap[key];
            }

            connStrBuilder.UserID = dbObject.ID;
            connStrBuilder.Password = dbObject.Password;

            var extra = dbObject.ConnectionString as OracleConnectionString ?? new OracleConnectionString();
            var ps = typeof(OracleConnectionString).GetProperties().Select(r =>
            {
                var attrs = r.GetCustomAttributes(false).OfType<Attribute>();
                return new
                {
                    Value = r.GetValue(extra, null),
                    DisplayName = InternalHelper.GetValue<DisplayNameAttribute, string>(attr => attr.DisplayName, attrs),
                    DefaultValue = InternalHelper.GetValue<DefaultValueAttribute, object>(attr => attr.Value, attrs),
                };
            });

            foreach (var p in ps)
            {
                var value = p.Value ?? p.DefaultValue;
                if (value != null)
                {
                    typeof(OracleConnectionStringBuilder).GetProperty(p.DisplayName).SetValue(connStrBuilder, value, null);
                }
            }

            return new OracleConnection(connStrBuilder.ConnectionString);
        }

        private static string FindOracleHome()
        {
            var oracleHome = System.Environment.GetEnvironmentVariable("ORACLE_HOME");
            if (oracleHome == null)
            {
                var path = System.Environment.GetEnvironmentVariable("Path");
                if (path != null)
                {
                    var paths = path.Split(';');
                    foreach (var p in paths)
                    {
                        var netadmin = Path.Combine(p, "..", "NETWORK", "ADMIN");
                        if (Directory.Exists(netadmin))
                        {
                            oracleHome = Path.GetDirectoryName(p);
                            break;
                        }
                    }
                }

                if (oracleHome == null)
                {
                    string registryPath = System.Environment.Is64BitOperatingSystem
                        ? "SOFTWARE\\WOW6432Node\\ORACLE\\ALL_HOMES"
                        : "SOFTWARE\\ORACLE\\ALL_HOMES";
                    var registry = Registry.LocalMachine.OpenSubKey(registryPath);
                    if (registry != null)
                    {
                        var lastHomeId = (string)registry.GetValue("LAST_HOME");
                        if (lastHomeId != null)
                            oracleHome = (string)registry.GetValue("ID" + lastHomeId + "\\PATH");
                        else
                        {
                            var defaultHome = (String)registry.GetValue("DEFAULT_HOME");
                            if (defaultHome != null)
                            {
                                foreach (var name in registry.GetValueNames())
                                {
                                    if ((string)registry.GetValue(name + "\\NAME") == defaultHome)
                                    {
                                        oracleHome = (string)registry.GetValue(name + "\\PATH");
                                        break;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return oracleHome;
        }

        protected override IDbDataParameter CreateParameter(DBParameter parameter, DbActionOptions options)
        {
            var p = new OracleParameter(parameter.Name, parameter.Value);
            if (!parameter.ParameterType.IsLazy)
                p.OracleDbType = GetParameterType(parameter.ParameterType);
            if (parameter.Size != null)
                p.Size = parameter.Size.Value;
            p.Direction = parameter.ParameterDirection;
            var opt = options as OracleOptions;
            switch (p.OracleDbType)
            {
                case OracleDbType.Long:
                case OracleDbType.LongRaw:
                    opt.InitializeLONGFetchSize = -1;
                    break;
                case OracleDbType.Clob:
                case OracleDbType.Blob:
                    opt.InitializeLOBFetchSize = -1;
                    break;
            }
            return p;
        }

        protected override AncestorException CreateAncestorException(Exception innerException, QueryParameter parameter)
        {
            var oracleException = innerException as OracleException;
            if (oracleException != null)
                return CreateAncestorException(oracleException.ErrorCode, oracleException.Message, oracleException, parameter);
            return base.CreateAncestorException(innerException, parameter);
        }

        protected override SqlMapper.IDynamicParameters CreateDynamicParameters(IEnumerable<DBParameter> parameters)
        {
            var dynamicParameters = new OracleDynamicParameters();
            OracleDbType dbType;
            foreach (var p in parameters)
            {

                var pType = p.ParameterType;
                if (pType.IsDbType)
                {
                    DbType? type = null;
                    if (!pType.IsLazy) // normal parameter
                    {
                        type = (DbType)pType.Code;
                    }
                    dynamicParameters.Add(p.Name, p.Value, type, p.ParameterDirection, p.Size);
                }
                else if (TryGetOracleDbType(pType.Name, out dbType))
                {
                    dynamicParameters.Add(p.Name, p.Value, dbType, p.ParameterDirection, p.Size);
                }
                else if (pType.IsLazy)
                    dynamicParameters.Add(p.Name, p.Value);
                else
                    throw new InvalidOperationException("invalidate parameter type: " + p);
            }
            return dynamicParameters;
        }
        protected override void PreExecute(IDbCommand command, DbActionOptions options)
        {
            var cmd = (OracleCommand)command;
            var opt = options as OracleOptions;
            if (cmd != null && opt != null)
                BindOptions(cmd, opt);
            base.PreExecute(command, options);
        }
        protected override void PreQuery(IDbCommand command, DbActionOptions options)
        {
            var cmd = (OracleCommand)command;
            var opt = options as OracleOptions;
            if (cmd != null && opt != null)
                BindOptions(cmd, opt);
            base.PreQuery(command, options);
        }
        protected override void RestoreParameter(IDbDataParameter dbDataParameter, DBParameter dbParameter)
        {
            var oracleParameter = dbDataParameter as OracleParameter;
            if (oracleParameter != null)
            {
                switch (oracleParameter.OracleDbType)
                {
                    case OracleDbType.Varchar2:
                        oracleParameter.Value = GetString(oracleParameter.Value);
                        break;
                    case OracleDbType.Int16:
                    case OracleDbType.Int32:
                    case OracleDbType.Int64:
                    case OracleDbType.Single:
                    case OracleDbType.Double:
                    case OracleDbType.Decimal:
                        oracleParameter.Value = GetDecimal(oracleParameter.Value, oracleParameter.OracleDbType);
                        break;
                    case OracleDbType.Clob:
                        oracleParameter.Value = GetClob(oracleParameter.Value);
                        break;
                    case OracleDbType.RefCursor:
                        var table = GetRefCursor(oracleParameter.Value);
                        if (dbParameter.ItemType != null) // convert to list                        
                        {
                            var enumerator = AncestorResultHelper.TableToCollection(table, dbParameter.ItemType, null, false, ResultListMode.All, dbParameter.ItemEncoding);
                            var list = (IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(dbParameter.ItemType));
                            var e = enumerator.GetEnumerator();
                            while (e.MoveNext())
                                list.Add(e.Current);
                            oracleParameter.Value = list;
                        }
                        else
                            oracleParameter.Value = table;
                        break;
                }
            }
            base.RestoreParameter(oracleParameter, dbParameter);
        }
        protected override DataTable CreateDataTable(IDbCommand command, int startIndex = 0, int length = 0)
        {
            var adapter = (OracleDataAdapter)CreateAdapter(command);
            var dataSet = new DataSet();
            adapter.Fill(dataSet, startIndex, length, "TABLE");
            return dataSet.Tables[0];
        }
        private static string GetString(object dbValue)
        {
            if (dbValue != null && !(dbValue is DBNull))
            {
                var oracleString = (OracleString)dbValue;
                if (!oracleString.IsNull)
                    return oracleString.Value;
                if (GlobalSetting.UseOracleStringParameter)
                    return oracleString.ToString();
            }
            return null;
        }
        private static object GetDecimal(object dbValue, OracleDbType code)
        {
            if (dbValue != null)
            {
                var oracleDemical = (OracleDecimal)dbValue;
                if (oracleDemical.IsNull)
                    return null;
                else
                {
                    switch (code)
                    {
                        case OracleDbType.Int16:
                            return (short)oracleDemical;
                        case OracleDbType.Int32:
                            return (int)oracleDemical;
                        case OracleDbType.Int64:
                            return (long)oracleDemical;
                        case OracleDbType.Single:
                            return (float)oracleDemical;
                        case OracleDbType.Double:
                            return (double)oracleDemical;
                        case OracleDbType.Decimal:
                            return (decimal)oracleDemical;
                    }
                }
            }
            return null;
        }
        private static DataTable GetRefCursor(object dbValue)
        {
            if (dbValue != null)
            {
                var adapter = new OracleDataAdapter();
                var table = new DataTable();
                try
                {
                    adapter.Fill(table, (OracleRefCursor)dbValue);
                }
                catch { }
                return table;
            }
            return null;
        }
        private static string GetClob(object dbValue)
        {
            if (dbValue != null)
            {
                var clob = (OracleClob)dbValue;
                using (var sr = new StreamReader(clob, Encoding.Unicode))
                {
                    return sr.ReadToEnd();
                }
            }
            return null;
        }
        private static void BindOptions(OracleCommand cmd, OracleOptions opt)
        {
            cmd.AddRowid = opt.AddRowid;
            cmd.BindByName = opt.BindByName;
            if (opt.InitializeLOBFetchSize != null)
                cmd.InitialLOBFetchSize = opt.InitializeLOBFetchSize.Value;
            if (opt.InitializeLONGFetchSize != null)
                cmd.InitialLONGFetchSize = opt.InitializeLONGFetchSize.Value;
            if (opt.FetchSize != null)
                cmd.FetchSize = opt.FetchSize.Value;
        }
        private static OracleDbType GetParameterType(DBParameterType parameterType)
        {
            OracleDbType type;
            if (parameterType.IsDbType)
            {
                return DbTypeToOracleDbType((DbType)parameterType.Code);
            }
            else if (TryGetOracleDbType(parameterType.Name, out type))
            {
                return type;
            }
            throw new ArgumentException("Can not convert parameter to OracleParameter:" + parameterType);
        }
        private static bool TryGetOracleDbType(string name, out OracleDbType type)
        {
            return Enum.TryParse(name, true, out type) || TypeNameMap.TryGetValue(name, out type);
        }
        private static OracleDbType DbTypeToOracleDbType(DbType dbType)
        {
            switch (dbType)
            {
                case DbType.AnsiString:
                case DbType.String:
                    return OracleDbType.Varchar2;
                case DbType.AnsiStringFixedLength:
                case DbType.StringFixedLength:
                    return OracleDbType.Char;
                case DbType.Byte:
                case DbType.SByte:
                    return OracleDbType.Byte;
                case DbType.UInt16:
                case DbType.Int16:
                    return OracleDbType.Int16;
                case DbType.Int32:
                case DbType.UInt32:
                    return OracleDbType.Int32;
                case DbType.Int64:
                case DbType.UInt64:
                    return OracleDbType.Int64;
                case DbType.Single:
                    return OracleDbType.Single;
                case DbType.Double:
                    return OracleDbType.Double;
                case DbType.Date:
                case DbType.DateTime:
                case DbType.Time:
                    return OracleDbType.Date;
                case DbType.Binary:
                    return OracleDbType.Blob;
                case DbType.VarNumeric:
                case DbType.Decimal:
                case DbType.Currency:
                    return OracleDbType.Decimal;
                case DbType.Object:
                    return OracleDbType.Blob;
                case DbType.Guid:
                    return OracleDbType.Raw;
                case DbType.Boolean:
                default:
                    throw new NotSupportedException("not supported type: " + dbType);
            }
        }
        #endregion Protected / Private



        private class OracleDynamicParameters : SqlMapper.IDynamicParameters
        {
            private readonly DynamicParameters _dynamicParameters = new DynamicParameters();

            private readonly Dictionary<string, OracleParameter> _oracleParameters = new Dictionary<string, OracleParameter>();

            public void Add(string name, object value = null, DbType? dbType = null, ParameterDirection? direction = null, int? size = null)
            {
                _dynamicParameters.Add(name, value, dbType, direction, size);
            }

            public void Add(string name, object value, OracleDbType oracleDbType, ParameterDirection direction, int? size = null)
            {
                var oracleParameter = new OracleParameter(name, oracleDbType, direction);
                oracleParameter.Value = value;
                if (size != null)
                    oracleParameter.Size = size.Value;
                _oracleParameters.Add(name, oracleParameter);
            }

            public void AddParameters(IDbCommand command, SqlMapper.Identity identity)
            {
                ((SqlMapper.IDynamicParameters)_dynamicParameters).AddParameters(command, identity);

                var oracleCommand = command as OracleCommand;

                if (oracleCommand != null)
                {
                    oracleCommand.Parameters.AddRange(_oracleParameters.Values.ToArray());
                }
            }
            public T Get<T>(string name)
            {
                OracleParameter p;
                if (_oracleParameters.TryGetValue(name, out p))
                {
                    return (T)p.Value;
                }
                return _dynamicParameters.Get<T>(name);
            }
        }
    }
}

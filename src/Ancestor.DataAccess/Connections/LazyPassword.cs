using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Ancestor.DataAccess.Connections
{
    public static class LazyPassword
    {
        private static readonly IDictionary<string, string> SchemaPasswords
            = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        private static readonly IDictionary<string, string> Cache
            = new Dictionary<string, string>();
        private static Core.Logging.ILogger logger
            = Core.Logging.Logger.CreateInstance("Ancestor.LazyPassword");

        public static bool IsAvaliable
        {
            get
            {
                try
                {
                    logger.WriteLog(TraceEventType.Verbose, "avaliable checking...");
                    var connectionStringsSection = ConfigurationManager.GetSection("connectionStrings") as ConnectionStringsSection;
                    // must have ConnectionStringsSection
                    if (connectionStringsSection != null)
                    {
                        logger.WriteLog(TraceEventType.Verbose,"section avaliabled, connection string checking...");
                        // must have protected attribute
                        var @protected = connectionStringsSection.SectionInformation.IsProtected;
                        logger.WriteLog(TraceEventType.Verbose,"section protected status: " + @protected);
                        return @protected;
                    }
                    else
                        logger.WriteLog(TraceEventType.Verbose,"section unavaliabled");

                }
                catch (Exception ex)
                {
                    logger.WriteLog(TraceEventType.Verbose,"avaliable checked fail:" + ex.Message);
                }
                return false;
            }
        }
        public static bool GetLazyPasswordEnabled(Core.DBObject dbObject)
        {
            return dbObject.IsLazyPassword ?? Core.AncestorGlobalOptions.GetBoolean("option.lzpw.enable");
        }

        public static string GetPassword(string user, string secret = null, string keyNode = null, string dataSource = null, string connectionString = null, Func<IDbConnection> connFactory = null)
        {
            if (connFactory == null)
            {
                // auto detected connection type
                // if 64bit use managed else use legency oracle
                connFactory = new Func<IDbConnection>(() =>
                {
                    Type connType;
                    if(Environment.Is64BitProcess)
                    {
                        logger.WriteLog(TraceEventType.Verbose, "detect x64 process, use managed oracle");
                        connType = Assembly.Load("Oracle.ManagedDataAccess").GetType("Oracle.ManagedDataAccess.Client.OracleConnection", true, true);
                    }
                    else
                    {
                        logger.WriteLog(TraceEventType.Verbose, "detect x86 process, use legency oracle");
                        connType = Assembly.Load("Oracle.DataAccess").GetType("Oracle.DataAccess.Client.OracleConnection", true, true);
                    }
                    var conn = (IDbConnection)Activator.CreateInstance(connType);
                    return conn;
                });
            }
            using (var conn = connFactory())
            {
                return GetPassword(conn, user, secret, keyNode, dataSource, connectionString);
            }
        }
        public static string GetPassword(Core.DBObject dbObject)
        {

            Func<IDbConnection> connFactory = null;
            switch (dbObject.DataBaseType)
            {
                case Core.DBObject.DataBase.Oracle:
                    connFactory = () =>
                    {
                        var connType = Assembly.Load("Oracle.DataAccess").GetType("Oracle.DataAccess.Client.OracleConnection", true, true);
                        var c = (IDbConnection)Activator.CreateInstance(connType);
                        return c;
                    };
                    break;
                case Core.DBObject.DataBase.ManagedOracle:
                    connFactory = () =>
                    {
                        var connType = Assembly.Load("Oracle.ManagedDataAccess").GetType("Oracle.ManagedDataAccess.Client.OracleConnection", true, true);
                        var c = (IDbConnection)Activator.CreateInstance(connType);
                        return c;
                    };
                    break;
                default:
                    throw new NotSupportedException("notsupported database type:" + dbObject.DataBaseType);
            }
            using (var conn = connFactory())
            {
                return GetPassword(conn, dbObject);
            }

        }


        public static string GetPassword(IDbConnection conn, string user, string secret = null, string keyNode = null, string dataSource = null, string connectionString = null)
        {
            if (user == null)
                throw new NullReferenceException("user can not be null");
            var secretKey = secret ?? GetLazyPasswordSecretKey(user);
            return GetPasswordInternal(conn, user, secretKey, keyNode, dataSource, connectionString);
        }
        public static string GetPassword(IDbConnection conn, Core.DBObject dbObject)
        {
            return GetPassword(conn, dbObject.ID, dbObject.LazyPasswordSecretKey, dbObject.LazyPasswordSecretKeyNode, dbObject.LazyPasswordDataSource, dbObject.LazyPasswordConnectionString);
        }
        internal static string GetPasswordInternal(IDbConnection conn, string user, string secretKey, string keyNode = null, string dataSource = null, string connectionString = null)
        {
            if (conn == null)
                throw new NullReferenceException("conn can not be null");
            if (user == null)
                throw new NullReferenceException("user can not be null");
            if (secretKey == null)
                throw new NullReferenceException("secretKey can not be null, user: " + user);
            logger.WriteLog(TraceEventType.Verbose,"schema=" + user);
            logger.WriteLog(TraceEventType.Verbose,"secretKey=" + secretKey);


            string pwd;

            string connStr = null;
            var gConnStr = Core.AncestorGlobalOptions.GetString("option.lzpw.node.connstr");
            var gDsn = Core.AncestorGlobalOptions.GetString("option.lzpw.node.dsn");
            if (connectionString != null)
            {
                logger.WriteLog(TraceEventType.Verbose,"use DBObject connectionString");
                connStr = connectionString;
            }
            else if (gConnStr != null)
            {
                logger.WriteLog(TraceEventType.Verbose,"use GlobalLazyPasswordConnectionString");
                connStr = gConnStr;
            }
            else
            {
                if (keyNode == null)
                    keyNode = GetLazyPasswordSecretKeyNode(user);
                logger.WriteLog(TraceEventType.Verbose,"keyNode=" + keyNode);

                connStr = ConfigurationManager.ConnectionStrings[keyNode].ConnectionString;
                if (dataSource != null)
                {
                    logger.WriteLog(TraceEventType.Verbose,"use DBObject datasource: " + dataSource);
                    connStr = ReplaceDataSource(conn, connStr, dataSource);
                }
                else if (gDsn != null)
                {
                    logger.WriteLog(TraceEventType.Verbose,"use GlobalLazyPasswordDataSource: " + gDsn);
                    connStr = ReplaceDataSource(conn, connStr, gDsn);
                }
            }

            
            connStr = ReplaceConnectionProperty(conn, connStr);

            logger.WriteLog(TraceEventType.Verbose,"connStr=" + connStr);
            conn.ConnectionString = connStr;

            var cacheKey = string.Format("{0}^{1}", user, connStr);
            if (!SchemaPasswords.TryGetValue(cacheKey, out pwd))
            {
                var opened = !conn.State.HasFlag(ConnectionState.Open);
                try
                {
                    if (opened)
                        conn.Open();
                    using (var cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = "FGET_USER_PASSWORD";
                        cmd.CommandType = CommandType.StoredProcedure;

                        var p0 = cmd.CreateParameter();
                        p0.ParameterName = "RtnVal";
                        p0.DbType = DbType.String;
                        p0.Direction = ParameterDirection.ReturnValue;
                        p0.Size = 200;
                        cmd.Parameters.Add(p0);

                        var p1 = cmd.CreateParameter();
                        p1.ParameterName = "V_SCHEMAUSER";
                        p1.DbType = DbType.String;
                        p1.Value = user;
                        p1.Direction = ParameterDirection.Input;
                        p1.Size = 100;
                        cmd.Parameters.Add(p1);

                        var p2 = cmd.CreateParameter();
                        p2.ParameterName = "V_KEY";
                        p2.DbType = DbType.String;
                        p2.Value = secretKey;
                        p2.Direction = ParameterDirection.Input;
                        p2.Size = 200;
                        cmd.Parameters.Add(p2);

                        cmd.ExecuteNonQuery();

                        var value = p0.Value.ToString();
                        if (value != "null")
                        {
                            pwd = value;
                            logger.WriteLog(TraceEventType.Verbose,"pwd=" + pwd);
                            SchemaPasswords.Add(cacheKey, pwd);
                        }
                        else
                        {
                            SchemaPasswords.Add(cacheKey, null);
                        }
                    }
                }
                catch (Exception ex)
                {
                    logger.WriteLog(TraceEventType.Verbose,ex.ToString());
                }
                finally
                {
                    if (conn.State.HasFlag(ConnectionState.Open) && opened)
                    {

                        logger.WriteLog(TraceEventType.Verbose,"close conection");
                        conn.Close();

                        // check if needs to clear pool, will ignore when using web
                        var clrPool = Core.AncestorGlobalOptions.GetBoolean("option.lzpw.clearpool");
                        if (clrPool)
                        {
                            // clear pool
                            logger.WriteLog(TraceEventType.Verbose, "trying clear pool");
                            var mClearPool = conn.GetType().GetMethod("ClearPool", BindingFlags.Public | BindingFlags.Static);
                            if (mClearPool != null)
                            {
                                logger.WriteLog(TraceEventType.Verbose,"invoke clear pool action");
                                mClearPool.Invoke(null, new object[] { conn });
                            }
                            else
                                logger.WriteLog(TraceEventType.Warning, "clear pool action missing");
                        }
                    }
                }
            }
            return pwd;
        }
        private static string ReplaceDataSource(IDbConnection conn, string connStr, string dataSource)
        {
            if (conn == null)
                throw new ArgumentNullException("conn", "connection cant be null");
            string connStrBuilderTypeName = null;
            if (conn.GetType().FullName == "Oracle.DataAccess.Client.OracleConnection")
                connStrBuilderTypeName = "Oracle.DataAccess.Client.OracleConnectionStringBuilder";
            else if (conn.GetType().FullName == "Oracle.ManagedDataAccess.Client.OracleConnection")
                connStrBuilderTypeName = "Oracle.ManagedDataAccess.Client.OracleConnectionStringBuilder";
            logger.WriteLog(TraceEventType.Verbose,"DbConnectionStringBuilder type: " + connStrBuilderTypeName);
            var connStrBuilderType = conn.GetType().Assembly.GetType(connStrBuilderTypeName, true, true);
            if (connStrBuilderType == null)
                throw new NullReferenceException("can not found type: " + connStrBuilderTypeName);

            var connStrBuilder = (DbConnectionStringBuilder)Activator.CreateInstance(connStrBuilderType, connStr);
            PropertyInfo property = null;
            switch (connStrBuilderTypeName)
            {
                case "Oracle.DataAccess.Client.OracleConnectionStringBuilder":
                case "Oracle.ManagedDataAccess.Client.OracleConnectionStringBuilder":
                    property = connStrBuilderType.GetProperty("DataSource");
                    if (property == null)
                        throw new InvalidOperationException("can not find DataSource property");
                    property.SetValue(connStrBuilder, dataSource, null);
                    break;
            }
            return connStrBuilder.ConnectionString;
        }
        private static string ReplaceConnectionProperty(IDbConnection conn, string connStr)
        {
            if (conn == null)
                throw new ArgumentNullException("conn", "connection cant be null");
            string connStrBuilderTypeName = null;
            if (conn.GetType().FullName == "Oracle.DataAccess.Client.OracleConnection")
                connStrBuilderTypeName = "Oracle.DataAccess.Client.OracleConnectionStringBuilder";
            else if (conn.GetType().FullName == "Oracle.ManagedDataAccess.Client.OracleConnection")
                connStrBuilderTypeName = "Oracle.ManagedDataAccess.Client.OracleConnectionStringBuilder";
            logger.WriteLog(TraceEventType.Verbose,"DbConnectionStringBuilder type: " + connStrBuilderTypeName);
            var connStrBuilderType = conn.GetType().Assembly.GetType(connStrBuilderTypeName, true, true);
            if (connStrBuilderType == null)
                throw new NullReferenceException("can not found type: " + connStrBuilderTypeName);
            var connStrBuilder = (DbConnectionStringBuilder)Activator.CreateInstance(connStrBuilderType, connStr);
            //PropertyInfo propertyMinPool = null;
            PropertyInfo propertyPooling = null;

            switch (connStrBuilderTypeName)
            {
                case "Oracle.DataAccess.Client.OracleConnectionStringBuilder":
                case "Oracle.ManagedDataAccess.Client.OracleConnectionStringBuilder":
                    //propertyMinPool = connStrBuilderType.GetProperty("MinPoolSize");
                    //if (propertyMinPool != null)
                    //    propertyMinPool.SetValue(connStrBuilder, 0, null);
                    propertyPooling = connStrBuilderType.GetProperty("Pooling");
                    if (propertyPooling != null)
                        propertyPooling.SetValue(connStrBuilder, false, null);
                    break;
            }
            return connStrBuilder.ConnectionString;
        }
        private static string GetLazyPasswordSecretKey(string user)
        {
            var secretKeyName = Core.AncestorGlobalOptions.GetString("option.lzpw.prefix") + user.ToUpper();
            var cacheKey = "K:" + secretKeyName;
            string secretKey;
            if (!Cache.TryGetValue(cacheKey, out secretKey))
            {
                secretKey = ConfigurationManager.AppSettings[secretKeyName];
                Cache.Add(cacheKey, secretKey);
            }
            return secretKey;
        }
        private static string GetLazyPasswordSecretKeyNode(string user)
        {
            var secretKeyNodeName = Core.AncestorGlobalOptions.GetString("option.lzpw.node.prefix") + user.ToUpper();
            var cacheKey = "KN:" + secretKeyNodeName;
            string secretKeyNode;
            if (!Cache.TryGetValue(cacheKey, out secretKeyNode))
            {
                secretKeyNode = ConfigurationManager.AppSettings[secretKeyNodeName] ?? Core.AncestorGlobalOptions.GetString("option.lzpw.node");
                Cache.Add(cacheKey, secretKeyNode);
            }
            return secretKeyNode;
        }

    }
}


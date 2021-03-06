﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Ancestor.DataAccess.DBAction
{
    public static class LazyPassword
    {
        private static readonly IDictionary<string, string> SchemaPasswords
            = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        private static readonly IDictionary<string, string> Cache
            = new Dictionary<string, string>();
        public static bool IsAvaliable
        {
            get
            {
                try
                {
                    var connectionStringsSection = ConfigurationManager.GetSection("connectionStrings") as ConnectionStringsSection;
                    // must have ConnectionStringsSection
                    if (connectionStringsSection != null)
                    {
                        Core.AncestorGlobalOptions.Log(null, "LazyPassword", "GetPasswordInternal", "connectionStrings checked");
                        // must have protected attribute
                        var @protected = connectionStringsSection.SectionInformation.IsProtected;
                        Core.AncestorGlobalOptions.Log(null, "LazyPassword", "GetPasswordInternal", "protected: " + @protected);
                        return @protected;
                    }
                }
                catch (Exception ex)
                {
                    if (Core.AncestorGlobalOptions.Debug)
                        Console.WriteLine("avaliable checked fail:" + ex.Message);
                }
                return false;
            }
        }
        public static string GetPassword(string user, string secret = null, string keyNode = null, Func<string, string> connStrFactory = null)
        {
            // auto detected connection type
            // if 64bit use managed else use legency oracle
            var connType = Environment.Is64BitProcess
                ? Assembly.Load("Oracle.ManagedDataAccess").GetType("Oracle.ManagedDataAccess.Client.OracleConnection", true, true)
                : Assembly.Load("Oracle.DataAccess").GetType("Oracle.DataAccess.Client.OracleConnection", true, true);
            var conn = (IDbConnection)Activator.CreateInstance(connType);
            return GetPassword(conn, user, secret, keyNode, connStrFactory);
        }
        public static string GetPassword(IDbConnection conn, string user, string secret = null, string keyNode = null, Func<string, string> connStrFactory = null)
        {
            if (user == null)
                throw new NullReferenceException("user can not be null");
            var secretKey = secret ?? GetLazyPasswordSecretKey(user);
            return GetPasswordInternal(conn, user, secretKey, keyNode, connStrFactory);
        }
        internal static string GetPasswordInternal(IDbConnection conn, string user, string secretKey, string keyNode = null, Func<string, string> connStrFactory = null)
        {
            if (user == null)
                throw new NullReferenceException("user can not be null");
            if (secretKey == null)
                throw new NullReferenceException("secretKey can not be null");

            if (keyNode == null)
                keyNode = GetLazyPasswordSecretKeyNode(user);

            Core.AncestorGlobalOptions.Log(null, "LazyPassword", "GetPasswordInternal", "schema=" + user);
            Core.AncestorGlobalOptions.Log(null, "LazyPassword", "GetPasswordInternal", "secretKey=" + secretKey);
            Core.AncestorGlobalOptions.Log(null, "LazyPassword", "GetPasswordInternal", "keyNode=" + keyNode);



            string pwd;
            if (!SchemaPasswords.TryGetValue(user, out pwd))
            {                
                var opened = !conn.State.HasFlag(ConnectionState.Open);
                
                try
                {
                    if (opened)
                    {
                        var connStr = ConfigurationManager.ConnectionStrings[keyNode].ConnectionString;
                        //var connStr = "User Id=cghuky;Password=uky";
                        Core.AncestorGlobalOptions.Log(null, "LazyPassword", "GetPasswordInternal", "connStr=" + connStr);
                        if (connStrFactory != null)
                        {
                            connStr = connStrFactory(connStr);
                            Core.AncestorGlobalOptions.Log(null, "LazyPassword", "GetPasswordInternal", "connStrFactory=" + connStr);
                        }
                        conn.ConnectionString = connStr;
                        conn.Open();
                    }
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
                            Core.AncestorGlobalOptions.Log(null, "LazyPassword", "GetPasswordInternal", "pwd=" + pwd);
                            SchemaPasswords.Add(user, pwd);
                        }
                        else
                        {
                            SchemaPasswords.Add(user, null);
                        }
                    }
                }
                catch (Exception ex)
                {
                    if (Core.AncestorGlobalOptions.Debug)
                        Core.AncestorGlobalOptions.Log(null, "LazyPassword", "GetPasswordInternal", ex.ToString());
                }
                finally
                {
                    if (conn.State.HasFlag(ConnectionState.Open) && opened)
                        conn.Close();
                }
            }
            return pwd;
        }
        private static string GetLazyPasswordSecretKey(string user)
        {
            var secretKeyName = Core.AncestorGlobalOptions.LazyPasswordSecretKeyPrefix + user.ToUpper();
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
            var secretKeyNodeName = Core.AncestorGlobalOptions.LazyPasswordSecretKeyNodePrefix + user.ToUpper();
            var cacheKey = "KN:" + secretKeyNodeName;
            string secretKeyNode;
            if (!Cache.TryGetValue(cacheKey, out secretKeyNode))
            {
                secretKeyNode = ConfigurationManager.AppSettings[secretKeyNodeName] ?? Core.AncestorGlobalOptions.LazyPasswordSecretKeyNode;
                Cache.Add(cacheKey, secretKeyNode);
            }
            return secretKeyNode;
        }

    }
}


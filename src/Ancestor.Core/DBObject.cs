using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ancestor.Core
{
    /// <summary>
    /// Database profile object
    /// </summary>
    public class DBObject
    {
        /// <summary>
        /// Database IP (direct connect used only)
        /// </summary>
        public string IP { get; set; }
        /// <summary>
        /// Database Hostname 
        /// </summary>
        public string Hostname { get; set; }
        /// <summary>
        /// Database Hostname 
        /// </summary>
        public string Node { get; set; }
        /// <summary>
        /// Catalog (MSSQL used only)
        /// </summary>
        public string Schema { get; set; }
        /// <summary>
        /// Databse user
        /// </summary>
        public string ID { get; set; }
        /// <summary>
        /// Database password
        /// </summary>
        public string Password { get; set; }
        /// <summary>
        /// Database port
        /// </summary>
        public string Port { get; set; }
        /// <summary>
        /// Database type (Predefined)
        /// </summary>
        public DataBase DataBaseType { get; set; }
        /// <summary>
        /// Database connect mode
        /// </summary>
        public Mode ConnectedMode { get; set; }
        /// <summary>
        /// Extra database connection string
        /// </summary>
        public IConnectionString ConnectionString { get; set; }

        /// <summary>
        /// Predefined database type
        /// </summary>
        public enum DataBase
        {
            Oracle,
            MSSQL,
            SQLlite,
            Access,
            MySQL,
            Sybase,
            ManagedOracle,
            OracleClient
        }
        /// <summary>
        /// Connection mode
        /// </summary>
        public enum Mode
        {
            Direct, 
            DSN,
            TNSNAME,
        }
        /// <summary>
        /// Parameter prefix string
        /// </summary>
        public string ParameterPrefix { get; set; }
        /// <summary>
        /// Parameter postfix string
        /// </summary>
        public string ParameterPostfix { get; set; }

        /// <summary>
        /// enable lazy password system
        /// </summary>
        public bool? IsLazyPassword { get; set; }
        /// <summary>
        /// lazy password secret key
        /// </summary>
        public string LazyPasswordSecretKey { get; set; }
        /// <summary>
        /// lazy password secret key connect dsn target
        /// </summary>
        public string LazyPasswordSecretKeyNode { get; set; }
    }
    /// <summary>
    /// Extra connection string interface
    /// </summary>
    public interface IConnectionString
    {
    }
}

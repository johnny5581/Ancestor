using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace Ancestor.Core
{
    /// <summary>
    /// Oracle Connection String (REF:10g)
    /// </summary>
    /// <remarks>https://docs.oracle.com/cd/B19306_01/win.102/b14307/featConnecting.htm</remarks>
    public class OracleConnectionString : IConnectionString
    {
        public OracleConnectionString()
        {
            Pooling = true;
            Max_Pool_Size = 10;
        }
        [DisplayName("ConnectionLifeTime")]
        public int? Connection_Lifetime { get; set; }
        [DisplayName("ConnectionTimeout")]
        public int? Connection_Timeout { get; set; }
        [DisplayName("ContextConnection")]
        public bool? Context_Connection { get; set; }
        [DisplayName("DBAPrivilege")]
        public string DBA_Privilege { get; set; }
        [DisplayName("DecrPoolSize")]
        public string Decr_Pool_Size { get; set; }
        [DisplayName("Enlist")]
        public int? Enlist { get; set; }
        [DisplayName("HAEvents")]
        public bool? HA_Events { get; set; }
        [DisplayName("LoadBalancing")]
        public bool? Load_Balancing { get; set; }
        [DisplayName("IncrPoolSize")]
        public int? Incr_Pool_Size { get; set; }
        [DisplayName("Pooling")]
        public bool? Pooling { set; get; }
        [DisplayName("MaxPoolSize")]
        public int? Max_Pool_Size { get; set; }
        [DisplayName("MinPoolSize")]
        public int? Min_Pool_Size { get; set; }
        [DisplayName("PersistSecurityInfo")]
        public bool? Persist_Security_Info { get; set; }
        [DisplayName("ProxyUserId")]
        public string Proxy_User_Id { get; set; }
        [DisplayName("ProxyPassword")]
        public string Proxy_Password { get; set; }
        [DisplayName("StatementCachePurge")]
        public bool? Statement_Cache_Purge { get; set; }
        [DisplayName("StatementCacheSize")]
        public int? Statement_Cache_Size { get; set; }
        [DisplayName("ValidateConnection")]
        public bool? Validate_Connection { get; set; }

        public object Clone()
        {
            return new OracleConnectionString
            {
                Connection_Lifetime = Connection_Lifetime,
                Connection_Timeout = Connection_Timeout,
                Context_Connection = Context_Connection,
                DBA_Privilege = DBA_Privilege,
                Decr_Pool_Size = Decr_Pool_Size,
                Enlist = Enlist,
                HA_Events = HA_Events,
                Incr_Pool_Size = Incr_Pool_Size,
                Load_Balancing = Load_Balancing,
                Max_Pool_Size = Max_Pool_Size,
                Min_Pool_Size = Min_Pool_Size,
                Persist_Security_Info = Persist_Security_Info,
                Pooling = Pooling,
                Proxy_Password = Proxy_Password,
                Proxy_User_Id = Proxy_User_Id,
                Statement_Cache_Purge = Statement_Cache_Purge,
                Statement_Cache_Size = Statement_Cache_Size,
                Validate_Connection = Validate_Connection
            };
        }
    }
}

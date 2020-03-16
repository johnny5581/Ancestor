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
        [DefaultValue(true)]
        public bool? Pooling { set; get; }
        [DisplayName("MaxPoolSize")]
        [DefaultValue(10)]
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

    }
}

using Ancestor.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ancestor.DataAccess.DAO
{
    /// <summary>
    /// Server object 
    /// </summary>
    public static class Server
    {
        internal static readonly DateTime SYSDATE = DateTime.MinValue.AddTicks(49);
        private static TimeSpan _TimeOffset;
        private static bool _TimeOffsetFlag = false;
        public static bool IsTimeInitialized
        {
            get { return _TimeOffsetFlag; }
        }

        /// <summary>
        /// Server Time (Proxy)
        /// </summary>
        public static DateTime SysDate
        {
            get { return SYSDATE; }
        }

        public static DateTime Now
        {
            get
            {
                if (_TimeOffsetFlag)
                    return DateTime.Now + _TimeOffset;
                throw new InvalidOperationException("ServerTime must be initialized first");
            }
        }

        /// <summary>
        /// Initialize <see cref="Server.DateTime"/> object
        /// </summary>
        public static bool InitializeServerTime(DBObject dbObject)
        {
            using (var dao = new Factory.DAOFactoryEx(dbObject).GetDataAccessObjectFactory())
            {
                var res = dao.ExecuteScalar("SELECT SYSDATE FROM DUAL", null, null);
                if (res.IsSuccess)
                {
                    var time = res.GetValue<DateTime>();
                    if (time.HasValue)
                    {
                        _TimeOffset = time.Value - DateTime.Now;
                        _TimeOffsetFlag = true;
                        return true;
                    }
                }
            }
            return false;
        }
    }
}

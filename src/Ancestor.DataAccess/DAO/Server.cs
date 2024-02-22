using Ancestor.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
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
        public static bool InitializeServerTime(IDataAccessObjectEx dao, out string message)
        {
            var internalDao = dao as IInternalDataAccessObject;
            if (internalDao == null)
            {
                message = "no date time sql";
                return false; // unknown server time sql
            }

            var serverTimeSql = internalDao.GetServerTime();
            var dummyTable = internalDao.GetDummyTable();
            var sql = string.IsNullOrEmpty(dummyTable)
                ? string.Format("Select {0}", serverTimeSql)
                : string.Format("Select {0} From {1}", serverTimeSql, dummyTable);
                
            var res = dao.ExecuteScalar(sql, null, null);
            if (!res.IsSuccess)
            {
                message = res.Message;
                return false; // execute false
            }
            var time = res.GetValue<DateTime>();
            if (!time.HasValue)
            {
                message = "sys time is empty";
                return false;

            }
            message = null;
            _TimeOffset = time.Value - DateTime.Now;
            _TimeOffsetFlag = true;
            return true;
        }
        /// <summary>
        /// Initialize <see cref="Server.DateTime"/> object
        /// </summary>
        public static bool InitializeServerTime(IDataAccessObjectEx dao)
        {
            string message;
            return InitializeServerTime(dao, out message);
        }
        /// <summary>
        /// Initialize <see cref="Server.DateTime"/> object
        /// </summary>
        public static bool InitializeServerTime(DBObject dbObject)
        {
            using (var dao = new Factory.DAOFactoryEx(dbObject).GetDataAccessObjectFactory())
            {
                return InitializeServerTime(dao);
            }
        }
        /// <summary>
        /// Initialize <see cref="Server.DateTime"/> object
        /// </summary>
        public static bool InitializeServerTime(DBObject dbObject, out string message)
        {
            using (var dao = new Factory.DAOFactoryEx(dbObject).GetDataAccessObjectFactory())
            {
                return InitializeServerTime(dao, out message);
            }
        }
    }
}

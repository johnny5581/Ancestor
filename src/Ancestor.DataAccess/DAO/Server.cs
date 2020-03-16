﻿using Ancestor.Core;
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
                throw new InvalidOperationException("Server.DateTime must be initialized first");
            }
        }

        /// <summary>
        /// Initialize <see cref="Server.DateTime"/> object
        /// </summary>
        public static void InitializeServerTime(DBObject dbObject)
        {
            using (var dao = new Factory.DAOFactory(dbObject).GetDataAccessObjectFactory())
            {
                var res = dao.ExecuteScalar("SELECT SYSDATE FROM DUAL", null, null);
                if (res.IsSuccess)
                {
                    var time = res.GetValue<DateTime>();
                    _TimeOffset = time - DateTime.Now;
                    _TimeOffsetFlag = true;
                }
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ancestor.Core
{
    /// <summary>
    /// Log manager for Ancestor
    /// </summary>
    public static class LogManager
    {
        public static bool Enabled
        {
            get { return EnabledLog && EnabledSqlLog; }
            set { EnabledLog = EnabledSqlLog = value; }
        }
        public static bool EnabledLog { get; set; }
        public static bool EnabledSqlLog { get; set; }

        public static void Log(LogLevel level, string message)
        {
            if (EnabledLog)
            {

            }
        }
        public static void LogSql(string sql, DBParameterCollection dbParameters)
        {
            if (EnabledSqlLog)
            {

            }
        }
    }
    public enum LogLevel
    {
        Debug,
        Info,
        Warning,
        Error,
    }

    public delegate void LogEventHandler(object sender, LogEventArgs e);
    public class LogEventArgs : EventArgs
    {

    }
}

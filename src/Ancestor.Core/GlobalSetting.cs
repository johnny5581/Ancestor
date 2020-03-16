using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ancestor.Core
{
    /// <summary>
    /// Log manager
    /// </summary>
    public static class GlobalSetting
    {
        private static bool _raiseExp = false;
        public static event LoggingEventHandler Logging;
        /// <summary>
        /// Raise exception when happen, global setting (will be override if dao set RaiseException flag)
        /// </summary>
        public static bool RaiseException
        {
            get { return _raiseExp; }
            set { _raiseExp = value; }
        }
        internal static void Log(IIdentifiable sender, string tag, string name, string message)
        {
            var handler = Logging;
            if (handler != null)
                handler(sender, new LoggingEventArgs(sender.Guid, tag, name, message));
        }
    }
    public delegate void LoggingEventHandler(object sender, LoggingEventArgs e);
    public class LoggingEventArgs : EventArgs
    {
        public LoggingEventArgs(Guid id, string tag, string name, string message)
        {
            Id = id;
            Tag = tag;
            Name = name;
            Message = message;
        }
        public Guid Id { get; }
        public string Tag { get; }
        public string Name { get; }
        public string Message { get; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ancestor.Core
{
    /// <summary>
    /// Log manager
    /// </summary>
    [Obsolete("use AncestorGlobalOptions instead.", true)]
    public static class GlobalSettings
    {
        private static bool _raiseExp = false;
        internal static string TnsnamesPath;
        internal static string SystemTnsnamesPath;
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
        public static bool UseOracleStringParameter { get; set; }

        /// <summary>
        /// Managed Oracle Data Access's TNSNAMES.ora location
        /// </summary>
        public static string ManagedOracleTnsNamesLocation
        {
            get { return TnsnamesPath ?? SystemTnsnamesPath; }
            set { TnsnamesPath = value; }
        }
    }
    
}

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;

namespace Ancestor.Core
{
    public static class AncestorGlobalOptions
    {
        private static bool _enabledTimeout = false;
        private static int _timeoutInterval = 10 * 1000; // 10sec
        private static string _lzPwSecretPref = "LZPW_";
        private static string _lzPwSecretNode = "";
        private static string _lzPwSecretNodePref = "LZPWN_";
        private static bool _lzPw = false;
        private static bool _enabledDebug = false;
        private static bool _raiseExp = false;
        internal static string TnsnamesPath;
        internal static string SystemTnsnamesPath;
        private static Encoding _gHwEnc;
        
        public static event LoggingEventHandler Logging;
        static AncestorGlobalOptions()
        {
            if (bool.TryParse(ConfigurationManager.AppSettings["ancestor.option.timeout.enable"], out bool enabled))
                _enabledTimeout = enabled;
            if (int.TryParse(ConfigurationManager.AppSettings["ancestor.option.timeout.interval"], out int interval))
                _timeoutInterval = interval;
            var lazyPrefix = ConfigurationManager.AppSettings["ancestor.option.lzpw.prefix"];
            if (lazyPrefix != null)
                _lzPwSecretPref = lazyPrefix;
            var lazyNodePrefix = ConfigurationManager.AppSettings["ancestor.option.lzpw.node.prefix"];
            if (lazyNodePrefix != null)
                _lzPwSecretNodePref = lazyNodePrefix;
            var lazyNode = ConfigurationManager.AppSettings["ancestor.option.lzpw.node"];
            if (lazyNode != null)
                _lzPwSecretNode = lazyNode;
            if (bool.TryParse(ConfigurationManager.AppSettings["ancestor.option.debug"], out bool debug))
                _enabledDebug = debug;
            if (bool.TryParse(ConfigurationManager.AppSettings["ancestor.option.lzpw.enable"], out bool lzpw))
                _lzPw = lzpw;
            var hwEncNm = ConfigurationManager.AppSettings["ancestor.option.hardword.encoding"];
            if(hwEncNm != null)            
                _gHwEnc = Encoding.GetEncoding(hwEncNm);            
        }
        public static bool Debug
        {
            get { return _enabledDebug; }
            set { _enabledDebug = value; }
        }
        public static bool EnableTimeout
        {
            get { return _enabledTimeout; }
            set { _enabledTimeout = value; }
        }
        public static int TimeoutInterval
        {
            get { return _timeoutInterval; }
            set { _timeoutInterval = value; }
        }
        public static string LazyPasswordSecretKeyPrefix
        {
            get { return _lzPwSecretPref; }
            set { _lzPwSecretPref = value; }
        }
        public static string LazyPasswordSecretKeyNode
        {
            get { return _lzPwSecretNode; }
            set { _lzPwSecretNode = value; }
        }
        public static string LazyPasswordSecretKeyNodePrefix
        {
            get { return _lzPwSecretNodePref; }
            set { _lzPwSecretNodePref = value; }
        }
        public static bool GlobalLazyPassword
        {
            get { return _lzPw; }
            set { _lzPw = value; }
        }
        public static bool RaiseException
        {
            get { return _raiseExp; }
            set { _raiseExp = value; }
        }
        /// <summary>
        /// (Oracle only) use OracleString instead String
        /// </summary>
        public static bool UseOracleStringParameter { get; set; }
        /// <summary>
        /// Managed Oracle Data Access's TNSNAMES.ora location
        /// </summary>
        public static string ManagedOracleTnsNamesLocation
        {
            get { return TnsnamesPath ?? SystemTnsnamesPath; }
            set { TnsnamesPath = value; }
        }
        public static Encoding GlobalHardwordEncoding
        {
            get { return _gHwEnc; }
            set { _gHwEnc = value; }
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

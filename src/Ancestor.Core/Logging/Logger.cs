/**
 * History:
 *   [12] 20240702
 *     調整appconf存取方式
 *     
 **/


using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Ancestor.Core.Logging
{
    /// #src#
    internal interface ILogger
    {
        void WriteLog(TraceEventType level, string message);
    }

    /// #src#
    internal static class Logger
    {
        private static ILogFactory _instance;
        public const int Version = 12;
        private static string _logName;
        private static string _logType;
        public static string LogType
        {
            get { return _logType; }
            set
            {
                if (_logType != null)
                    throw new InvalidOperationException("log type can't be setted twice");
                _logType = value;
            }
        }
        public static string LogName
        {
            get { return _logName ?? AppDomain.CurrentDomain.FriendlyName; }
            set { _logName = value; }
        }

        private static ILogFactory GetFactory()
        {
            if (_instance == null)
            {
                var instance = AppDomain.CurrentDomain.GetData("Logger");
                if (instance == null)
                {
                    // 沒有工廠
                    _instance = new LogFactory();
                    AppDomain.CurrentDomain.SetData("Logger", _instance);
                }
                else
                {
                    // 版本檢查
                    var versionMember = instance.GetType().GetProperty("Version", BindingFlags.Public | BindingFlags.Instance);
                    var obsoleted = false;
                    if (versionMember == null)
                        obsoleted = true;
                    else if ((int)versionMember.GetValue(instance, null) < Version)
                        obsoleted = true;

                    if (obsoleted)
                    {
                        // 替換舊的工廠，呼叫Obsolete
                        var newInstance = new LogFactory();
                        var obsoleteMember = instance.GetType().GetMethod("Obsolete");
                        if (obsoleteMember != null)
                            obsoleteMember.Invoke(instance, new object[] { newInstance });
                        AppDomain.CurrentDomain.SetData("Logger", newInstance);
                        _instance = newInstance;
                    }
                    else
                        _instance = new ProxyLogFactory();
                }
            }
            return _instance;
        }

        public static void WriteLog(string name, TraceEventType level, string message)
        {
            GetFactory().WriteLog(name, (int)level, message);
        }

        public static void WriteLog(TraceEventType level, string message)
        {
            WriteLog(LogName, level, message);
        }


        private static string GetMessage(string message, object[] args)
        {
            if (args == null || args.Length == 0)
                return message;
            else if (message.Contains("{0}"))
                return string.Format(message, args);
            else
            {
                if (message.Trim().LastOrDefault() != ':')
                    message += ": ";
                message += string.Join(", ", Enumerable.Range(0, args.Length).Select(i => "{" + i + "}"));
                return string.Format(message, args);
            }
        }
        public static ILogger CreateInstance(string name)
        {
            return new Instance(name);
        }
        #region Basic 
        public static void Critical(this ILogger logger, string message, params object[] args)
        {
            logger.WriteLog(TraceEventType.Critical, GetMessage(message, args));
        }
        public static void Error(this ILogger logger, string message, params object[] args)
        {
            logger.WriteLog(TraceEventType.Error, GetMessage(message, args));
        }
        public static void Warning(this ILogger logger, string message, params object[] args)
        {
            logger.WriteLog(TraceEventType.Warning, GetMessage(message, args));
        }
        public static void Information(this ILogger logger, string message, params object[] args)
        {
            logger.WriteLog(TraceEventType.Information, GetMessage(message, args));
        }
        public static void Debug(this ILogger logger, string message, params object[] args)
        {
            logger.WriteLog(TraceEventType.Verbose, GetMessage(message, args));
        }
        #endregion Basic

        #region Condition 
        public static void WriteLogIf(this ILogger logger, TraceEventType eventType, bool condition, string message)
        {
            if (condition)
                logger.WriteLog(eventType, message);
        }

        public static void DebugIf(this ILogger logger, bool condition, string message, params object[] args)
        {
            if (condition)
                logger.Debug(message, args);
        }
        public static void InformationIf(this ILogger logger, bool condition, string message, params object[] args)
        {
            if (condition)
                logger.Information(message, args);
        }
        public static void WarningIf(this ILogger logger, bool condition, string message, params object[] args)
        {
            if (condition)
                logger.Warning(message, args);
        }
        public static void ErrorIf(this ILogger logger, bool condition, string message, params object[] args)
        {
            if (condition)
                logger.Error(message, args);
        }
        #endregion Condition
        private class Instance : ILogger
        {
            private readonly string _name;

            public Instance(string name)
            {
                _name = name;
            }

            public void WriteLog(TraceEventType level, string message)
            {
                Logger.WriteLog(_name, level, message);
            }
        }

        private interface ILogFactory
        {
            void Obsolete(object newInstance);
            void WriteLog(string name, int level, string message);
        }
        abstract class BasicLogFactory : ILogFactory, IDisposable
        {
            public BasicLogFactory()
            {
                Version = Logger.Version;
            }
            protected bool _disposed;
            public virtual void Obsolete(object newInstance)
            {
                // 將這個實例的物件取代為 Proxy
                _instance = new ProxyLogFactory();

                // 釋放資源
                Dispose();
            }

            public abstract void WriteLog(string name, int level, string message);

            public int Version { get; }
            #region Dispose
            public void Dispose()
            {
                OnDisposing(true);
                GC.SuppressFinalize(this);
            }
            ~BasicLogFactory()
            {
                OnDisposing(false);
            }
            protected void OnDisposing(bool disposing)
            {
                if (!_disposed)
                {
                    Dispose(disposing);
                    _disposed = true;
                }
            }
            protected virtual void Dispose(bool disposing)
            {

            }
            #endregion
        }

        public abstract class LogHandler
        {
            public LogHandler()
            {
                var level = System.Configuration.ConfigurationManager.AppSettings["logger.level"];
                TraceEventType filterLevel;
                if (!Enum.TryParse(level, true, out filterLevel))
                {
                    if (string.IsNullOrEmpty(level))
                        filterLevel = TraceEventType.Information; // 預設使用Info
                    else
                    {
                        switch (level.ToLower())
                        {
                            case "debug":
                                filterLevel = TraceEventType.Verbose;
                                break;
                            case "info":
                                filterLevel = TraceEventType.Information;
                                break;
                            case "warn":
                                filterLevel = TraceEventType.Warning;
                                break;
                            default:
                                filterLevel = TraceEventType.Transfer;
                                break;
                        }
                    }
                }
                FilterLevel = filterLevel;
            }
            public TraceEventType FilterLevel { get; set; }
            public abstract void Write(string name, TraceEventType level, string message);
            protected string GetFormattedString(string name, TraceEventType level, string message)
            {
                return string.Format("[{0:yyyy-MM-dd HH:mm:ss}][{1}][{2, 5}] {3}", DateTime.Now, name, GetLevelString(level), message);
            }
            protected string GetLevelString(TraceEventType level)
            {
                switch (level)
                {
                    case TraceEventType.Critical:
                    case TraceEventType.Error:
                        return "ERROR";
                    case TraceEventType.Warning:
                        return "WARN";
                    case TraceEventType.Information:
                        return "INFO";
                    case TraceEventType.Verbose:
                        return "DEBUG";
                    default:
                        return "";
                }
            }
        }

        public class ConsoleLogHandler : LogHandler
        {
            public override void Write(string name, TraceEventType level, string message)
            {
                if (level <= FilterLevel)
                    Console.WriteLine(GetFormattedString(name, level, message));
            }
        }
        public class TraceSourceLogHandler : LogHandler
        {
            private readonly TraceSource _source;
            public TraceSourceLogHandler(string name)
            {
                _source = new TraceSource(name);
            }
            public override void Write(string name, TraceEventType level, string message)
            {
                if (level <= FilterLevel)
                    _source.TraceEvent(level, 1, GetFormattedString(name, level, message));
            }
        }
        public class Log4NetHandler : LogHandler
        {
            private readonly Dictionary<string, object> _instances
                = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
            private Assembly _assembly;
            private MethodInfo _mGetLogger;
            private MethodInfo[] _mDebug = new MethodInfo[3];
            private MethodInfo[] _mInfo = new MethodInfo[3];
            private MethodInfo[] _mWarn = new MethodInfo[3];
            private MethodInfo[] _mError = new MethodInfo[3];
            private MethodInfo[] _mFatal = new MethodInfo[3];
            public Log4NetHandler()
            {
                InitializeAssembly();
                InitializeConfiguration();
                InitializeWrapper();
            }



            public override void Write(string name, TraceEventType level, string message)
            {
                object logger;
                if (!_instances.TryGetValue(name, out logger))
                {
                    logger = _mGetLogger.Invoke(null, new object[] { name });
                    _instances.Add(name, logger);
                }

                MethodInfo[] m;
                switch (level)
                {
                    case TraceEventType.Error:
                        m = _mError;
                        break;
                    case TraceEventType.Warning:
                        m = _mWarn;
                        break;
                    case TraceEventType.Verbose:
                        m = _mDebug;
                        break;
                    case TraceEventType.Critical:
                        m = _mFatal;
                        break;
                    default:
                    case TraceEventType.Information:
                        m = _mInfo;
                        break;
                }
                // 目前只有紀錄純文字
                m[0].Invoke(logger, new object[] { message });
            }

            private void InitializeAssembly()
            {
                _assembly = AppConfigHelper.Get("logger.log4net.dll", "logger.log4net.dll.resolve",
                    (asm, res, type) =>
                    {
                        var lib = LogResManager.GetResourceBytes(asm, res, throwOnError: false, nameResolveType: type);
                        if (lib != null)
                            return Assembly.Load(lib);
                        return null;
                    },
                    (name) =>
                    {
                        if (name.EndsWith(".dll", StringComparison.OrdinalIgnoreCase)) // 檔案
                        {
                            var libPath = System.IO.Path.GetFullPath(name);
                            if (System.IO.File.Exists(libPath))
                                return Assembly.LoadFile(libPath);
                            var libFolder = System.IO.Path.GetDirectoryName(typeof(LogResManager).Assembly.Location);
                            libPath = System.IO.Path.Combine(libFolder, name);
                            if (System.IO.File.Exists(libPath))
                                return Assembly.LoadFile(libPath);
                        }
                        // 非檔案類，嘗試直接載入
                        return AppDomain.CurrentDomain.Load(name);
                    });

                if (_assembly == null)
                    throw new NullReferenceException("no log4net dll found");
            }
            private void InitializeConfiguration()
            {
                var configuratorType = _assembly.GetType("log4net.Config.XmlConfigurator");
                var flgConf = AppConfigHelper.Get("logger.log4net.conf", "logger.log4net.conf.resolve",
                    (asm, res, type) =>
                    {
                        var stream = LogResManager.GetResourceStream(asm, res, throwOnError: false, nameResolveType: type);
                        if (stream != null)
                        {
                            var mConfStream = configuratorType.GetMethod("Configure", BindingFlags.Public | BindingFlags.Static, null, new Type[] { typeof(Stream) }, null);
                            if (mConfStream != null)
                            {
                                mConfStream.Invoke(null, new object[] { stream });
                                return true;
                            }
                        }
                        return false;
                    },
                    (name) =>
                    {
                        var paths = new List<string>();
                        paths.Add(Path.GetFullPath(name));
                        paths.Add(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, name));
                        paths.Add(Path.GetFullPath("log4net.config"));
                        paths.Add(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "log4net.config"));

                        foreach (var confPath in paths)
                        {
                            if (File.Exists(confPath))
                            {
                                var fileInfo = new FileInfo(confPath);
                                var mConfFile = configuratorType.GetMethod("Configure", BindingFlags.Public | BindingFlags.Static, null, new Type[] { typeof(FileInfo) }, null);
                                if (mConfFile != null)
                                {
                                    mConfFile.Invoke(null, new object[] { fileInfo });
                                    return true;
                                }
                            }
                        }
                        return false;
                    });

                if (!flgConf) // 無法設定
                {
                    try
                    {
                        var mConf = _assembly.GetType("log4net.Config.BasicConfigurator").GetMethod("Configure", BindingFlags.Public | BindingFlags.Static, null, Type.EmptyTypes, null);
                        if (mConf != null)
                        {
                            mConf.Invoke(null, new object[0]);
                            flgConf = true;
                        }
                    }
                    catch { }
                }

                if (!flgConf)
                    throw new InvalidOperationException("config log4net fail");
            }
            private void InitializeWrapper()
            {

                var managerType = _assembly.GetType("log4net.LogManager");
                _mGetLogger = managerType.GetMethod("GetLogger", BindingFlags.Public | BindingFlags.Static, null, new Type[] { typeof(string) }, null);

                var logType = _assembly.GetType("log4net.ILog");

                Binding(logType, "Debug", ref _mDebug);
                Binding(logType, "Info", ref _mInfo);
                Binding(logType, "Warn", ref _mWarn);
                Binding(logType, "Error", ref _mError);
                Binding(logType, "Fatal", ref _mFatal);

            }
            private void Binding(Type logType, string name, ref MethodInfo[] methods)
            {
                methods[0] = logType.GetMethod(name, BindingFlags.Public | BindingFlags.Instance, null, new Type[] { typeof(string) }, null);
                methods[1] = logType.GetMethod(name, BindingFlags.Public | BindingFlags.Instance, null, new Type[] { typeof(string), typeof(Exception) }, null);
                methods[2] = logType.GetMethod(name, BindingFlags.Public | BindingFlags.Instance, null, new Type[] { typeof(string), typeof(object[]) }, null);
            }


        }
        private class LogFactory : BasicLogFactory
        {
            private List<LogHandler> _handlers = new List<LogHandler>();

            public LogFactory()
            {
                string[] configLoggerTypes = null;
                var configLoggerType = LogType ?? System.Configuration.ConfigurationManager.AppSettings["logger.type"];
                if (configLoggerType != null)
                {
                    configLoggerTypes = configLoggerType.Split(';');
                }
                else
                {
                    configLoggerTypes = new string[0];
                }


                if (configLoggerTypes.Length == 0 || configLoggerTypes.Contains("Console", StringComparer.OrdinalIgnoreCase))
                {
                    _handlers.Add(new ConsoleLogHandler());
                }
                if (configLoggerTypes.Contains("TraceSource", StringComparer.OrdinalIgnoreCase))
                {
                    var name = _logName ?? System.Configuration.ConfigurationManager.AppSettings["logger.name"];
                    if (string.IsNullOrEmpty(name))
                        name = AppDomain.CurrentDomain.FriendlyName;
                    _handlers.Add(new TraceSourceLogHandler(name));
                }
                if (configLoggerTypes.Contains("log4net", StringComparer.OrdinalIgnoreCase))
                {
                    _handlers.Add(new Log4NetHandler());
                }
            }
            public override void WriteLog(string name, int level, string message)
            {
                foreach (var handler in _handlers)
                    handler.Write(name, (TraceEventType)level, message);
            }

        }
        private class ProxyLogFactory : ILogFactory
        {
            private readonly AppDomain _domain;
            public ProxyLogFactory() : this(AppDomain.CurrentDomain)
            {
            }
            public ProxyLogFactory(AppDomain domain)
            {
                _domain = domain;
            }
            public void Obsolete(object newInstance)
            {
                // Do nothing
            }
            private object GetInstance()
            {
                return _domain.GetData("Logger");
            }
            public void WriteLog(string name, int level, string message)
            {
                DynamicInvoke(t => t.GetMethod("WriteLog"), new object[] { name, level, message });
            }



            #region Tools
            private object DynamicInvoke(Func<Type, MethodInfo> memberResolver, object[] args)
            {
                object result = null;
                var instance = GetInstance();
                if (instance != null)
                {
                    var instanceType = instance.GetType();
                    var member = memberResolver(instanceType);
                    if (member != null)
                        result = member.Invoke(instance, args);
                }
                return result;
            }

            private object DynamicInvoke(Func<Type, FieldInfo> memberResolver)
            {
                object result = null;
                var instance = GetInstance();
                if (instance != null)
                {
                    var instanceType = instance.GetType();
                    var member = memberResolver(instanceType);
                    if (member != null)
                        result = member.GetValue(instance);
                }
                return result;
            }
            private void DynamicInvoke(Func<Type, FieldInfo> memberResolver, object value)
            {
                var instance = GetInstance();
                if (instance != null)
                {
                    var instanceType = instance.GetType();
                    var member = memberResolver(instanceType);
                    if (member != null)
                        member.SetValue(instance, value);
                }
            }

            private object DynamicInvoke(Func<Type, PropertyInfo> memberResolver)
            {
                object result = null;
                var instance = GetInstance();
                if (instance != null)
                {
                    var instanceType = instance.GetType();
                    var member = memberResolver(instanceType);
                    if (member != null)
                        result = member.GetValue(instance, null);
                }
                return result;
            }
            private void DynamicInvoke(Func<Type, PropertyInfo> memberResolver, object value)
            {
                var instance = GetInstance();
                if (instance != null)
                {
                    var instanceType = instance.GetType();
                    var member = memberResolver(instanceType);
                    if (member != null)
                        member.SetValue(instance, value, null);
                }
            }
            #endregion
        }
        internal static class AppConfigHelper
        {
            private const string PREF_RES = "res://";

            public static T Resolve<T>(string value, Func<Assembly, string, int, T> resResolver, Func<string, T> fileResolver)
            {
                if (string.IsNullOrEmpty(value))
                    return default(T);
                else if (value.StartsWith(PREF_RES, StringComparison.OrdinalIgnoreCase))
                {
                    var resName = value.Substring(PREF_RES.Length);
                    Assembly asm = null;
                    if (resName.Contains(";"))
                    {
                        var tmp = resName.Split(';');
                        resName = tmp[0];
                        asm = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(r => string.Equals(r.GetName().Name, tmp[1], StringComparison.OrdinalIgnoreCase));
                    }
                    int resType = 0;
                    if (resName.Contains(','))
                    {
                        var tmp = resName.Split(',');
                        resName = tmp[0];
                        int.TryParse(tmp[1], out resType);
                    }
                    return resResolver(asm, resName, resType);
                }
                else
                {
                    return fileResolver(value);
                }
            }
            public static T Resolve<T>(string value, int resType, Func<Assembly, string, int, T> resResolver, Func<string, T> fileResolver)
            {
                if (string.IsNullOrEmpty(value))
                    return default(T);
                else if (value.StartsWith(PREF_RES, StringComparison.OrdinalIgnoreCase))
                {
                    var resName = value.Substring(PREF_RES.Length);
                    Assembly asm = null;
                    if (resName.Contains(";"))
                    {
                        var tmp = resName.Split(';');
                        resName = tmp[0];
                        asm = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(r => string.Equals(r.GetName().Name, tmp[1], StringComparison.OrdinalIgnoreCase));
                    }
                    return resResolver(asm, resName, resType);
                }
                else
                {
                    return fileResolver(value);
                }
            }
            public static T Get<T>(string key, Func<Assembly, string, int, T> resResolver, Func<string, T> fileResolver)
            {
                var config = System.Configuration.ConfigurationManager.AppSettings[key];
                return Resolve(key, resResolver, fileResolver);
            }

            public static T Get<T>(string key, string resTypeKey, Func<Assembly, string, int, T> resResolver, Func<string, T> fileResolver)
            {
                var config = System.Configuration.ConfigurationManager.AppSettings[key];
                int resType;
                var resTypeConf = System.Configuration.ConfigurationManager.AppSettings[resTypeKey];
                int.TryParse(resTypeConf, out resType);
                return Resolve(config, resType, resResolver, fileResolver);
            }
        }
        internal static class LogResManager
        {
            private static readonly string _baseNs;

            static LogResManager()
            {
                _baseNs = typeof(LogResManager).Namespace;
            }

            public static string GetResourceName(string name)
            {
                return _baseNs + "." + name;
            }
            private static string GetFuzzyResourceName(Assembly assembly, string name)
            {
                var names = assembly.GetManifestResourceNames();
                return names.FirstOrDefault(r => r.EndsWith(name, StringComparison.OrdinalIgnoreCase));
            }

            public static Stream GetResourceStream(string name, bool throwOnError = true, int nameResolveType = 0)
            {
                return GetResourceStream(null, name, throwOnError, nameResolveType);
            }
            public static string GetResourceString(string name, bool throwOnError = true, int nameResolveType = 0)
            {
                return GetResourceString(null, name, throwOnError, nameResolveType);
            }
            public static byte[] GetResourceBytes(string name, bool throwOnError = true, int nameResolveType = 0)
            {
                return GetResourceBytes(null, name, throwOnError, nameResolveType);
            }

            public static T GetResource<T>(string name, Func<Stream, T> streamResolver, bool throwOnError = true, int nameResolveType = 0)
            {
                return GetResource(null, name, streamResolver, throwOnError, nameResolveType);
            }
            public static Stream GetResourceStream(Assembly assembly, string name, bool throwOnError = true, int nameResolveType = 0)
            {
                var asm = assembly ?? typeof(LogResManager).Assembly;
                var n = GetResolveName(name, nameResolveType, throwOnError, asm);
                return ResManager.GetResourceStream(asm, n, throwOnError);
            }
            public static string GetResourceString(Assembly assembly, string name, bool throwOnError = true, int nameResolveType = 0)
            {
                var asm = assembly ?? typeof(LogResManager).Assembly;
                var n = GetResolveName(name, nameResolveType, throwOnError, asm);
                return ResManager.GetResourceString(asm, n, throwOnError);
            }
            public static byte[] GetResourceBytes(Assembly assembly, string name, bool throwOnError = true, int nameResolveType = 0)
            {
                var asm = assembly ?? typeof(LogResManager).Assembly;
                var n = GetResolveName(name, nameResolveType, throwOnError, asm);
                return ResManager.GetResourceBytes(asm, n, throwOnError);
            }

            public static T GetResource<T>(Assembly assembly, string name, Func<Stream, T> streamResolver, bool throwOnError = true, int nameResolveType = 1)
            {
                var asm = assembly ?? typeof(LogResManager).Assembly;
                var n = GetResolveName(name, nameResolveType, throwOnError, asm);
                return ResManager.GetResource(asm, n, streamResolver, throwOnError);
            }

            private static string GetResolveName(string name, int resolveType, bool throwOnError = true, Assembly assembly = null)
            {
                string n = null;
                switch (resolveType)
                {
                    case 0: // self
                        n = GetResourceName(name);
                        break;
                    case 1: // absolute
                        n = name;
                        break;
                    case 2: // fuzzy
                        n = GetFuzzyResourceName(assembly, name);
                        break;
                    default:
                        throw new NotSupportedException("unsupported resolve type: " + resolveType);
                }
                if (n == null && throwOnError)
                    throw new InvalidOperationException("no suitable name found: " + name + ", with resolve type: " + resolveType);
                return n;
            }

            internal class ResManager
            {
                private readonly Assembly _assembly;
                private readonly Dictionary<string, object> _resCache;
                public ResManager(Assembly assembly)
                {
                    _assembly = assembly;
                    _resCache = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
                }


                public Stream GetStream(string name, bool throwOnError = true)
                {
                    if (name == null)
                        throw new ArgumentNullException("name");
                    var stream = _assembly.GetManifestResourceStream(name);
                    if (stream == null && throwOnError)
                        throw new InvalidOperationException("resource '" + name + "' is not exists");
                    return stream;
                }

                public string GetString(string name, bool throwOnError = true)
                {
                    return Get(name, stream =>
                    {
                        if (stream == null) return null;
                        using (var sr = new StreamReader(stream))
                            return sr.ReadToEnd();
                    }, throwOnError);
                }

                public byte[] GetBytes(string name, bool throwOnError = true)
                {
                    return Get(name, (stream) =>
                    {
                        if (stream == null) return null;
                        var buffer = new byte[4096];
                        var readBytes = 0;
                        using (var ms = new MemoryStream())
                        {
                            while ((readBytes = stream.Read(buffer, 0, buffer.Length)) != 0)
                            {
                                ms.Write(buffer, 0, readBytes);
                                ms.Flush();
                            }
                            return ms.ToArray();
                        }
                    }, throwOnError);
                }
                public T Get<T>(string name, Func<Stream, T> factory, bool throwOnError = true)
                {
                    object resource;
                    if (!_resCache.TryGetValue(name, out resource))
                    {
                        using (var stream = GetStream(name, throwOnError))
                        {
                            resource = factory(stream);
                            _resCache.Add(name, resource);
                        }
                    }
                    return (T)resource;
                }
                private static readonly Dictionary<Assembly, ResManager> _cache
                    = new Dictionary<Assembly, ResManager>();
                private static ResManager GetResManager(Assembly assembly)
                {
                    ResManager manager;
                    if (!_cache.TryGetValue(assembly, out manager))
                    {
                        manager = new ResManager(assembly);
                        _cache.Add(assembly, manager);
                    }
                    return manager;
                }
                public static Stream GetResourceStream(Assembly assembly, string name, bool throwOnError = true)
                {
                    return GetResManager(assembly).GetStream(name, throwOnError);
                }
                public static string GetResourceString(Assembly assembly, string name, bool throwOnError = true)
                {
                    return GetResManager(assembly).GetString(name, throwOnError);
                }
                public static byte[] GetResourceBytes(Assembly assembly, string name, bool throwOnError = true)
                {
                    return GetResManager(assembly).GetBytes(name, throwOnError);
                }

                public static T GetResource<T>(Assembly assembly, string name, Func<Stream, T> streamResolver, bool throwOnError = true)
                {
                    return GetResManager(assembly).Get(name, streamResolver, throwOnError);
                }


                public static string GetResourceString(Type referenceType, string name, bool throwOnError = true)
                {
                    var tuple = Resolve(referenceType, name);
                    return GetResourceString(tuple.Item1, tuple.Item2, throwOnError);
                }
                public static byte[] GetResourceBytes(Type referenceType, string name, bool throwOnError = true)
                {
                    var tuple = Resolve(referenceType, name);
                    return GetResourceBytes(tuple.Item1, tuple.Item2, throwOnError);
                }

                public static T GetResource<T>(Type referenceType, string name, Func<Stream, T> streamResolver, bool throwOnError = true)
                {
                    var tuple = Resolve(referenceType, name);
                    return GetResource(tuple.Item1, tuple.Item2, streamResolver, throwOnError);
                }

                private static Tuple<Assembly, string> Resolve(Type referenceType, string name)
                {
                    var assembly = referenceType.Assembly;
                    var n = referenceType.Namespace + "." + name;
                    return Tuple.Create(assembly, n);
                }
            }
        }
    }
}

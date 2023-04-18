using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ancestor.Core
{
    public static class AncestorGlobalOptions
    {
        private static readonly Dictionary<string, string> _settings
            = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        private const string SettingPrefix = "ancestor.";
        static AncestorGlobalOptions()
        {
            // default values
            SetOption("option.lzpw.prefix", "LZPW_");
            SetOption("option.lzpw.node.prefix", "LZPWN_");
            SetOption("option.lzpw.clearpool", "Y");
            SetOption("option.close", "auto");
            SetOption("option.exec_conf", "N");
            SetOption("option.exec_ignore", "N");
            SetOption("option.lzpw.prov", "oracle");

            var settings = System.Configuration.ConfigurationManager.AppSettings;
            foreach (string key in settings.Keys)
            {
                if (key.StartsWith(SettingPrefix))
                {
                    var name = key.Substring(SettingPrefix.Length);
                    SetOption(name, settings[key]);
                }
            }
        }
        public static void SetOption(string name, string value)
        {
            if (_settings.ContainsKey(name))
                _settings[name] = value;
            else
                _settings.Add(name, value);
        }

        public static T GetOption<T>(string name, T defaultValue, Func<string, T> converter)
        {
            try
            {
                if (_settings.TryGetValue(name, out string value))
                    return converter(value);
            }
            catch { }
            return defaultValue;
        }

        public static string GetString(string name, string defaultValue = null)
        {
            return GetOption(name, defaultValue, r => r);
        }
        public static bool GetBoolean(string name, bool defaultValue = false)
        {
            return GetOption(name, defaultValue, ConvertBoolean);
        }
        public static int GetInteger(string name, int defaultValue = 0)
        {
            return GetOption(name, defaultValue, r => int.Parse(r));
        }
        private static bool ConvertBoolean(string value)
        {
            if (string.IsNullOrEmpty(value)) return false;
            switch (value.ToLower())
            {
                case "y":
                case "t":
                case "true":
                case "1":
                    return true;
                case "n":
                case "f":
                case "false":
                case "0":
                    return false;
                default:
                    throw new InvalidCastException("cannot cast to boolean:" + value);
            }
        }
    }
}

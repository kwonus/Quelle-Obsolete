using System;
using System.Collections.Generic;
using System.Text;

namespace ClarityHMI
{
    public static class HMISession
    {
        public static Dictionary<string, string> SessionConfig = new Dictionary<string, string>();
        public static Dictionary<string, string> StatementConfig = new Dictionary<string, string>();

        public static string Show(string setting, HMIScope scope, bool silent = false)
        {
            if (setting == null)
            {
                Console.WriteLine("Program design error");
                return "";
            }
            Dictionary<string, string> config = null;

            switch (scope)
            {
                case HMIScope.Session:      config = StatementConfig; break;
                case HMIScope.Statement:    config = StatementConfig; break;
                case HMIScope.Global:       Console.WriteLine("Global scope is not implemeneted yet."); return "";
                default:                    Console.WriteLine("Program design error"); return "";
            }
            if (config != null)
            {
                var normalized = setting.Trim().ToLower();

                if (normalized == "*")
                {
                    foreach (string key in config.Keys)
                        Console.WriteLine(key + ":\t" + config[key]);
                    return "*";
                }
                else if (config.ContainsKey(normalized))
                {
                    if (!silent)
                        Console.WriteLine(config[normalized]);
                    return config[normalized];
                }
            }
            return "";
        }
        public static bool Cancel(string setting, HMIScope scope, bool silent = false)
        {
            if (setting == null)
            {
                Console.WriteLine("Program design error");
                return false;
            }
            Dictionary<string, string> config = null;

            switch (scope)
            {
                case HMIScope.Session: config = StatementConfig; break;
                case HMIScope.Statement: config = StatementConfig; break;
                case HMIScope.Global: Console.WriteLine("Global scope is not implemeneted yet."); return false;
                default: Console.WriteLine("Program design error"); return false;
            }
            if (config != null)
            {
                var normalized = setting.Trim().ToLower();
                if (config.ContainsKey(normalized))
                {
                    config.Remove(normalized);
                    if (!silent)
                        Console.WriteLine("ok");
                }
                else
                {
                    if (!silent)
                        Console.WriteLine("The configuration settings was not not found; nothing to cancel.");
                }
                return true;
            }
            Console.WriteLine("Program design error");
            return false;
        }
        public static bool Config(string setting, HMIScope scope, string value, bool silent = false)
        {
            if (setting == null)
            {
                Console.WriteLine("Program design error");
                return false;
            }
            Dictionary<string, string> config = null;

            switch (scope)
            {
                case HMIScope.Session:      config = StatementConfig; break;
                case HMIScope.Statement:    config = StatementConfig; break;
                case HMIScope.Global:       Console.WriteLine("Global scope is not implemeneted yet."); return false;
                default:                    Console.WriteLine("Program design error"); return false;
            }
            if ((config != null) && Cancel(setting, scope, true))
            {
                var normalizedSetting = setting.Trim().ToLower();
                var normalizedValue = value.Trim();

                if (value == null || value.Length < 1)
                    return true;    // Cancelled

                config[normalizedSetting] = normalizedValue;
                Console.WriteLine("ok");

                return true;
            }
            Console.WriteLine("Program design error");
            return false;
        }
    }
}

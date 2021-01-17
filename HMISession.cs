using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ClarityHMI
{
    public static class HMISession
    {
        public static Dictionary<string, string> SessionConfig = new Dictionary<string, string>();
        public static Dictionary<string, string> StatementConfig = new Dictionary<string, string>();

        public class ClarityResultString: IClarityResultString
        {
            public string result { get; protected set; }
            public bool success { get; protected set; }
            public string[] errors { get; protected set; }
            public string[] warnings { get; protected set; }

            public ClarityResultString(string result = null, string error = null, string warning = null)
            {
                this.result = result;
                this.success = (result != null) && (error == null);
                this.errors = (error != null) ? new string[] { error.Trim() } : null;
                this.warnings = (warning != null) ? new string[] { warning.Trim() } : null;
            }
            public ClarityResultString()
            {
                this.result = null;
                this.success = false;
                this.errors = null;
                this.warnings = null;
            }
            public ClarityResultString AddWarning(string warning)
            {
                int resize = this.warnings != null ? this.warnings.Length : 1;
                string[] array = new string[resize];
                for (int i = 0; i < resize - 1; i++)
                    array[i] = this.warnings[i];
                array[resize - 1] = warning;
                this.warnings = array;

                return this;
            }
            private ClarityResultString(IClarityResultObject result)
            {
                this.result = (string) result.result;
                this.success = result.success;
                this.errors = result.errors;
                this.warnings = result.warnings;
            }
            public static ClarityResultString Create(IClarityResultObject result)
            {
                return (result != null) ? new ClarityResultString(result) : new ClarityResultString();
            }
        }
        public class ClarityResult : IClarityResult
        {
            public bool success { get; protected set; }
            public string[] errors { get; protected set; }
            public string[] warnings { get; protected set; }

            public ClarityResult(bool success)
            {
                this.success = success;
                this.errors = null;
                this.warnings = null;
            }
            public ClarityResult(string error)
            {
                this.success = false;
                this.errors = new string[] { error.Trim() };
                this.warnings = null;
            }
            public ClarityResult(string error, string warning)
            {
                this.success = false;
                this.errors = new string[] { error.Trim() };
                this.warnings = new string[] { warning.Trim() };
            }
            public ClarityResult(bool success, string warning)
            {
                this.success = success;
                this.errors = null;
                this.warnings = new string[] { warning.Trim() };
            }
            public ClarityResult()
            {
                this.success = false;
                this.errors = null;
                this.warnings = null;
            }
        }
        public class ClarityResultObject : ClarityResult, IClarityResultObject
        {
            public object result { get; protected set; }

            public ClarityResultObject(bool success, object obj) : base(success)
            {
                this.result = obj;
            }
            internal ClarityResultObject(string error, object obj) : base(error)
            {
                if (this.success)
                    this.success = (obj != null);
                this.result = obj;
            }
            public ClarityResultObject(string error, string warning, object obj) : base(error, warning)
            {
                if (this.success)
                    this.success = (obj != null);
                this.result = obj;
            }
            public ClarityResultObject(bool success, string warning, object obj) : base(success,warning)
            {
                if (this.success)
                    this.success = (obj != null);
                this.result = obj;
            }
            public ClarityResultObject(object obj) : base()
            {
                this.success = (obj != null);
                this.result = obj;
            }
            public ClarityResultObject(string error) : base(error)
            {
                this.result = null;
            }
        }


        public static IClarityResultString Show(string setting, HMIScope scope)
        {
             if (setting == null)
                return new ClarityResultString(error: "Driver design error");

            Dictionary<string, string> config = null;

            switch (scope)
            {
                case HMIScope.Session:      config = SessionConfig;   break;
                case HMIScope.Statement:    config = StatementConfig; break;
                case HMIScope.System:       config = null;            break;
                case HMIScope.Cloud:        return new ClarityResultString(error: "This driver does not support Cloud Drivers!");
                default:                    return new ClarityResultString(error: "Driver design error", warning: "Unknown setting scope provided by driver");
            }
            string result = null;
            string warning = null;
            var normalized = setting.Trim().ToLower();
            if (config != null)
            {
                if (normalized == "*")
                {
                    result = "*";

                    foreach (string key in config.Keys)
                        if (result == null)
                            result = key + ":\t" + config[key];
                        else
                            result += ("\n" + key + ":\t" + config[key]);

                    if (result == null)
                        warning = "No keys found";
                }
                else if (config.ContainsKey(normalized))
                {
                    result = config[normalized];
                }
            }
            else
            {
                result = ReadGlobalSetting(normalized);
            }
            if (result != null && result.Length < 1)
                result = null;

            return result != null
                ? new ClarityResultString(result: result, warning: warning)
                : new ClarityResultString(warning: warning).AddWarning("Unable to read setting (Maybe it has not been set");
        }
        public static IClarityResult Remove(string setting, HMIScope scope)
        { 
            if (setting == null)
                return new ClarityResult(error: "Driver design error");

            Dictionary<string, string> config = null;

            switch (scope)
            {
                case HMIScope.Session:      config = SessionConfig;   break;
                case HMIScope.Statement:    config = StatementConfig; break;
                case HMIScope.System:       config = null;            break;
                case HMIScope.Cloud:        return new ClarityResult(error: "This driver does not support Cloud Drivers!");
                default:                    return new ClarityResult(error: "Driver design error", warning: "Unknown scope provided by driver");
            }
            string result = null;
            string warning = null;
            var normalized = setting.Trim().ToLower();
            if (config != null)
            {
                if (config.ContainsKey(normalized))
                {
                    config.Remove(normalized);
                    return new ClarityResult(true);
                }
                else
                {
                    if (normalized.StartsWith("{") && normalized.EndsWith("}"))
                        return new ClarityResult(true, warning: "The statement label was not not found; nothing to remove.");
                    else
                        return new ClarityResult(true, warning: "The configuration setting was not not found; nothing to remove.");
                }
            }
            else if (RemovelobalSetting(normalized) != null)
                return new ClarityResult(true);

            else if (normalized.StartsWith("{") && normalized.EndsWith("}"))
                    return new ClarityResult(true, warning: "The statement label was not not found; nothing to remove.");

            else
                return new ClarityResult(true, warning: "The configuration setting was not not found; nothing to remove.");
        }
        public static IClarityResult Config(string setting, HMIScope scope, string value)
        {
            if (setting == null || value == null)
                return new ClarityResult(error: "Driver design error");

            Dictionary<string, string> config = null;

            switch (scope)
            {
                case HMIScope.Session:      config = SessionConfig;   break;
                case HMIScope.Statement:    config = StatementConfig; break;
                case HMIScope.System:       config = null;            break;
                case HMIScope.Cloud:        return new ClarityResult(error: "This driver does not support Cloud Drivers!");
                default:                    return new ClarityResult(error: "Driver design error", warning: "Unknown scope provided by driver");
            }
            string result = null;
            string warning = null;
            var normalizedSetting = setting.Trim().ToLower();
            var normalizedValue = setting.Trim().ToLower();
            if (config != null)
            {
                Remove(setting, scope);
                config[normalizedSetting] = normalizedValue;
                return new ClarityResult(success: true);
            }
            return WriteGlobalSetting(normalizedSetting, normalizedValue)
                ? new ClarityResult(success: true)
                : new ClarityResult(error: "Unable to save setting.");

        }
        private readonly static char[] dot = new char[] { '.' };

        private static string _root = null;
        private static string root
        {
            get
            {
                if (_root != null)
                    return _root;

                string dir = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                if (dir == null)
                    dir = "./";
                else
                    dir.Trim().Replace('\\', '/');
                if (dir.Length == 0)
                    dir = "./";
                if (!dir.EndsWith('/'))
                    dir += "/";
                dir += "Clarity-HMI/";

                _root = dir;
                return dir;
            }
        }
        private static string GetSectionSpec(string keyspec, string path = null)
        {
            if (path == null)
                return GetSectionSpec(keyspec, root);

            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            string[] parts = keyspec.Split(dot, StringSplitOptions.None);

            if (parts.Length == 1)
                return path + "/" + parts[0].Trim() + ".conf";

            string dir = path + "/" + parts[0].Trim();
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            string newspec = parts[1];
            for (int i = 2; i < parts.Length; i++)
            {
                newspec += dot[0];
                newspec += parts[i].Trim();
            }
            return GetSectionSpec(newspec, dir);
        }
        private static bool WriteGlobalSetting(string keypath, string value)
        {
            string path = RemovelobalSetting(keypath);

            if (File.Exists(path))
                return false;

            // Create a file to write to.
            using (StreamWriter sw = File.CreateText(path))
            {
                sw.Write(value);
            }
            return File.Exists(path);
        }
        private static bool WriteGlobalSetting(string keypath, Int64 value)
        {
            string path = GetSectionSpec(keypath);
            RemovelobalSetting(path);

            if (File.Exists(path))
                return false;

            // Create a file to write to.
            using (StreamWriter sw = File.CreateText(path))
            {
                sw.Write(value.ToString());
            }
            return File.Exists(path);
        }
        private static string ReadGlobalSetting(string keypath)
        {
            string path = GetSectionSpec(keypath);
            if (File.Exists(path))
                using (StreamReader sr = File.OpenText(path))
                {
                    return sr.ReadLine();
                }
            return null;
        }
        private static string RemovelobalSetting(string keypath)
        {
            string path = GetSectionSpec(keypath);
            if (File.Exists(path))
                File.Delete(path);

            return !File.Exists(path) ? path : null;    // null return is an eror; path is returned for chaining on success.
        }
    }
}

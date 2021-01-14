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
                case HMIScope.Session:      config = SessionConfig;   break;
                case HMIScope.Statement:    config = StatementConfig; break;
                case HMIScope.System:       config = null;            break;
                default:                    Console.WriteLine("Program design error"); return "";
            }
            var normalized = setting.Trim().ToLower();
            if (config != null)
            {
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
            else
            {
                string value = ReadGlobalSetting(normalized);
                if (value == null || value.Length < 1)
                {
                    Console.WriteLine("Error reading global setting.");
                }
            }
            return "";
        }
        public static bool Reomove(string setting, HMIScope scope, bool silent = false)
        {
            if (setting == null)
            {
                Console.WriteLine("Program design error");
                return false;
            }
            Dictionary<string, string> config = null;

            switch (scope)
            {
                case HMIScope.Session:   config = SessionConfig;   break;
                case HMIScope.Statement: config = StatementConfig; break;
                case HMIScope.System:    config = null;            break;
                default:                 Console.WriteLine("Program design error"); return false;
            }
            var normalized = setting.Trim().ToLower();
            if (config != null)
            {
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
            else if (RemovelobalSetting(normalized) == null)
            {
                Console.WriteLine("Error removing global setting.");
            }
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
                case HMIScope.Session:      config = SessionConfig;   break;
                case HMIScope.Statement:    config = StatementConfig; break;
                case HMIScope.System:       config = null;            break;
                default:                    Console.WriteLine("Program design error"); return false;
            }
            var normalizedSetting = setting.Trim().ToLower();
            var normalizedValue = value.Trim();

            if (config != null)
            {
                if (Reomove(setting, scope, true))
                {

                    if (value == null || value.Length < 1)
                        return true;    // Cancelled

                    config[normalizedSetting] = normalizedValue;
                    Console.WriteLine("ok");

                    return true;
                }
            }
            else if (!WriteGlobalSetting(normalizedSetting, normalizedValue))
            {
                Console.WriteLine("Error saving global setting.");
            }
            return false;
        }
        private readonly static char[] dot = new char[] { '.' };
        private readonly static string[] delimiter = new string[] { "<|||||||>" };
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
        private static bool WriteGlobalSetting(string keypath, string[] values)
        {
            string path = GetSectionSpec(keypath);
            RemovelobalSetting(path);

            if (File.Exists(path))
                return false;

            // Create a file to write to.
            using (StreamWriter sw = File.CreateText(path))
            {
                sw.Write(string.Join(delimiter[0], values));
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
        private static Int64 ReadGlobalSettingAsInteger(string keypath)
        {
            string path = GetSectionSpec(keypath);
            if (File.Exists(path))
                using (StreamReader sr = File.OpenText(path))
                {
                    string text = sr.ReadLine();
                    try
                    {
                        return Int64.Parse(text);
                    }
                    catch
                    {
                        ;
                    }
                }
            return Int64.MinValue;
        }
        private static string[] ReadGlobalSettingStringArray(string keypath)
        {
            string path = GetSectionSpec(keypath);
            if (File.Exists(path))
                using (StreamReader sr = File.OpenText(path))
                {
                    return sr.ReadLine().Split(delimiter, StringSplitOptions.None);
                }
            return new string[0];
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

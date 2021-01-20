using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ClarityHMI
{
    public class ControlInfo
    {
        public string Default;
        public Type type;
        public string[] values;
        public Int64?[] MinMax;
        public bool hidden;

        public ControlInfo()
        {
            Default = null;
            type = typeof(string);
            values = null;
            MinMax = null;
            hidden = false;
    }
        public ControlInfo(string[] values, bool hidden = false)
        {
            this.Default = values[0];
            this.type = typeof(string);
            this.values = values;
            this.MinMax = null;
            this.hidden = hidden;
        }
        public ControlInfo(Int64? def, Int64? min, Int64? max, bool hidden = false)
        {
            this.Default = def.HasValue ? def.Value.ToString() : null;
            this.type = typeof(Int64);
            this.values = null;
            this.MinMax = new Int64?[] { min, max };
            this.hidden = hidden;
        }
    }
    public class HMISession
    {
        public const string SEARCH  = "search";
        public const string DISPLAY = "display";
        public const string CLARITY = "clarity";
        public const string MACROS  = "macro";

        public Dictionary<string, Dictionary<string, string>> Configuration
        {
            get;
            protected set;
        }
        protected Dictionary<string, Dictionary<string, ControlInfo>> StandardConfig;
        public HMISession()
        {
            this.StandardConfig = new Dictionary<string, Dictionary<string, ControlInfo>>()
            {
                { CLARITY,  StandardConfig_CLARITY }, 
                { DISPLAY,  StandardConfig_DISPLAY },
                { SEARCH,   StandardConfig_SEARCH },
                { MACROS,   new Dictionary<string, ControlInfo>() }
            };
            this.Configuration = new Dictionary<string, Dictionary<string, string>>();
             foreach (var section in this.StandardConfig.Keys)
            {
                var defaultControls = StandardConfig[section];
                var localControls = new Dictionary<string, string>();

                this.Configuration.Add(section, localControls);
            }
        }

        private static Dictionary<string, ControlInfo> StandardConfig_DISPLAY = new Dictionary<string, ControlInfo>()
        {
            { "heading", new ControlInfo() },
            { "record",  new ControlInfo() },
            { "format",  new ControlInfo(new string[] { "text", "html", "json", "xml" }) }
        };
        private static Dictionary<string, ControlInfo> StandardConfig_SEARCH = new Dictionary<string, ControlInfo>()
        {
            { "span",    new ControlInfo(0, 0, 1000) }
        };
        private static Dictionary<string, ControlInfo> StandardConfig_CLARITY = new Dictionary<string, ControlInfo>()
        {
            { "host",    new ControlInfo() },
            { "debug",   new ControlInfo(0, 0, 1, hidden:true) },
            { "data",    new ControlInfo(new string[] { "binary", "json", "xml", "pb" }, hidden:true) }
        };

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

        protected readonly static char[] dot = new char[] { '.' };

        protected static string _root = null;
        public static string ClarityHome
        {
            get
            {
                if (_root == null)
                {
                    string dir = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                    if (dir == null || dir.Trim().Length == 0)
                    {
                        dir = ".";
                    }
                    else
                    {
                        dir = dir.Trim();

                        while (dir.EndsWith('/') || dir.EndsWith('\\'))
                            dir = dir.Substring(0, dir.Length - 1).TrimEnd();

                        if (dir == null || dir.Trim().Length == 0)
                            dir = ".";
                    }
                    _root = dir;
                }
                return Path.Combine(_root, "Clarity-HMI");  // always combine so that _root is immutable
            }
        }
        private /*deprecated*/  static bool __WriteGlobalSetting(string keypath, string value)
        {
            string path = __RemovelobalSetting(keypath);

            if (File.Exists(path))
                return false;

            // Create a file to write to.
            using (StreamWriter sw = File.CreateText(path))
            {
                sw.Write(value);
            }
            return File.Exists(path);
        }
        private /*deprecated*/  static bool __WriteGlobalSetting(string keypath, Int64 value)
        {
            string path = keypath;// GetSectionSpec(keypath);
            __RemovelobalSetting(path);

            if (File.Exists(path))
                return false;

            // Create a file to write to.
            using (StreamWriter sw = File.CreateText(path))
            {
                sw.Write(value.ToString());
            }
            return File.Exists(path);
        }
        private /*deprecated*/  static string __ReadGlobalSetting(string keypath)
        {
            string path = keypath;// GetSectionSpec(keypath);
            if (File.Exists(path))
                using (StreamReader sr = File.OpenText(path))
                {
                    return sr.ReadLine();
                }
            return null;
        }
        private /*deprecated*/  static string __RemovelobalSetting(string keypath)
        {
            string path = keypath;// GetSectionSpec(keypath);
            if (File.Exists(path))
                File.Delete(path);

            return !File.Exists(path) ? path : null;    // null return is an eror; path is returned for chaining on success.
        }
    }
}

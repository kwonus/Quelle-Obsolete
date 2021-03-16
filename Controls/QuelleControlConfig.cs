using System;
using System.Collections.Generic;
using System.IO;

namespace QuelleHMI.Definitions
{
    public abstract class QuelleControlConfig
    {
        public const string SEARCH = "search";
        public const string DISPLAY = "display";
        public const string SYSTEM = "system";

        protected static Dictionary<string, QuelleControlConfig> Configuration;
        public static CTLSearch  search    { get => (CTLSearch) Configuration[QuelleControlConfig.SEARCH]; }
        public static CTLDisplay display   { get => (CTLDisplay)Configuration[QuelleControlConfig.DISPLAY]; }
        public static CTLSystem system     { get => (CTLSystem) Configuration[QuelleControlConfig.SYSTEM]; }

        private static string appdir;
        static QuelleControlConfig()
        {
            QuelleControlConfig.appdir = QuelleControlConfig.QuelleHome;
            QuelleControlConfig.Configuration = new Dictionary<string, QuelleControlConfig>();
            var searchConf  = Path.Combine(appdir, QuelleControlConfig.SEARCH + ".yaml");
            var displayConf = Path.Combine(appdir, QuelleControlConfig.DISPLAY + ".yaml");
            var systemConf  = Path.Combine(appdir, QuelleControlConfig.SYSTEM + ".yaml");

            QuelleControlConfig.Configuration.Add(QuelleControlConfig.SEARCH, new CTLSearch(searchConf));
            QuelleControlConfig.Configuration.Add(QuelleControlConfig.DISPLAY, new CTLDisplay(displayConf));
            QuelleControlConfig.Configuration.Add(QuelleControlConfig.SYSTEM, new CTLSystem(systemConf));
        }

        public QuelleControlConfig (string file)
        {
            this.conf = file;
            this.map = new Dictionary<string, string>();
            if (File.Exists(file))
                this.Read(this.conf);            
        }
        private string conf;
        protected Dictionary<String, String> map;

        protected (bool ok, string message) Read(string file)
        {
            (bool ok, string message) result = (false, null);

            this.map.Clear();
            using (StreamReader reader = new StreamReader(file))
            {
                for (string line = reader.ReadLine(); line != null; line = reader.ReadLine())
                {
                    var parts = line.Split(new char[] { '=' }, 2, StringSplitOptions.None);
                    if (parts.Length == 2)
                    {
                        parts[0] = parts[0].Trim();
                        parts[1] = parts[1].Trim();

                        if (parts[0].Length > 0 && parts[1].Length > 0)
                        {
                            this.map.Add(parts[0], parts[1]);
                        }
                    }
                }
            }
            result.ok = true;
            return result;
        }
        protected (bool ok, string message) Write(string file)
        {
            (bool ok, string message) result = (false, null);

            using (StreamWriter writer = new StreamWriter(file))
            {
                foreach (var entry in this.map)
                {
                    writer.Write(entry.Key);
                    writer.Write("\t= ");
                    writer.WriteLine(entry.Value);
                }
            }
            result.ok = true;
            return result;

        }
        public (bool ok, string message) Update()
        {
            return this.Write(this.conf);
        }
        public (bool ok, string message) Retreive()
        {
            return this.Read(this.conf);
        }
        public (bool ok, string message) Backup(string file)
        {
            return this.Write(file);
        }
        public (bool ok, string message) Restore(string file)
        {
            var result = this.Read(file);
            if (result.ok)
                result = this.Write(this.conf);
            return result;
        }
        protected readonly static char[] dot = new char[] { '.' };

        protected static string _root = null;
        public static string QuelleHome
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
                return Path.Combine(_root, "Quelle");  // always combine so that _root is immutable
            }
        }
        public static bool IsControl(string candidate, out string normalizedName)
        {
            normalizedName = null;

            if ((candidate == null) || string.IsNullOrWhiteSpace(candidate))
                return false;

            if (candidate.Contains('.'))
            {
                var parts = HMIClause.SmartSplit(candidate, '.');
                if (parts.Length != 2)
                    return false;

                parts[0] = parts[0].ToLower();

                if (QuelleControlConfig.Configuration.ContainsKey(parts[0]))
                {
                    parts[1] = parts[1].ToLower();
                    var config = QuelleControlConfig.Configuration[parts[0]];
                    var found = config.map.ContainsKey(parts[1]) || (parts[1] == "*");
                    if (found)
                    {
                        normalizedName = candidate.ToLower();
                        return true;
                    }
                }
            }
            else
            {
                var lower = candidate.ToLower();

                if (QuelleControlConfig.Configuration.ContainsKey(lower))
                {
                    normalizedName = lower + ".*";
                    return true;
                }
                foreach (var key in QuelleControlConfig.Configuration.Keys)
                {
                    var config = QuelleControlConfig.Configuration[key];

                    if (config.map.ContainsKey(lower))
                    {
                        normalizedName = key + "." + lower;
                        return true;
                    }
                }
            }
            return false;
        }
    }
}

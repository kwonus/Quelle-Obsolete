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
        protected QuelleControlConfig(QuelleControlConfig source) // for subclass copy constructors
        {
            this.conf = null;
            this.map = new Dictionary<string, string>();

            foreach (String key in source.map.Keys)
            {
                this.map.Add(key, source.map[key]);
            }
        }
        public QuelleControlConfig (string file)
        {
            this.conf = file;
            this.map = new Dictionary<string, string>();
            if (File.Exists(file))
                this.Read(this.conf);    
        }
        public abstract bool Update(string key, string value);

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
                    var parts = line.Split(new char[] { ':' }, 2, StringSplitOptions.None);
                    if (parts.Length == 2)
                    {
                        parts[0] = parts[0].Trim();
                        parts[1] = parts[1].Trim();

                        if (parts[0].Length > 0 && parts[1].Length > 0 && !parts[1].Trim().StartsWith("!!null"))
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
                    writer.Write(": ");
                    writer.WriteLine(entry.Value != null ? entry.Value : "!!null");
                }
            }
            result.ok = true;
            return result;
        }
        protected (bool ok, string message) Update()
        {
            return this.Write(this.conf);
        }
        protected (bool ok, string message) Retreive()
        {
            return this.Read(this.conf);
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
                string folder = Path.Combine(_root, "Quelle");  // always combine so that _root is immutable
                if (!Directory.Exists(folder))
                    Directory.CreateDirectory(folder);
                return folder;
            }
        }
        public static bool IsControl(string candidate, out string normalizedName)
        {
            normalizedName = null;


            if ((candidate == null) || string.IsNullOrWhiteSpace(candidate))
                return false;

            if (candidate.Contains('.'))
            {
                var parts = Actions.Action.SmartSplit(candidate, '.');
                if (parts.Length != 2)
                    return false;

                parts[0] = parts[0].ToLower();
                parts[1] = parts[1].ToLower();

                normalizedName = parts[0] + '.' + parts[1];

                switch (parts[0])
                { 
                    case QuelleControlConfig.SEARCH:  return QuelleControlConfig.search.CONTROLS.Contains(parts[1]);
                    case QuelleControlConfig.DISPLAY: return QuelleControlConfig.display.CONTROLS.Contains(parts[1]);
                    case QuelleControlConfig.SYSTEM:  return QuelleControlConfig.system.CONTROLS.Contains(parts[1]);
                    default:
                        normalizedName = null;
                        return false;
                }
            }
            else
            {
                if (QuelleControlConfig.search.CONTROLS.Contains(candidate))
                    normalizedName = QuelleControlConfig.SEARCH + '.' + candidate.ToLower();
                else if (QuelleControlConfig.display.CONTROLS.Contains(candidate))
                    normalizedName = QuelleControlConfig.DISPLAY + '.' + candidate.ToLower();
                else if (QuelleControlConfig.system.CONTROLS.Contains(candidate))
                    normalizedName = QuelleControlConfig.SYSTEM + '.' + candidate.ToLower();
                else
                    return false;

                return true;
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using static QuelleHMI.HMISession;
using System.Linq;
using QuelleHMI.Controls;

namespace QuelleHMI
{
    public class HMIConfigurationDefault : HMISession, IQuelleConfig
    {
        private Dictionary<string, QuelleControlConfig> configs;
        public CTLSearch search   { get => (CTLSearch) configs[HMISession.SEARCH]; }
        public CTLDisplay display { get => (CTLDisplay) configs[HMISession.DISPLAY]; }
        public CTLQuelle system   { get => (CTLQuelle) configs[HMISession.SYSTEM]; }

       private string appdir;
        public HMIConfigurationDefault()
        {
            this.appdir = HMISession.QuelleHome;
            this.configs = new Dictionary<string, QuelleControlConfig>();
            var search = Path.Combine(appdir, HMISession.SEARCH + ".yaml");
            var display = Path.Combine(appdir, HMISession.DISPLAY + ".yaml");
            var system = Path.Combine(appdir, HMISession.SYSTEM + ".yaml");

            configs.Add(HMISession.SEARCH, new CTLSearch(search));
            configs.Add(HMISession.DISPLAY, new CTLDisplay(display));
            configs.Add(HMISession.SYSTEM, new CTLQuelle(system));
        }
        public class HMIResultInt : IQuelleResultInt
        {
            public Int64 result { get; internal set; }
            public bool success { get; internal set; }
            public string[] errors { get; internal set; }
            public string[] warnings { get; internal set; }

            public HMIResultInt(IQuelleResultString sresult)
            {
                if (sresult != null)
                {
                    this.result = sresult.result != null ? Int64.Parse(sresult.result) : Int64.MinValue;
                    this.success = sresult.success;
                    this.errors = sresult.errors;
                    this.warnings = sresult.warnings;
                }
                else
                {
                    this.result = Int64.MinValue;
                    this.success = false;
                    this.errors = null;
                    this.warnings = null;
                }
            }
        }

        protected (string section, string setting, IQuelleResultString error) GetPair(string spec)
        {
            if (spec == null)
                return (null, null, new HMIResultString(error: "Driver design error"));

            string[] parts = spec.Split(dot, StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length > 2 || parts.Length < 1)
                return (null, null, new HMIResultString(error: "Driver design error"));

            (string section, string setting) key = (parts.Length == 1) ? (null, parts[0].Trim().ToLower()) : (parts[0].Trim().ToLower(), parts[1].Trim().ToLower());

            if (key.section == null)
            {
                foreach (string configSection in this.StandardConfig.Keys)
                    if (this.StandardConfig[configSection].ContainsKey(key.setting))
                    {
                        key.section = configSection;
                        break;
                    }
            }
            if (key.section == null)
            {
                if (key.setting == "*")
                    return (null, null, new HMIResultString(error: "* here is a syntax error; please specify: " + SEARCH + ", " + DISPLAY + ", or " + SYSTEM + "to enumerate all available control settins."));
                else if (key.setting == SEARCH || key.setting == DISPLAY || key.setting == SYSTEM)
                    return (key.setting, "*", null);  // swap positions
                else
                    return (null, null, new HMIResultString(error: "Driver design error"));
            }
            return (key.section, key.setting, null);
        }
        public IQuelleResultString Read(string setting)
        {
            (string section, string setting, IQuelleResultString error) key = GetPair(setting);
            if (key.error != null)
                return key.error;

            string result = null;
            string warning = null;

            var standard = this.StandardConfig[key.section];
            var config = this.Configuration[key.section];
            if (key.setting == "*")
            {
                foreach (string item in standard.Keys)
                {
                    var std = standard[item];

                    if (std.hidden)
                        continue;
                    var entry = item + ":";
                    if (config.ContainsKey(item))
                        entry += ("\t" + config[item].Trim());
                    else if (std.Default != null)
                        entry += ("\t" + std.Default + "\t[default]");
                    
                    if (result == null)
                        result = entry;
                    else
                        result += ("\n" + entry);
                }
                if (result == null)
                    warning = "No control variables found";
            }
            else if (config.ContainsKey(key.setting))
            {
                result = config[key.setting];
            }
            else if (standard.ContainsKey(key.setting))
            {
                result = standard[key.setting].Default; // + "\t[default]";
            }

            if (result != null && result.Length < 1)
                result = null;

            return result != null
                ? new HMIResultString(result: result, warning: warning)
                : new HMIResultString(warning: warning).AddWarning("Unable to read setting (Maybe it has not been set");
        }
        public IQuelleResultInt ReadInt(string setting)
        {
            IQuelleResultString result = this.Read(setting);
            return new HMIResultInt(result);
        }

        public IQuelleResult Remove(string setting)
        {
            (string section, string setting, IQuelleResultString error) key = GetPair(setting);
            if (key.error != null)
                return key.error;

            string result = null;
            string warning = null;

            var config = this.Configuration[key.section];
            if (key.setting == "*")
            {
                result = "*";

                foreach (string item in this.StandardConfig[key.section].Keys)
                {
                    if (config.ContainsKey(item))
                        config.Remove(item);

                    var info = this.StandardConfig[key.section][item];
                    if (info.Default != null)
                        config[item] = info.Default;
                }
            }
            else if (config.ContainsKey(key.setting))
            {
                result = config[key.setting];
            }

            if (result != null && result.Length < 1)
                result = null;

            return result != null
                ? new HMIResultString(result: result, warning: warning)
                : new HMIResultString(warning: warning).AddWarning("Unable to read setting (Maybe it has not been set");
        }

        public IQuelleResult Write(string setting, string value)
        {
            (string section, string setting, IQuelleResultString error) key = GetPair(setting);
            if (key.error != null)
                return key.error;

            var normalizedValue = value.Trim().ToLower();
            if (string.IsNullOrWhiteSpace(value))
                new HMIResult(error: "A value must be specified when defining control variables; use a removal command instead if you want to meove the value");

            var config = this.Configuration[key.section];
            if (config.ContainsKey(key.setting))
                config.Remove(key.setting);

            config[key.setting] = normalizedValue;

            return new HMIResult(success: true);
        }
        public IQuelleResult Write(string setting, Int64 value)
        {
            return Write(setting, value.ToString());
        }
    }
}

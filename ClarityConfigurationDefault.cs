using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using static ClarityHMI.HMISession;
using System.Linq;

namespace ClarityHMI
{
    public class ClarityConfigurationDefault : HMISession, IClarityConfig
    {
        public string seachConf { get => Path.Combine(appdir, HMISession.SEARCH + ".conf"); }
        public string displayConf { get => Path.Combine(appdir, HMISession.DISPLAY + ".conf"); }
        public string clarityConf { get => Path.Combine(appdir, HMISession.CLARITY + ".conf"); }

        private string appdir;
        public ClarityConfigurationDefault()
        {
            this.appdir = HMISession.ClarityHome;
        }
        public class ClarityResultInt : IClarityResultInt
        {
            public Int64 result { get; internal set; }
            public bool success { get; internal set; }
            public string[] errors { get; internal set; }
            public string[] warnings { get; internal set; }

            public ClarityResultInt(IClarityResultString sresult)
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

        protected (string section, string setting, IClarityResultString error) GetPair(string spec)
        {
            if (spec == null)
                return (null, null, new ClarityResultString(error: "Driver design error"));

            string[] parts = spec.Split(dot, StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length > 2 || parts.Length < 1)
                return (null, null, new ClarityResultString(error: "Driver design error"));

            (string section, string setting) key = (parts.Length == 1) ? (null, parts[0].Trim().ToLower()) : (parts[0].Trim().ToLower(), parts[1].Trim().ToLower());

            if (key.section == null)
            {
                foreach (string configSection in this.Configuration.Keys)
                    if (this.Configuration.ContainsKey(key.setting))
                    {
                        key.section = configSection;
                        break;
                    }
            }
            if (key.section == null)
            {
                if (key.setting == "*")
                    return (null, null, new ClarityResultString(error: "* here is a syntax error; please specify: " + SEARCH + ", " + DISPLAY + ", or " + CLARITY + "to enumerate all available control settins."));
                else if (key.setting == SEARCH || key.setting == DISPLAY || key.setting == CLARITY)
                    return (key.setting, "*", null);  // swap positions
                else
                    return (null, null, new ClarityResultString(error: "Driver design error"));
            }
            return (key.section, key.setting, null);
        }
        public IClarityResultString Read(string setting, HMIScope scope)
        {
            (string section, string setting, IClarityResultString error) key = GetPair(setting);
            if (key.error != null)
                return key.error;

            switch (scope)
            {
                case HMIScope.Session:  
                case HMIScope.System:   break;
                case HMIScope.Cloud:    return new ClarityResultString(error: "This driver does not support Cloud Drivers!");
                default:                return new ClarityResultString(error: "Driver design error", warning: "Unknown setting scope provided by driver");
            }
            string result = null;
            string warning = null;

            var config = this.Configuration[key.section];
            if (key.setting == "*")
            {
                result = "*";

                foreach (string item in this.Configuration.Keys)
                    if (result == null)
                        result = item + ":\t" + config[item];
                    else
                        result += ("\n" + key + ":\t" + config[item]);

                if (result == null)
                    warning = "No control variables found";
            }
            else if (config.ContainsKey(key.setting))
            {
                result = config[key.setting];
            }

            if (result != null && result.Length < 1)
                result = null;

            return result != null
                ? new ClarityResultString(result: result, warning: warning)
                : new ClarityResultString(warning: warning).AddWarning("Unable to read setting (Maybe it has not been set");
        }
        public IClarityResultInt ReadInt(string setting, HMIScope scope)
        {
            IClarityResultString result = this.Read(setting, scope);
            return new ClarityResultInt(result);
        }

        public IClarityResult Remove(string setting, HMIScope scope)
        {
            (string section, string setting, IClarityResultString error) key = GetPair(setting);
            if (key.error != null)
                return key.error;

            switch (scope)
            {
                case HMIScope.Session:
                case HMIScope.System:   break;
                case HMIScope.Cloud:    return new ClarityResultString(error: "This driver does not support Cloud Drivers!");
                default:                return new ClarityResultString(error: "Driver design error", warning: "Unknown setting scope provided by driver");
            }
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
                ? new ClarityResultString(result: result, warning: warning)
                : new ClarityResultString(warning: warning).AddWarning("Unable to read setting (Maybe it has not been set");
        }

        public IClarityResult Write(string setting, HMIScope scope, string value)
        {
            (string section, string setting, IClarityResultString error) key = GetPair(setting);
            if (key.error != null)
                return key.error;

            switch (scope)
            {
                case HMIScope.Session:
                case HMIScope.System: break;
                case HMIScope.Cloud: return new ClarityResultString(error: "This driver does not support Cloud Drivers!");
                default: return new ClarityResultString(error: "Driver design error", warning: "Unknown setting scope provided by driver");
            }
            var normalizedValue = value.Trim().ToLower();
            if (string.IsNullOrWhiteSpace(value))
                new ClarityResult(error: "A value must be specified when defining control variables; use a removal command instead if you want to meove the value");

            var config = this.Configuration[key.section];
            if (config.ContainsKey(key.setting))
            {
                config.Remove(key.setting);
                config[key.setting] = normalizedValue;
            }
            return new ClarityResult(success: true);
        }
        public IClarityResult Write(string setting, HMIScope scope, Int64 value)
        {
            return Write(setting, scope, value.ToString());
        }
    }
}

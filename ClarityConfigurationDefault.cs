using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using static ClarityHMI.HMISession;
using System.Linq;

namespace ClarityHMI
{
    public class ClarityConfigurationDefault : IClarityConfig
    {
        public class ClarityResultInt: IClarityResultInt
        {
            public Int64 result { get; internal set; }
            public bool success { get; internal set; }
            public string[] errors { get; internal set; }
            public string[] warnings { get; internal set; }

            public ClarityResultInt(Int64? result = null, string error = null, string warning = null)
            {
                this.result = result != null ? result.Value : Int64.MinValue;
                this.success = (result != null) && (error == null);
                this.errors = (error != null) ? new string[] { error.Trim() } : null;
                this.warnings = (warning != null) ? new string[] { warning.Trim() } : null;
            }
            public ClarityResultInt()
            {
                this.result = Int64.MinValue;
                this.success = false;
                this.errors = null;
                this.warnings = null;
            }
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
        public class ClarityResultArray : IClaritResultStringyArray
        {
            public string[] results { get; internal set; }
            public bool success { get; internal set; }
            public string[] errors { get; internal set; }
            public string[] warnings { get; internal set; }

            public ClarityResultArray(string[] results, string error = null, string warning = null)
            {
                this.results = results;
                this.success = (results != null) && (error == null);
                this.errors = (error != null) ? new string[] { error.Trim() } : null;
                this.warnings = (warning != null) ? new string[] { warning.Trim() } : null;
            }
            public ClarityResultArray()
            {
                this.results = null;
                this.success = false;
                this.errors = null;
                this.warnings = null;
            }
            public ClarityResultArray(IClarityResultString sresult)
            {
                if (sresult != null)
                {
                    this.results = sresult.result != null ? sresult.result.Split(delimiter, StringSplitOptions.RemoveEmptyEntries) : null;
                    this.success = sresult.success;
                    this.errors = sresult.errors;
                    this.warnings = sresult.warnings;
                }
                else
                {
                    this.results = null;
                    this.success = false;
                    this.errors = null;
                    this.warnings = null;
                }
            }
        }
        private readonly static string[] delimiter = new string[] { "<|||||||>" };

        public IClarityResultString Read(string setting, HMIScope scope)
        {
            return HMISession.Show(setting, scope);
        }
        public IClaritResultStringyArray ReadArray(string setting, HMIScope scope)
        {
            IClarityResultString result = HMISession.Show(setting, scope);
            return new ClarityResultArray(result);

        }
        public IClarityResultInt ReadInt(string setting, HMIScope scope)
        {
            IClarityResultString result = HMISession.Show(setting, scope);
            return new ClarityResultInt(result);
        }

        public IClarityResult Remove(string setting, HMIScope scope)
        {
            return HMISession.Remove(setting, scope);
        }

        public IClarityResult Write(string setting, HMIScope scope, string value)
        {
            return HMISession.Config(setting, scope, value);
        }
        public IClarityResult Write(string setting, HMIScope scope, string[] values)
        {
            string value = values != null ? string.Join(delimiter[0], values) : null;
            return HMISession.Config(setting, scope, value);
        }
        public IClarityResult Write(string setting, HMIScope scope, Int64 value)
        {
             return HMISession.Config(setting, scope, value.ToString());
        }
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace QuelleHMI
{
    public abstract class HMISession
    {
        public class HMIResultString : IQuelleResultString
        {
            public string result { get; protected set; }
            public bool success { get; protected set; }
            public string[] errors { get; protected set; }
            public string[] warnings { get; protected set; }

            public HMIResultString(string result = null, string error = null, string warning = null)
            {
                this.result = result;
                this.success = (result != null) && (error == null);
                this.errors = (error != null) ? new string[] { error.Trim() } : null;
                this.warnings = (warning != null) ? new string[] { warning.Trim() } : null;
            }
            public HMIResultString()
            {
                this.result = null;
                this.success = false;
                this.errors = null;
                this.warnings = null;
            }
            public HMIResultString AddWarning(string warning)
            {
                int resize = this.warnings != null ? this.warnings.Length : 1;
                string[] array = new string[resize];
                for (int i = 0; i < resize - 1; i++)
                    array[i] = this.warnings[i];
                array[resize - 1] = warning;
                this.warnings = array;

                return this;
            }
            private HMIResultString(IQuelleResultObject result)
            {
                this.result = (string)result.result;
                this.success = result.success;
                this.errors = result.errors;
                this.warnings = result.warnings;
            }
            public static HMIResultString Create(IQuelleResultObject result)
            {
                return (result != null) ? new HMIResultString(result) : new HMIResultString();
            }
        }
        public class HMIResult : IQuelleResult
        {
            public bool success { get; protected set; }
            public string[] errors { get; protected set; }
            public string[] warnings { get; protected set; }

            public HMIResult(bool success)
            {
                this.success = success;
                this.errors = null;
                this.warnings = null;
            }
            public HMIResult(string error)
            {
                this.success = false;
                this.errors = new string[] { error.Trim() };
                this.warnings = null;
            }
            public HMIResult(string error, string warning)
            {
                this.success = false;
                this.errors = new string[] { error.Trim() };
                this.warnings = new string[] { warning.Trim() };
            }
            public HMIResult(bool success, string warning)
            {
                this.success = success;
                this.errors = null;
                this.warnings = new string[] { warning.Trim() };
            }
            public HMIResult()
            {
                this.success = false;
                this.errors = null;
                this.warnings = null;
            }
        }
    }
}

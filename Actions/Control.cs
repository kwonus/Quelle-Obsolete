using QuelleHMI.Definitions;
using System;
using System.Collections.Generic;
using System.Text;

namespace QuelleHMI.Actions
{
    public class Control : HMIClause
    {
        public const string SYNTAX = "CONTROL";
        public override string syntax { get => SYNTAX; }
        public const string SET = "set";
        public const string CLEAR = "clear";
        public static readonly List<string> IMPLICIT = new List<string>() { SET, CLEAR };
        public string controlName { get; private set; }
        public string controlValue { get; private set; }

        public Control(HMIStatement statement, UInt32 segmentOrder, string verb, string segment)
        : base(statement, segmentOrder, HMIPolarity.UNDEFINED, segment, HMIClauseType.IMPLICIT)
        {
            this.verb = verb;
        }
        protected override bool Parse()
        {
            if (this.segment == null || this.segment.Trim().Length == 0)
            {
                this.Notify("error", "Cannot parse an empty clause");
                return false;
            }
            var action = Control.GetAction(this.segment);
            if (action.tokens != null && action.tokens.Length >= 1 && action.error == null)
            {
                this.controlName = action.tokens[0].Trim();
                this.controlValue = action.tokens.Length > 1 ? action.tokens[1].Trim() : null;
                this.verb = action.verb;
            }
            return this.verb == Control.CLEAR || this.verb == Control.SET;
        }
        public static readonly char[] dot = new char[] { '.' };
        public override bool Execute()
        {
            if (this.errors.Count == 0)
            {
                var parts = this.controlName.Split(dot);
                var value = this.verb == Control.CLEAR ? null : this.controlValue;
                if (parts.Length == 2)
                {
                    switch (parts[0])
                    {
                        case QuelleControlConfig.SEARCH:  return QuelleControlConfig.search.Update(parts[1],  value);
                        case QuelleControlConfig.DISPLAY: return QuelleControlConfig.display.Update(parts[1], value);
                        case QuelleControlConfig.SYSTEM:  return QuelleControlConfig.system.Update(parts[1],  value);
                    }
                }
                this.errors.Add("Ill-defined control action provided by user");
            }
            return false;
        }
        public static (string verb, string[] tokens, string error) GetAction(string text)
        {
            string[] tokens = null;
            string error = null;
            int equals = text.IndexOf('=');
            if (equals > 0 && equals+1 < text.Length)
            {
                var value = text.Substring(equals+1).Trim();
                var first = text.Substring(0, equals).Trim();
                string control;
                if (QuelleControlConfig.IsControl(first, out control))
                {
                    if (value.Length > 0)
                    {
                        if (value == "@")  // This is a CONTROL::clear
                            return (Control.CLEAR, new string[] { control }, null);

                        return (Control.SET, new string[] { control, value }, null);
                    }
                    return (Control.SET, new string[] { control, value }, "Attempting to set an unknown control");
                }
                else
                {
                    if (value.Length > 0)
                    {
                        if (value == "@")  // This is a CONTROL::clear
                            return (Control.CLEAR, new string[] { control }, "Attempting to clear an unknown control");

                        return (Control.SET, new string[] { control, value }, "Attempting to set an unknown control");
                    }
                    return (Control.SET, new string[] { control, value }, "Unknown control and invalid syntax");
                }
            }
            else if (equals >= 0)
            {
                return (Control.SET, null, "Invalid CONTROL syntax: Badly placed equal sign");
            }
            return (null, null, null);  // NOT A CONTROL ... AND NO ERRORS
        }
        public static string Help(string verb)  // SET or CLEAR
        {
            return "";
        }
    }
}

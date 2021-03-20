using QuelleHMI.Definitions;
using System;
using System.Collections.Generic;
using System.Text;

namespace QuelleHMI.Actions
{
    public class Control : Action
    {
        public const string SYNTAX = "CONTROL";
        public override string syntax { get => SYNTAX; }
        public const string SET = "set";
        public const string CLEAR = "clear";
        public static readonly List<string> IMPLICIT = new List<string>() { SET, CLEAR };
        public string controlGroup { get; private set; }
        public string controlName { get; private set; }
        public string controlValue { get; private set; }

        public Control(HMIStatement statement, UInt32 segmentOrder, string verb, string segment, string name, string value)
        : base(statement, HMIClauseType.IMPLICIT, segmentOrder, segment)
        {
            int dot = name != null ? name.IndexOf('.') : -1;
            if (dot > 0 && dot+1 < name.Length)
            {
                this.verb = verb;
                this.controlName = name.Substring(dot+1);
                this.controlGroup = name.Substring(0, dot);
                this.controlValue = value;
            }
            else
            {
                this.verb = "";
                this.controlName = name;
                this.controlValue = value;
            }
        }
        public override bool Parse()
        {
            // little to do the constructor did all the parsing already
            //
            return this.verb == Control.CLEAR || this.verb == Control.SET;
        }
        public static readonly char[] dot = new char[] { '.' };
        public override bool Execute()
        {
            if (this.errors.Count == 0)
            { 
                var value = this.verb == Control.CLEAR ? null : this.controlValue;
                switch (this.controlGroup)
                {
                    case QuelleControlConfig.SEARCH:  return QuelleControlConfig.search.Update(this.controlName,  value);
                    case QuelleControlConfig.DISPLAY: return QuelleControlConfig.display.Update(this.controlName, value);
                    case QuelleControlConfig.SYSTEM:  return QuelleControlConfig.system.Update(this.controlName,  value);
                }
                this.errors.Add("Ill-defined control action provided by user");
            }
            return false;
        }
        public static (Action action, string error) GetAction(HMIStatement statement, string segment, uint order)
        {
            string text = statement.statement;
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
                            return (new Control(statement, order, Control.CLEAR, segment, control, value), null);

                        return (new Control(statement, order, Control.SET, segment, control, value), null);
                    }
                    return (null, "Attempting to set a control with an ill-formed value");
                }
                else
                {
                    if (value.Length > 0)
                    {
                        if (value == "@")  // This is a CONTROL::clear
                            return (null, "Attempting to clear an unknown control");

                        return (null, "Attempting to set an unknown control");
                    }
                    return (null, "Unknown control and invalid syntax");
                }
            }
            else if (equals >= 0)
            {
                return (null, "Invalid CONTROL syntax: Badly placed equal sign");
            }
            return (null, null);  // NOT A CONTROL ... AND NO ERRORS
        }
        public static string Help(string verb)  // SET or CLEAR
        {
            return "";
        }
    }
}

using QuelleHMI.Controls;
using System;
using System.Collections.Generic;
using System.Text;

namespace QuelleHMI.Verbs
{
    public class Control : HMIClause
    {
        public const string SYNTAX = "CONTROL";
        public override string syntax { get => SYNTAX; }
        public const string SET = "set";
        public const string CLEAR = "clear";
        public string controlName { get; private set; }
        public string controlValue { get; private set; }

        public Control(HMIStatement statement, UInt32 segmentOrder, string segment)
    : base(statement, segmentOrder, HMIPolarity.UNDEFINED, segment, HMIClauseType.IMPLICIT)
        {
            if (this.verb == null)
                this.verb = this.syntax;
        }
        protected override bool Parse()
        {
            if (this.segment == null || this.segment.Trim().Length == 0)
            {
                this.Notify("error", "Cannot parse an empty clause");
                return false;
            }
            var pair = Control.GetTokenPair(this.segment);
            if (pair.token != null && pair.token.Length >= 1 && pair.error == null)
            {
                this.controlName = pair.token[0].Trim();
                this.controlValue = pair.token.Length > 1 ? pair.token[1].Trim() : null;
                this.verb = pair.verb;
            }
            return this.verb == Control.CLEAR || this.verb == Control.SET;
        }
        public override bool Execute()
        {
            if (this.errors.Count == 0)
            {
                var result = QuelleMacro.Create(this.controlName, this.controlValue);
                if (result == null)
                {
                    this.errors.Add("Could not create macro");
                }
            }
            return (this.errors.Count == 0);
        }
        private static (string verb, string[] token, string error) GetTokenPair(string text)
        {
            // TODO: How to handle =@
            int i;
            int offset;
            int len = text.Length;
            (string verb, string[] tokens, string error) result = (Control.SYNTAX, null, null);

            for (offset = 0; offset < len; offset++)
            {
                char c = text[offset];
                if (!char.IsWhiteSpace(c))
                    break;
            }
            if (offset >= len)
            {
                return result;          // this is not an error. It just means that there was trailing whitespace
            }
            bool c_quoted = false;
            string[] tokens = null;
            string remainder;
            for (i = offset; i < len; i++)
            {
                if (c_quoted)
                {
                    c_quoted = false;
                    i++;
                    continue;
                }
                char c = text[i];
                if ((tokens == null) && c == '\\')
                {
                    c_quoted = true;
                    continue;
                }
                remainder = text.Substring(i);
                if (c == '=')
                {
                    if (i + 1 < len)
                    {
                        result.verb = Control.SET;
                        tokens = new string[] { text.Substring(offset, i - offset).Trim().ToLower(), text.Substring(i + 1).Trim().ToLower() };
                    }
                    break;
                }
                else if (remainder.StartsWith("::"))
                {
                    var semicolon = i;
                    if (i + 2 < len)
                    {
                        i += 2;
                        remainder = remainder.Substring(2).Trim().ToLower();
                        if (remainder.StartsWith(Control.CLEAR) && ((i + Control.CLEAR.Length) < len))
                        {
                            i += Control.CLEAR.Length;
                            remainder = remainder.Substring(Control.CLEAR.Length).Trim();
                            if (remainder.StartsWith('!'))
                            {
                                result.verb = Control.CLEAR;
                                tokens = new string[] { text.Substring(offset, semicolon - offset).Trim().ToLower() };
                            }
                        }
                    }
                    break;
                }
            }
            if (tokens != null && tokens[0] != null && tokens[0].Length > 0 && ((result.verb == Control.CLEAR) || (result.verb != Control.SET && tokens[1] != null && tokens[1].Length > 0)))
            {
                string normalized;
                if (QuelleControlConfig.IsControl(tokens[0], out normalized))
                {
                    tokens[0] = normalized;
                    result.tokens = tokens;
                    return result;
                }
            }
            result.error = "Comtrol setting is malformed";
            return result;
        }
        //  Assume that an explicit verb has not been passed
        //  (We will not be looking for explicit verbs here)
        //
        public static bool Test(string text)
        {
            if (text != null && text.Trim().Length > 0)
            {
                var pair = Control.GetTokenPair(text);
                if (pair.token != null && pair.error == null)
                {
                    return (pair.verb == Control.CLEAR && pair.token.Length == 1) || (pair.verb == Control.SET && pair.token.Length == 2 && pair.token[1].Length > 0);
                }
            }
            return false;
        }
        public static string Help(string verb)  // SET or CLEAR
        {
            return "";
        }
    }
}

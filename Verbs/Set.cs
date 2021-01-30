using System;
using System.Collections.Generic;
using System.Text;

namespace QuelleHMI.Verbs
{
    public class Set : HMIClause
    {
        public const string VERB = "set";
        public string controlName { get; private set; }
        public string controlValue { get; private set; }
 
        public Set(HMIStatement statement, UInt32 segmentOrder, string segment)
    : base(statement, segmentOrder, HMIPolarity.UNDEFINED, segment, HMIClauseType.IMPLICIT)
        {
            this.maximumScope = HMIScope.System;
            this.verb = VERB;
        }
        protected override bool Parse()
        {
            if (this.segment == null || this.segment.Trim().Length == 0)
            {
                this.Notify("error", "Cannot parse an empty clause");
                return false;
            }
            var pair = Set.GetTokenPair(this.segment);
            if (pair.token != null && pair.token.Length == 2 && pair.error == null)
            {
                this.controlName = pair.token[0].Trim();
                this.controlValue = pair.token[1].Trim();
            }
            return false;
        }
        public override bool Execute()
        {
            if (this.errors.Count == 0)
            {
                var result = HMICommand.Driver.Write("quelle.macro." + this.controlName, this.statement.scope, this.controlValue);
                if (result.errors != null)
                {
                    foreach (var error in result.errors)
                        this.errors.Add(error);
                }
                else if (!result.success)
                {
                    this.errors.Add("Unspecific macro error; Please contact vendor about this Quelle driver implementation");
                }
                if (result.warnings != null)
                {
                    foreach (var warnings in result.warnings)
                        this.warnings.Add(warnings);
                }
            }
            return (this.errors.Count == 0);
        }
        private static (string[] token, string error) GetTokenPair(string text)
        {
            int i;
            int offset;
            int len = text.Length;
            (string[] tokens, string error) result = (null, null);

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
                if (c == '=')
                {
                    if (i+1 < len)
                        tokens = new string[] { text.Substring(offset, i-offset).Trim().ToLower(), text.Substring(i+1).Trim().ToLower() };
                    break;
                }
            }
            if (tokens != null && tokens[0] != null && tokens[1] != null && tokens[0].Length > 0 && tokens[1].Length > 0)
            {
                string normalized;
                if (HMISession.IsControl(tokens[0], out normalized))
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
            if (text == null || text.Trim().Length == 0)
            {
                return false;
            }
            var pair = Set.GetTokenPair(text);
            return (pair.token != null && pair.token.Length == 2 && pair.error == null);
        }
    }
}

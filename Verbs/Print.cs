using QuelleHMI.Fragments;
using System;
using System.Collections.Generic;
using System.Text;

namespace QuelleHMI.Verbs
{
    public interface IQuelleDisplayClause
    {
        string[] specification { get; }
    }
    public class Print: HMIClause, IQuelleDisplayClause
    {
        public const string SYNTAX = "DISPLAY";
        public override string syntax { get => SYNTAX; }
        public const string VERB = "@print";
        public Dictionary<UInt64, PrimativeFragment> fragments { get; private set; }

        public string[] specification
        {
            get
            {
                var spec = new string[this.fragments.Count];
                int i = 0;
                foreach (var frag in this.fragments.Values)
                    spec[i++] = frag.text;
                return spec;
            }
        }

        public Print(HMIStatement statement, UInt32 segmentOrder, string segment)
            : base(statement, segmentOrder, HMIPolarity.UNDEFINED, segment, HMIClauseType.EXPLICIT_INDEPENDENT)
        {
            this.verb = VERB;

        }
        protected override bool Parse()
        {
            var segment = this.segment.Trim();
            if (segment.StartsWith(VERB))
                segment = segment.Substring(VERB.Length).TrimStart();

            int len = segment.Length;
            string error = null;
            uint sequence = 1;

            for (var frag = this.GetNextPrintToken(segment); (frag.error == null) && (frag.offset > 0) && (frag.offset <= len || frag.token != null);
                     frag = this.GetNextPrintToken(segment, frag.offset))
            {
                if (frag.error != null)
                {
                    error = frag.error;
                    break;
                }
                if (frag.token != null)
                {
                    sequence++;
                    var current = new PrimativeFragment(this, frag.token, sequence);
                    this.fragments.Add(sequence, current);
                }
                if (frag.offset >= len)
                    break;
            }
            if (error != null)
            {
                this.Notify("error", error);
                return false;
            }
            return true;
        }
        public override bool Execute()
        {
            throw new NotImplementedException();
        }
        private (string token, int offset, string error) GetNextPrintToken(string text, int offset = 0)
        {
            if (text == null)
                return (null, -1, "Cannot pass in null value for parsing");
            if (offset < 0)
                return (null, -1, "Cannot pass in a negative offset for parsing");

            int len = text.Length;
            (string token, int offset, string error) result = (null, offset, null);

            for (result.offset = offset; result.offset < len; result.offset++)
            {
                char c = text[result.offset];
                if (!char.IsWhiteSpace(c))
                    break;
            }
            if (result.offset >= len)
            {
                result.offset = len;   // mark end-of-text
                return result;          // this is not an error. It just means that there was trailing whitespace
            }
            offset = result.offset;
            bool macro = false;
            bool c_quoted = false;

            for (result.offset = offset; result.offset < len; result.offset++)
            {
                if (c_quoted)
                {
                    c_quoted = false;
                    result.offset++;
                    continue;
                }
                char c = text[result.offset];

                int isBrace = 0;

                if (char.IsWhiteSpace(c) && !macro)
                {
                    break;
                }
                else switch (c)
                    {
                        case '\\': c_quoted = true; continue;
                        case '[': isBrace = (-1); break;
                        case ']': isBrace = (1); break;

                        default: continue;
                    }
                if (isBrace < 0)
                {
                    if (macro)
                    {
                        result.error = "Fragment contained nested open braces, but nesting braces is not supported.";
                        return result;
                    }
                    macro = true;
                }
                else if (isBrace > 0)
                {
                    if (!macro)
                    {
                        result.error = "Fragment contained a closing brace, but no corresponding open brace.";
                        return result;
                    }
                    macro = false;
                    result.token = text.Substring(offset, ++result.offset - offset);
                    break;
                }
            }
            if (true)
            {
                ;
            }
            if (result.offset >= len)
            {
                result.offset = len;
                if (macro)
                    result.error = "Fragment contained an open brace. but no no closing brace.";
            }
            if (result.error == null && result.token == null)
            {
                result.token = text.Substring(offset, result.offset - offset);
            }
            return result;
        }
        public static string Help()
        {
            return "";
        }
    }
}

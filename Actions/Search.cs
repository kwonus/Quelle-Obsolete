using QuelleHMI.Fragments;
using System;
using System.Collections.Generic;
using System.Linq;

namespace QuelleHMI.Actions
{
    public interface IQuelleSearchClause
    {
        IQuelleSearchFragment[] fragments { get; }
        string segment { get; }
        byte polarity { get; }
    }
    public class Search : Actions.Action, IQuelleSearchClause
    {
        public const string SYNTAX = "SEARCH";
        public override string syntax { get => SYNTAX; }
        public const string FIND = "find";

        public static readonly List<string> IMPLICIT = new List<string>() { FIND };

        protected Dictionary<UInt64, SearchFragment> searchFragments;
        public IQuelleSearchFragment[] fragments
        {
            get
            {
                var list = (from key in this.searchFragments.Keys orderby key select this.searchFragments[key]);
                return list.ToArray();
            }
        }

        private Boolean quoted;

        public override bool Parse()
        {
            this.quoted = this.ParseQuotedSearch() || (this.errors.Count > 0);

            if (this.errors.Count > 0)
                return false;
            else if (this.quoted)
                return true;

            return this.ParseUnquotedSearch();
        }
        public override bool Execute()
        {
            // throw new NotImplementedException();
            return true;
        }

        public Search(HMIStatement statement, UInt32 segmentOrder, HMIPolarity polarity, string segment)
        : base(statement, HMIClauseType.IMPLICIT, segmentOrder, segment, polarity)
        {
            this.verb = FIND;
        }
        private (string token, int offset, string error) GetNextUnquotedSearchToken(string text, int offset = 0)
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
            bool parenthetical = false;
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

                int isParen = 0;

                if (char.IsWhiteSpace(c) && !parenthetical)
                {
                    break;
                }
                else switch (c)
                    {
                        case '\\': c_quoted = true; continue;
                        case '(': isParen = (-1); break;
                        case ')': isParen = (1); break;

                        default: continue;
                    }
                if (isParen < 0)
                {
                    if (parenthetical)
                    {
                        result.error = "Fragment contained nested open parenthesis, but nesting parentheticals is not supported.";
                        return result;
                    }
                    parenthetical = true;
                }
                else if (isParen > 0)
                {
                    if (!parenthetical)
                    {
                        result.error = "Fragment contained a closing parenthesis, but no corresponding open parenthesis.";
                        return result;
                    }
                    parenthetical = false;
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
                if (parenthetical)
                    result.error = "Fragment contained an open parenthesis. but no no closing parentheis.";
            }
            if (result.error == null && result.token == null)
            {
                result.token = text.Substring(offset, result.offset - offset);
            }
            return result;
        }
        private (string token, int offset, bool bracketed, bool ordered, string error) GetNextQuotedSearchToken(string text, int offset = 0, bool bracketed = false)
        {
            if (text == null)
                return (null, -1, false, !bracketed, "Cannot pass in null value for parsing");
            if (offset < 0)
                return (null, -1, false, !bracketed, "Cannot pass in a negative offset for parsing");

            int len = text.Length;
            (string token, int offset, bool bracketed, bool ordered, string error) result = (null, offset, bracketed, !bracketed, null);

            for (result.offset = offset; result.offset < len; result.offset++)
            {
                char c = text[result.offset];
                if (!char.IsWhiteSpace(c))
                    break;
            }
            if (result.offset >= len)
            {
                result.offset = len;   // mark end-of-text
                if (result.bracketed)
                    result.error = "Fragment contained an open square-brace. but no no closing square0-race.";
                return result;          // this is not an error. It just means that there was trailing whitespace
            }
            offset = result.offset;
            bool parenthetical = false;
            if (text.Substring(result.offset).StartsWith("..."))
            {
                result.offset += 3;
                result.token = "...";
            }
            else
            {
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
                    int isParen = 0;

                    if (char.IsWhiteSpace(c) && !parenthetical)
                    {
                        break;
                    }
                    else switch (c)
                        {
                            case '\\': c_quoted = true; continue;
                            case '[': isBrace = (-1); break;
                            case ']': isBrace = (1); break;
                            case '(': isParen = (-1); break;
                            case ')': isParen = (1); break;

                            default: continue;
                        }
                    if (isParen < 0)
                    {
                        if (parenthetical)
                        {
                            result.error = "Fragment contained nested open parenthesis, but nesting parentheticals is not supported.";
                            return result;
                        }
                        parenthetical = true;
                    }
                    else if (isParen > 0)
                    {
                        if (!parenthetical)
                        {
                            result.error = "Fragment contained a closing parenthesis, but no corresponding open parenthesis.";
                            return result;
                        }
                        parenthetical = false;
                        result.token = text.Substring(offset, ++result.offset - offset);
                        break;
                    }
                    if (isBrace < 0)
                    {
                        if (parenthetical)
                        {
                            result.error = "Square braces cannot be contained within parenthetical tokens.";
                            return result;
                        }
                        if (result.bracketed)
                        {
                            result.error = "Fragment contained nested open square-braces, but nesting is not supported.";
                            return result;
                        }
                        result.bracketed = true;
                    }
                    else if (isBrace > 0)
                    {
                        if (parenthetical)
                        {
                            result.error = "Parenthesis and/or square-brace mismatch. Punctuation of this type must match.";
                            return result;
                        }
                        if (!result.bracketed)
                        {
                            result.error = "Fragment contained a closing square-brace, but no corresponding open square-brace.";
                            return result;
                        }
                        result.bracketed = true;
                        break;
                    }
                }
            }
            if (result.offset >= len)
            {
                result.offset = len;   // mark end-of-text with (-1)
                if (result.bracketed)
                    result.error = "Fragment contained an open square-brace. but no no closing square-brace.";
                else if (parenthetical)
                    result.error = "Fragment contained an open parenthesis. but no no closing parentheis.";
            }
            if (result.error == null && result.token == null)
            {
                result.token = text.Substring(offset, result.offset - offset);
            }
            return result;
        }
        private bool ParseUnquotedSearch()
        {
            this.searchFragments = new Dictionary<UInt64, SearchFragment>();

            int len = this.segment.Length;
            string error = null;
            uint sequence = 0;

            for (var frag = GetNextUnquotedSearchToken(this.segment); (frag.error == null) && (frag.offset > 0) && (frag.offset <= len || frag.token != null);
                     frag = GetNextUnquotedSearchToken(this.segment, frag.offset))
            {
                if (frag.error != null)
                {
                    error = frag.error;
                    break;
                }
                if (frag.token != null)
                {
                    ++sequence;
                    var current = new SearchFragment(this, frag.token, sequence);
                    this.searchFragments.Add(sequence, current);
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
        private bool ParseQuotedSearch()
        {
            this.searchFragments = new Dictionary<UInt64, SearchFragment>();

            if (!(this.segment.StartsWith('"'.ToString()) && this.segment.EndsWith('"'.ToString())))
            {
                return false;
            }
            if (this.segment.Length == 2)
            {
                statement.Notify("warning", "An empty quotation was provided; segment will be ignored");
                return false;
            }
            if (this.segment.Length < 2)
            {
                statement.Notify("error", "An-matched quote was provided");
                statement.Notify("error", "Segment processing has been aborted.");
                return false;
            }
            this.segment = this.segment.Substring(1, this.segment.Length - 2);

            int len = this.segment.Length;
            string error = null;
            uint sequence = 1;

            for (var frag = GetNextQuotedSearchToken(this.segment); (frag.error == null) && (frag.offset > 0) && (frag.offset <= len || frag.token != null);
                     frag = GetNextQuotedSearchToken(this.segment, frag.offset, frag.bracketed))
            {
                if (frag.error != null)
                {
                    error = frag.error;
                    break;
                }
                if (frag.token != null)
                {
                    uint order = frag.ordered ? sequence : 0;
                    sequence++;
                    var current = new SearchFragment(this, frag.token, order);
                    this.searchFragments.Add(sequence, current);
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
        public static string Help(string topic)
        {
            return "";
        }
    }
}

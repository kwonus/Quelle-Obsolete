using System;
using System.Collections.Generic;
using System.Text;
using static ClarityHMI.HMIStatement;
using System.Linq;

namespace ClarityHMI
{
    public class HMISegment
    {
        public static string[] DirectiveVerbs(string directive)
        {
            if (directive == null)
                return null;

            var upper = directive.Trim().ToUpper();
            string[] verbs = Directives.ContainsKey(upper) ? Directives[upper] : null;
            return verbs;
        }
        public static string DirectiveIncludesVerb(string directive, string verb)
        {
            if (directive != null && verb != null)
            {
                var upper = directive.Trim().ToUpper();
                string[] verbs = Directives.ContainsKey(upper) ? Directives[upper] : null;
                if (verbs != null)
                    foreach (var candidate in verbs)
                        if (verb.Equals(candidate, StringComparison.InvariantCultureIgnoreCase))
                            return candidate;
            }
            return null;
        }
        public static string DirectiveDefault(string directive)
        {
            if (directive == null)
                return null;

            var upper = directive.Trim().ToUpper();
            string[] verbs = Directives.ContainsKey(upper) ? Directives[upper] : null;
            return verbs != null && verbs.Length >= 1 ? verbs[0] : null;
        }
        public const string SEARCH = "SEARCH";
        public const string DISPLAY = "DISPLAY";    // Formerly FILE
        public const string CONTROL = "CONTROL";    // Formerly PERSISTENCE
        public const string STATUS = "STATUS";
        public const string REMOVAL = "REMOVAL";

        private static Dictionary<string, string[]> Directives = new Dictionary<string, string[]>() {
            {SEARCH,    new string[] {"find"  } },                              // When using default segment identification, the first entry ("find") is always the implied result
            {DISPLAY,   new string[] {"print" } },
            {CONTROL,   new string[] {"set", "#set", "@set" } },               // When using default segment identification, the first entry ("set") is always the implied result
            {STATUS,    new string[] {"get", "#get", "@get", "expand", "#expand", "@expand" } },               //  (config/show/remove is for registry-like program settings)
            {REMOVAL,   new string[] {"clear", "#clear", "@clear", "remove", "#remove", "@remove"} } //  (config/show/remove is for registry-like program settings)
        };
        public static string[] Searches => Directives[SEARCH];
        public static string[] Statuses => Directives[STATUS];
        public static string[] Removals => Directives[REMOVAL];
        public static string[] Displays => Directives[DISPLAY];
        public static string[] Controls => Directives[CONTROL];

        public static string FIND => Searches[0];
        public static string GET => Statuses[0];
        public static string EXPAND => Statuses[3];
        public static string CLEAR => Removals[0];
        public static string REMOVE => Removals[3];
        public static string PRINT => Displays[0];
        public static string SET => Controls[0];

        SettingOperation? IsConfig(string verb)
        {
            if (verb == null)
                return null;
            var test = verb.Trim().ToLower();
            if (test.Length == 0)
                return null;

            foreach (var op in (from candidate in Controls where candidate == test select HMIStatement.SettingOperation.Write))
                return op;
            foreach (var op in (from candidate in Statuses where candidate == test select HMIStatement.SettingOperation.Read))
                return op;
            foreach (var op in (from candidate in Removals where candidate == test select HMIStatement.SettingOperation.Remove))
                return op;

            return null;
        }
        public static string[] IsPersistencePattern(string text)
        {
            if (text == null)
                return null;

            string[] parts;
            string[] tokens = text.Split(Whitespace, 2, StringSplitOptions.RemoveEmptyEntries);
            string verb = null;
            bool failed = false;
UseDefaultVerb:
            if (tokens.Length == 2)
            {
                verb = tokens[0];
                verb = DirectiveIncludesVerb(CONTROL, verb);

                if (verb != null)
                {
                    string remainder = tokens[1];
                    parts = remainder.Split(EqualSign, 2, StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length == 2)
                    {
                        tokens = new string[3];
                        tokens[0] = verb;
                        tokens[1] = parts[0].Trim();
                        tokens[2] = parts[1].Trim();

                        if (parts[0].Length == 0 || parts[1].Length == 0)
                            return null;

                        var variable = tokens[1];
                        foreach (char c in variable.ToCharArray())
                        {
                            if (char.IsWhiteSpace(c))
                                return null;
                            if (char.IsLetterOrDigit(c) || (c == '_') || (c == '-') || (c == '~') || (c == '.') || (c == '/') || (c == '\\'))
                                continue;
                            return null;
                        }
                    }
                    return tokens;
                }
            }
            if (!failed)
            {
                failed = true;
                parts = text.Split(EqualSign, 2, StringSplitOptions.None);

                if (parts.Length == 2)
                {
                    tokens = new string[] { SET, text };
                    goto UseDefaultVerb;
                }
            }
            return null;
        }
        public static (string verb, string directive) IsVerb(string verb)
        {
            if (verb == null)
                return (null, null);

            foreach (var directive in Directives.Keys)
            {
                var verbs = Directives[directive];
                foreach (var candidate in verbs)
                    if (verb.Equals(candidate, StringComparison.InvariantCultureIgnoreCase))
                        return (candidate, directive);
            }
            return (null, null);
        }
        public static string[] HasVerb(string text)
        {
            if (text == null)
                return null;

            string[] tokens = text.Split(Whitespace, 2, StringSplitOptions.RemoveEmptyEntries);
            if (tokens.Length == 2)
            {
                tokens[0] = IsVerb(tokens[0]).verb;
                if (tokens[0] != null)
                    return tokens;
            }
            return null;
        }
        public static string[] IsSearchPattern(string text)
        {
            if (text == null)
                return null;

            var persistence = IsPersistencePattern(text);
            if (persistence != null)
                return null;

            string[] tokens = HasVerb(text);

            if (tokens == null)
                tokens = new string[] { DirectiveDefault(SEARCH), text };

            if (tokens.Length == 2)
            {
                if (tokens[0].Length == 0 || tokens[1].Length == 0)
                    return null;

                int countQuotes = 0;
                var search = tokens[1];
                for (int i = 0; i < search.Length; i++)
                {
                    if (search[i] == '"')
                    {
                        if (++countQuotes == 1 && i != 0)
                            return null;
                        if (countQuotes == 2 && i != search.Length - 1)
                            return null;
                    }
                }
                if (countQuotes > 0 && countQuotes != 2)
                    return null;
            }
            return tokens;
        }
        public static string[] NormalizeSegment(string text)
        {
            if (text == null)
                return null;

            //  Only SEARCH and PERSISTENCE segments can be implicitly recognized
            //
            var tokens = HasVerb(text);
            if (tokens != null && tokens.Length >= 2)
            {
                string[] config = IsPersistencePattern(text);
                if (config != null)
                    return config;

                string[] search = IsSearchPattern(text);
                if (search != null)
                    return search;

                return tokens;
            }

            // Persistence pattern is the most identifying pattern, because it MUST contain =
            //
            var parts = IsPersistencePattern(text);
            if (parts != null)
                return parts;

            //  Since we test persistence patter first, we can serch for "set" as the first word of a segment
            //
            parts = IsSearchPattern(text);
            if (parts != null)
                return parts;

            //  No other segments can be implicitly recognized
            //
            return null;
        }
        private bool error = false;
        private HMIStatement statement;
        public string verb;

        public void Notify(string mode, string message)
        {
            if (this.statement != null)
                this.statement.Notify(mode, message);

            if (!this.error)
                this.error = mode == null || !mode.Equals("warning", StringComparison.InvariantCultureIgnoreCase);
        }
        UInt16 span;
        public UInt32 order { get; private set;  }
        public UInt32 sequence { get; private set; }  // Sequence number of segment
        public string segment { get; private set; }
        public string[] rawFragments { get; private set; }
        private char polarity;
        private Boolean quoted;

        public Dictionary<UInt64, HMIFragment> fragments { get; private set; }
        public readonly static string[] Whitespace = new string[] { " ", "\t" };
        public readonly static string[] EqualSign = new string[] { "=" };

        private void ProcessPreparsedFragments(string[] preparsed, uint skip = 0)
        {
            UInt32 order = 1;
            this.rawFragments = skip == 0 ? preparsed : new string[preparsed.Length - skip];
            if (skip > 0)
                for (int i = 0; i < this.rawFragments.Length; i++)
                    this.rawFragments[i] = preparsed[skip+i].Trim();

            foreach (string frag in this.rawFragments)
            {
                HMIFragment current = new HMIFragment(this, 0, order, frag);
                this.fragments.Add(order, current);
                if (this.error)
                    break;

                order++;
            }
        }
        private void ParseUnquoted()
        {
            UInt32 order = 1;
            this.rawFragments = this.segment.Split(Whitespace, StringSplitOptions.RemoveEmptyEntries);
            foreach (string frag in this.rawFragments)
            {
                HMIFragment current = new HMIFragment(this, 0, order, frag);
                this.fragments.Add(order, current);
                if (this.error)
                    break;

                order++;
            }
        }
        private void ParseQuoted()
        {
            if (!(this.segment.StartsWith('"') && this.segment.EndsWith('"')))
            {
                statement.Notify("error", "A quoted string must begin and end with double-quotes:");
                statement.Notify("error", "Segment processing has been aborted.");
                return;
            }
            if (this.segment.Length == 2)
            {
                statement.Notify("warning", "An empty quotation was provided; segment will be ignored");
             }
            if (this.segment.Length < 2)
            {
                statement.Notify("error", "An-matched quote was provided");
                statement.Notify("error", "Segment processing has been aborted.");
            }
            this.segment = this.segment.Substring(1, this.segment.Length-2);
            this.segment = HMISegment.UnspaceParenthetical(this.segment);

            List<string> listFragments = new List<string>();

            var openBrace = false;
            string prefix = "";
            string fragment = "";
            for (int i = 0; i < this.segment.Length; i++)
            {
                char c = this.segment[i];

                if (c == ']')
                {
                    if (openBrace)
                    {
                        openBrace = false;
                        listFragments.Add(prefix + fragment.Trim() + c.ToString());
                        fragment = "";
                        prefix = "";
                        continue;
                    }
                    statement.Notify("error", "Mismatched square braces provided:");
                    statement.Notify("error", "Fragment processing has been aborted.");
                    return;
                }
                if (c == '[')
                {
                    openBrace = true;
                    prefix = c.ToString();
                    continue;
                }
                if (c == '.' && (i < this.segment.Length-2) && (this.segment[i+1] == '.') && (this.segment[i+2] == '.'))
                {
                    prefix = "...";
                    i += 2;
                    continue;
                }
                if (char.IsWhiteSpace(c))
                {
                    if (fragment.Length > 0)
                    {
                        var respaced = HMISegment.RespaceParenthetical(prefix + fragment.Trim());
                        listFragments.Add(respaced);
                        fragment = "";
                        prefix = "";
                    }
                }
                fragment += c;
            }
            if (fragment.Length > 0)
            {
                if (prefix.Contains('['))
                {
                    statement.Notify("error", "Mismatched square braces provided:");
                    statement.Notify("error", "Fragment processing has been aborted.");
                    return;
                }
                if (prefix.Contains("..."))
                {
                    statement.Notify("warning", "elipses at the end of a quoted string are ignored");
                }
                var respaced = HMISegment.RespaceParenthetical(prefix + fragment.Trim());
                listFragments.Add(respaced);
            }
            UInt32 order = 1;
            this.rawFragments = listFragments.ToArray();
            foreach (string frag in this.rawFragments)
            {
                HMIFragment current = new HMIFragment(this, order, order, frag);
                this.fragments.Add(order, current);

                order++;
            }
        }
        public HMISegment(HMIStatement statement, UInt32 segmentOrder, UInt16 span, char polarity, string segment)
        {
            string[] normalized = NormalizeSegment(segment);

            if (normalized == null || normalized.Length < 2)
            {
                statement.Notify("error", "Unable to parse statement.");
                statement.Notify("error", "Segment processing has been aborted.");
                return;
            }
            this.statement = statement;
            this.verb = normalized[0];

            this.fragments = new Dictionary<UInt64, HMIFragment>();
            this.sequence = segmentOrder;
 
            this.span = span;
            this.rawFragments = null;
            this.segment = normalized[1] != null ? normalized[1] : "";
            this.polarity = polarity != '-' ? '+' : '-';

            if (DirectiveIncludesVerb(SEARCH, this.verb) != null)
            {
                this.quoted = (DirectiveIncludesVerb(SEARCH, this.verb) != null) && (this.segment.Length > 2) && this.segment.StartsWith('"') && this.segment.EndsWith('"');
                if (this.quoted)
                    this.ParseQuoted();
                else
                    this.ParseUnquoted();
            }
            else
            {
                this.ProcessPreparsedFragments(normalized, 1);
            }
        }
        public static string UnspaceParenthetical(string text)
        {
            if (text == null)
                return null;
            int p = text.IndexOf('(');
            if (p >= 0 || text.IndexOf("...") >= 0)
            {
                bool paren = false;
                bool elipses = false;

                StringBuilder builder = new StringBuilder("", text.Length)
                    .Replace("[", "[ ")
                    .Replace("(", "( ")
                    .Replace("]", " ] ")
                    .Replace(")", " ) ")
                    .Replace("...", " ...");

                for (int i = 0; i < text.Length; i++)
                    if (text[i] == '(')
                    {
                        elipses = false;
                        paren = true;
                        builder.Append(text[i]);
                    }
                    else if (text[i] == ')')
                    {
                        elipses = false;
                        paren = false;
                        builder.Append(text[i]);
                    }
                    else if ((text[i] == '.') && text.Substring(i).StartsWith("..."))
                    {
                        elipses = true;
                        builder.Append(text[i++]);
                        builder.Append(text[i++]);
                        builder.Append(text[i]);
                    }
                    else if (char.IsWhiteSpace(text[i]))
                    {
                        builder.Append(paren || elipses ? '@' : text[i]);
                    }
                    else
                    {
                        elipses = false;
                        builder.Append(text[i]);
                    }
                return builder.ToString();
            }
            return text;
        }
        public static string RespaceParenthetical(string text)
        {
            if (text == null)
                return null;
            int a = text.IndexOf('@');
            if (a >= 0)
            {
                bool paren = false;
                bool elipses = false;

                StringBuilder builder = new StringBuilder("", text.Length);
                for (int i = 0; i < text.Length; i++)
                    if (text[i] == '(')
                    {
                        elipses = false;
                        paren = true;
                        builder.Append(text[i]);
                    }
                    else if (text[i] == ')')
                    {
                        elipses = false;
                        paren = false;
                        builder.Append(text[i]);
                    }
                    else if ((text[i] == '.') && text.Substring(i).StartsWith("..."))
                    {
                        elipses = true;
                        builder.Append(text[i++]);
                        builder.Append(text[i++]);
                        builder.Append(text[i]);
                    }
                    else if (text[i] == '@')
                    {
                        builder.Append(paren || elipses ? ' ' : text[i]);
                    }
                    else
                    {
                        elipses = false;
                        builder.Append(text[i]);
                    }
                int original = builder.Length + 1;
                while(builder.Length < original)
                {
                    original = builder.Length;

                    builder.Replace("( ", "(");
                    builder.Replace("[ ", "[");
                    builder.Replace(" )", ")");
                    builder.Replace(" ]", "]");
                    builder.Replace("  ", " ");
                }
                return builder.ToString();
            }
            return text;
        }
    }
}

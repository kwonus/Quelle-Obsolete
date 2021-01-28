using System;
using System.Collections.Generic;
using System.Text;
using static QuelleHMI.HMIStatement;
using System.Linq;

namespace QuelleHMI
{
    static class HMIClauseType_Methods
    {
        public static bool IsDependent(this HMIClause.HMIClauseType type)
        {
            if (((uint)type & (uint)HMIClause.HMIClauseType.DEPENDENT) != 0)
                return type != HMIClause.HMIClauseType.UNDEFINED;
            return false;
        }
        public static bool IsSimple(this HMIClause.HMIClauseType type)
        {
            if (((uint)type & (uint)HMIClause.HMIClauseType.SIMPLE) != 0)
                return type != HMIClause.HMIClauseType.UNDEFINED;
            return false;
        }
        public static bool Isordinary(this HMIClause.HMIClauseType type)
        {
            return type == HMIClause.HMIClauseType.ORDINARY;
        }
    }
    public class HMIClause
    {
        public enum HMIPolarity
        {
            NEGATIVE = (-1),
            UNDEFINED = 0,
            POSITIVE = 1
        }
        public enum HMIClauseType
        {
            UNDEFINED = 0xF,
            ORDINARY = 0,
            SIMPLE = 1,
            DEPENDENT = 2,
            SIMPLE_OR_DEPENDENT = 3
        }

        public static string[] DirectiveVerbs(string directive)
        {
            if (directive == null)
                return null;

            var upper = directive.Trim().ToUpper();
            string[] verbs = IndependentClauses.ContainsKey(upper) ? IndependentClauses[upper] : null;
            return verbs;
        }
        public static string DirectiveIncludesVerb(string directive, string verb)
        {
            if (directive != null && verb != null)
            {
                var upper = directive.Trim().ToUpper();
                string[] verbs = IndependentClauses.ContainsKey(upper) ? IndependentClauses[upper] : null;
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
            string[] verbs = IndependentClauses.ContainsKey(upper) ? IndependentClauses[upper] : null;
            return verbs != null && verbs.Length >= 1 ? verbs[0] : null;
        }
        //  These do not exactly match the granularity of the documentation, because they are organized by syntax rules
        //
        public const string SEARCH  = "SEARCH";
        public const string DISPLAY = "DISPLAY";
        public const string SETTERS = "SETTERS";    // CONTROL
        public const string GETTERS = "GETTERS";    // CONTROL, MACROS
        public const string REMOVAL = "REMOVAL";    // CONTROL, MACROS
        public const string PROGRAM = "SYSTEM";
        public const string MACRODEF= "MACRODEF";   // MACROS

        //  Inferred verbs are not prefixed. Explcit verbs are prefixed with @ to make parsing the DSL unambiguous.
        private static Dictionary<string, string[]> IndependentClauses = new Dictionary<string, string[]>() {
            {SEARCH,    new string[] {"find"   }},  // When using default segment identification, the first entry ("search") is always the implied result
            {SETTERS,   new string[] {"set"    }},  // When using default segment identification, the first entry ("set") is always the implied result
            {GETTERS,   new string[] {"@show"  }},  // registry-like program settings, or macros
            {REMOVAL,   new string[] {"@clear" }}   // registry-like program settings, or macros
        };
        private static Dictionary<string, string[]> SimpleClauses = new Dictionary<string, string[]>() {
            {DISPLAY,   new string[] { "@print" } },
            {PROGRAM,   new string[] { HELP, BACKUP, RESTORE, EXIT } }
        };
        private static Dictionary<string, string[]> DependentClauses = new Dictionary<string, string[]>() {
            {DISPLAY,   new string[] {"@print" } },
            {MACRODEF,  new string[] {"@define" } }
        };
        //  Independent/Ordinary Clauses
        public static string[] SearchVerbs => IndependentClauses[SEARCH];
        public static string[] RemovalVerbs => IndependentClauses[REMOVAL];
        public static string[] SetterVerbs => IndependentClauses[SETTERS];
        public static string[] GetterVerbs => IndependentClauses[GETTERS];

        public static string FIND => SearchVerbs[0];
        public static string SET => SetterVerbs[0];

        public static string GET => GetterVerbs[0];

        public static string CLEAR => RemovalVerbs[0];
        public static string REMOVE => RemovalVerbs[1];

        //  Dependent Clause Verbs
        public static string[] DependentDisplayVerbs => DependentClauses[DISPLAY];
        public static string[] DependentMacroVerbs => DependentClauses[MACRODEF];

        public static string PRINT_SUB => DependentDisplayVerbs[0];
        public static string DEFINE => DependentMacroVerbs[0];

        //  Simple Clause Verbs
        public static string[] ProgramVerbs => SimpleClauses[PROGRAM];

        public const string HELP    = "@help";
        public const string BACKUP  = "@backup";
        public const string RESTORE = "@restore";
        public const string EXIT    = "@exit";

        public static string[] SimpleDisplayVerbs => SimpleClauses[DISPLAY];
        public static string PRINT_SIMPLE  = SimpleDisplayVerbs[0];

        public string misplaced { get; protected set; }

        SettingOperation? IsConfig(string verb)
        {
            if (verb == null)
                return null;
            var test = verb.Trim().ToLower();
            if (test.Length == 0)
                return null;

            foreach (var op in (from candidate in SetterVerbs where candidate == test select HMIStatement.SettingOperation.Write))
                return op;
            foreach (var op in (from candidate in GetterVerbs where candidate == test select HMIStatement.SettingOperation.Read))
                return op;
            foreach (var op in (from candidate in RemovalVerbs where candidate == test select HMIStatement.SettingOperation.Remove))
                return op;

            return null;
        }
        //  Assume that an explicit verb has not been passed
        //  (We will not be looking for explicit verbs here)
        //
        public (string[] tokens, string error) IsPersistencePattern(string text)
        {
            if (text == null)
                return (null, "Driver design error; cannot test patter when input is null");

            var parts = HMIClause.SmartSplit(text, '=');
            if (parts.Length == 2)
            {
                this.maximumScope = HMIScope.System;
                var tokens = new string[3];
                tokens[0] = SET;
                tokens[1] = parts[0];
                tokens[2] = parts[1];

                if (parts[0].Length == 0 || parts[1].Length == 0)
                    return (tokens, "User input error; Control assignments require a name and a value");

                var variable = tokens[1];
                if (!HMISession.IsControl(variable))
                    return (tokens, "User input error; The syntax of the phrase matches a control assigment, but the control name could not be found.");

                return (tokens, null);
            }
            return (null, null);
        }
        public static (HMIClauseType type, string verb, string directive) IsVerb(string verb)
        {
            if (verb == null)
                return (HMIClauseType.UNDEFINED, null, null);

            foreach (var directive in IndependentClauses.Keys)
            {
                var verbs = IndependentClauses[directive];
                foreach (var candidate in verbs)
                    if (verb.Equals(candidate, StringComparison.InvariantCultureIgnoreCase))
                        return (HMIClauseType.ORDINARY, candidate, directive);
            }
            foreach (var directive in DependentClauses.Keys)
            {
                var verbs = DependentClauses[directive];
                foreach (var candidate in verbs)
                    if (verb.Equals(candidate, StringComparison.InvariantCultureIgnoreCase))
                        return (HMIClauseType.DEPENDENT, candidate, directive);
            }
            foreach (var directive in SimpleClauses.Keys)
            {
                var verbs = SimpleClauses[directive];
                foreach (var candidate in verbs)
                    if (verb.Equals(candidate, StringComparison.InvariantCultureIgnoreCase))
                        return (HMIClauseType.SIMPLE, candidate, directive);
            }
            return (HMIClauseType.UNDEFINED, null, null);
        }
        protected (bool yes, string[] tokens) HasVerb(string text)
        {
            if (text == null)
                return (false, null);

            string[] tokens = text.Split(Whitespace, 2, StringSplitOptions.RemoveEmptyEntries);
            var info = IsVerb(tokens[0]);
            if (info.verb != null)
            {
                this.verb = info.verb;
                tokens[0] = info.verb;

                switch (info.directive)
                {
                    case HMIClause.MACRODEF:
                    case HMIClause.GETTERS:
                    case HMIClause.SETTERS:
                    case HMIClause.REMOVAL: this.maximumScope = HMIScope.System; break;

                    case HMIClause.SEARCH:
                    case HMIClause.DISPLAY: this.maximumScope = HMIScope.Statement; break;

                    case HMIClause.PROGRAM: this.maximumScope = HMIScope.Undefined; break;
                }
                return (true, tokens);
            }
            return (false, null);
        }
        protected string[] NormalizeClause(string text)
        {
            if (text == null)
                return null;

            //  Look first for explicit verb references:
            //
            var tokens = this.HasVerb(text);
            if (tokens.yes)
                return tokens.tokens;

            //  Only CONTROL::SET can be implicitly recognized
            //
            var controls = IsPersistencePattern(text);
            if (controls.tokens != null)
            {
                if (controls.error != null)
                    this.Notify("error", controls.error);

                return controls.tokens;
            }
            //  No other segments can be implicitly recognized, it defaults to SEARCH
            //
            this.maximumScope = HMIScope.Statement;
            return (new string[] { FIND, text });
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
        public UInt32 order { get; private set;  }
        public UInt32 sequence { get; private set; }  // Sequence number of segment
        public string segment { get; private set; }
        public string[] rawFragments { get; private set; }
        public HMIPolarity polarity { get; private set; }
        public HMIScope maximumScope { get; private set; }
        private Boolean quoted;

        public Dictionary<UInt64, HMIFragment> fragments { get; private set; }
        public readonly static string[] Whitespace = new string[] { " ", "\t" };

        //  These are not search fragments so they bypass the strict SEARCG fragment tokenization rules
        //
        private void ProcessPreparsedFragments(string[] preparsed, uint skip = 0)
        {
            this.rawFragments = skip == 0 ? preparsed : new string[preparsed.Length - skip];
            if (skip > 0)
                for (int i = 0; i < this.rawFragments.Length; i++)
                    this.rawFragments[i] = preparsed[skip+i].Trim();

            UInt32 order = 1;
            foreach (string frag in this.rawFragments)
            {
                HMIFragment current = new HMIFragment(this, frag, order++, singletonToken:true);
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
                result.offset = len;   // mark end-of-text with (-1)
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
                        case ']': isBrace =  (1); break;
                        case '(': isParen = (-1); break;
                        case ')': isParen =  (1); break;

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
                result.token = text.Substring(offset, result.offset-offset);
            }
            return result;
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
            this.rawFragments = null;

            int len = this.segment.Length;
            string error = null;
            uint sequence = 1;
            (string token, int offset, bool bracketed, bool ordered, string error) fragment;
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
                    HMIFragment current = new HMIFragment(this, frag.token, order, !(frag.token.StartsWith("(") && frag.token.EndsWith(")")));
                    this.fragments.Add(sequence, current);
                }
                if (frag.offset >= len)
                    break;
            }
            if (error != null)
            {
                this.Notify("error", error);
                return;
            }
        }
        public HMIClause(HMIStatement statement, UInt32 segmentOrder, HMIPolarity polarity, string segment, HMIClauseType clauseType)
        {
            this.misplaced = null;
            this.maximumScope = HMIScope.Undefined;
            this.statement = statement;

            string[] normalized = NormalizeClause(segment);

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
 
            this.rawFragments = null;
            this.segment = normalized[1] != null ? normalized[1] : "";
            this.polarity = polarity;

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
        public static string[] SmartSplit(string source, char delimit)
        {
            if (source == null || string.IsNullOrWhiteSpace(source))
                return null;

            int len = source.Length;

            var splits = new List<string>();

            var c_quoted = false;   // character-quoted means that crrent character is quoted with \
            var d_quoted = false;   // double-quoted means that this character is enclosed in double-quotes
            int last = 0;
            int i;
            for (i = 0; /**/; i++)  // looking for "//" or "/-"
            {
                if (i >= len)
                {
                    if (last < len)
                        splits.Add(source.Substring(last, len-last).Trim());
                    else
                        splits.Add("");

                    break;
                }
                if (c_quoted)   // then this character should be ignored as a delimiter and be ignored as per double-quoting
                {
                    c_quoted = false;
                    continue;
                }
                char c = source[i];

                if (d_quoted)   // ignore all characters enclosed in double-quotes for segmentation purposes
                {
                    d_quoted = (c != '\"'); // true only when this is the matching double-quote
                    continue;
                }
                switch (c)
                {
                    case '\\': c_quoted = true; continue;
                    case '"':  d_quoted = true; continue;
                }
                if (i >= len - 1)
                    continue;

                if (c != delimit)
                    continue;

                splits.Add(source.Substring(last, i-last).Trim());

                last = i + 1;
            }
            switch (splits.Count)
            {
                case 0: return new string[0];
                case 1: return new string[] { splits[0] };
                default:return splits.ToArray();
            }
        }
    }
}

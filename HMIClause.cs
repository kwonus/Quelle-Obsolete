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
    public abstract class HMIClause
    {
        abstract protected bool Parse();

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
        //  Assume that an explicit verb has not been passed
        //  (We will not be looking for explicit verbs here)
        //
        public static (string[] tokens, string error) IsPersistencePattern(string text)
        {
            if (text == null)
                return (null, "Driver design error; cannot test patter when input is null");

            var parts = HMIClause.SmartSplit(text, '=');
            if (parts.Length == 2)
            {
                var tokens = new string[3];
                tokens[0] = Verbs.Set.VERB;
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
        public static HMIClause CreateVerbClause(HMIStatement statement, uint order, HMIPolarity polarity, string text)
        {
            if (statement == null)
                return null;

            if (text == null || text.Length < 1)
                return null;

            //  Look first for explicit verb references:
            //
            if (text[0] == '@')
            {
                var tokens = text.Split(Whitespace, StringSplitOptions.RemoveEmptyEntries);
                switch(tokens[0].ToLower())
                {
                    case Verbs.Print.VERB:  return new Verbs.Print(statement, order, text);
                    case Verbs.Clear.VERB:  return new Verbs.Clear(statement, order, text);
                    case Verbs.Define.VERB: return new Verbs.Define(statement, order, text);
                    case Verbs.Show.VERB:   return new Verbs.Show(statement, order, text);
                    case Verbs.Help.VERB:   return new Verbs.Help(statement, text);
                    case Verbs.Backup.VERB: return new Verbs.Backup(statement, text);
                    case Verbs.Restore.VERB:return new Verbs.Restore(statement, text);
                    case Verbs.Exit.VERB:   return new Verbs.Exit(statement, text);
                }
                statement.Notify("error", "Unknown verb provided: " + text[0]);
                return null;
            }
            //  Only CONTROL::SET can be implicitly recognized
            //
            var controls = IsPersistencePattern(text);
            if (controls.tokens != null)
            {
                return new Verbs.Set(statement, order, text);
            }
            //  No other segments can be implicitly recognized, it defaults to SEARCH
            //
            return new Verbs.Search(statement, order, polarity, text);
        }
        protected bool error = false;
        protected HMIStatement statement;
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
        public string segment { get; protected set; }
        public HMIPolarity polarity { get; private set; }
        public HMIScope maximumScope { get; protected set; }

        public Dictionary<UInt64, HMIFragment> fragments { get; private set; }
        public readonly static string[] Whitespace = new string[] { " ", "\t" };

        protected HMIClause(HMIStatement statement, UInt32 segmentOrder, HMIPolarity polarity, string segment, HMIClauseType clauseType)
        {
            this.maximumScope = HMIScope.Undefined;
            this.statement = statement;

            string normalized = segment.Trim();

            if (normalized == null || normalized.Length < 1)
            {
                statement.Notify("error", "Unable to parse statement.");
                statement.Notify("error", "Segment processing has been aborted.");
                return;
            }
            this.segment = normalized;
            this.fragments = new Dictionary<UInt64, HMIFragment>();
            this.sequence = segmentOrder; 
            this.polarity = polarity;

            this.Parse();
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

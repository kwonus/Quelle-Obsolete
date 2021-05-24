using System;
using System.Collections.Generic;
using System.Text;
using static QuelleHMI.HMIStatement;

namespace QuelleHMI.Actions
{
    public abstract class Action
    {
        abstract public bool Parse();
        abstract public bool Execute();
        protected static UInt32 currentSequence;


        protected List<string> errors { get => this.statement.command.errors; }
        protected List<string> warnings { get => this.statement.command.warnings; }
        protected HMIClauseType type;
        public bool isImplicit()
        {
            return type == HMIClauseType.IMPLICIT;
        }
        public bool isExplicit()
        {
            return type == HMIClauseType.EXPLICIT;
        }
        public bool isDefined()
        {
            return type != HMIClauseType.UNDEFINED;
        }
        public abstract string syntax { get; }

        protected (bool ok, string[] errors, string[] warnings) Status
        {
            get
            {
                return (this.errors.Count == 0, this.errors.Count == 0 ? this.errors.ToArray() : null, this.warnings.Count == 0 ? this.warnings.ToArray() : null);
            }
        }

        public enum HMIPolarity
        {
            NEGATIVE = (-1),
            UNDEFINED = 0,
            POSITIVE = 1
        }
        public enum HMIClauseType
        {
            UNDEFINED = -1,
            IMPLICIT = 0,
            EXPLICIT = 1
        }
 
        public static Action CreateAction(HMIStatement statement, uint order, HMIPolarity polarity, string text)
        {
            if (statement == null)
                return null;

            if (text == null || text.Length < 1)
                return null;

            Action.currentSequence = 0;

            //  Look first for explicit verb references:
            //
            if (text[0] == '@')
            {
                var tokens = text.Split(Whitespace, StringSplitOptions.RemoveEmptyEntries);
                var verb = tokens[0].ToLower();

                // Look for explicit verbs
                if (Display.EXPLICIT.Contains(verb))
                    return new Display(statement, text);
                if (Label.EXPLICIT.Contains(verb))
                    return new Label(statement, text);
                if (Control_Get.EXPLICIT.Contains(verb))
                    return new Control_Get(statement, text);
                if (System.EXPLICIT.Contains(verb))
                    return new System(statement, text);
                if (History.EXPLICIT.Contains(verb))
                    return new History(statement, text);

                statement.Notify("error", "Unknown verb provided: " + verb);
                return null;
            }
            //  Only CONTROL::SET/CONTROL::CLEAR can be implicitly recognized
            //
            var controlAction = Control.GetAction(statement, text, order);
            if (controlAction.error != null)
            {
                statement.Notify("error", controlAction.error);
                return null;
            }
            else if (controlAction.action != null)
            {
                return controlAction.action;
            }
            //  No other segments can be implicitly recognized, it defaults to SEARCH
            //
            return new Actions.Search(statement, order, polarity, text);
        }
        protected HMIStatement statement;
        public string verb;

        public void Notify(string mode, string message)
        {
            if (this.statement != null)
                this.statement.Notify(mode, message);
        }
        protected UInt32 sequence { get; private set; }  // Sequence number of segment
        public string segment { get; protected set; }
        public HMIPolarity Polarity { get; private set; }
        public byte polarity { get => (byte) this.Polarity; }


        public readonly static string[] Whitespace = new string[] { " ", "\t" };

        protected Action(HMIStatement statement, HMIClauseType clauseType, UInt32 segmentOrder, string segment, HMIPolarity polarity = HMIPolarity.UNDEFINED)
        {
            this.statement = statement;
            this.type = clauseType;

            string normalized = segment.Trim();

            if (normalized == null || normalized.Length < 1)
            {
                statement.Notify("error", "Unable to parse statement.");
                statement.Notify("error", "Segment processing has been aborted.");
                return;
            }
            this.segment = normalized;
            this.sequence = segmentOrder; 
            this.Polarity = polarity;
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

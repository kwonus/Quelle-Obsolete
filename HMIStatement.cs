using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using static QuelleHMI.HMIClause;
using QuelleHMI.Actions;
using QuelleHMI.Definitions;

namespace QuelleHMI
{
    public class HMIStatement
    {
        public enum SettingOperation
        {
            Read = 0,
            Write = 1,
            Remove = (-1)
        }
        public HMICommand command { get; protected set; }
        public void Notify(string mode, string message)
        {
            if (this.command != null)
                this.command.Notify(mode, message);
        }
        protected List<string> errors { get => this.command.errors; }
        protected List<string> warnings { get => this.command.warnings; }

        public string statement { get; private set; }
        public Dictionary<UInt32, HMIClause> segmentation { get; private set; }

        protected (bool simple, HMIClause explicitClause, HMIClause[] setters, HMIClause[] removals, HMIClause[] searches) normalized;
        public HMIClause explicitClause
        {
            get
            {
                return normalized.explicitClause;
            }
        }

        public static string SquenchAndNormalizeText(string text)
        {
            return System.Text.RegularExpressions.Regex.Replace(text, @"\s+", " ").ToLower();
        }
        public static string SquenchText(string text)
        {
            return System.Text.RegularExpressions.Regex.Replace(text, @"\s+", " ");
        }
        public HMIStatement(HMICommand command, string statement)
        {
            this.normalized = (false, null, null, null, null);
            if (command == null)
                return;
            if (statement == null)
            {
                command.Notify("error", "Cannot pass a null statement for parsing");
                return;
            }
            this.command = command;
            this.statement = statement.Trim();
            if (this.statement.Length < 1)
            {
                command.Notify("error", "Cannot pass a nill or empty statement for parsing");
                return;
            }

            int len = this.statement.Length;

            // create segmentation boundaries, carefully ignoring quoted delimiters (can't use String.Split() ... similar to HMIPhrase.SmartSplit()
            //
            var c_quoted = false;   // character-quoted means that crrent character is quoted with \
            var d_quoted = false;   // double-quoted means that this character is enclosed in double-quotes
            var negatives = new Dictionary<UInt32, string>(); // order, rawSegment
            var positives = new Dictionary<UInt32, string>(); // order, rawSegment
            var polarity = positives;
            int last = 0;
            UInt32 order = 1;
            int i;
            for (i = 0; /**/; i++)  // looking for ";" or "--" or "@print" or "@define"
            {
                if (i >= len)
                {
                    if (last < len)
                        polarity.Add(order++, this.statement.Substring(last, len - last).Trim());
                    break;
                }
                if (c_quoted)   // then this character should be ignored as a delimiter and be ignored as per double-quoting
                {
                    c_quoted = false;
                    continue;
                }
                char c = this.statement[i];

                if (d_quoted)   // ignore all characters enclosed in double-quotes for segmentation purposes
                {
                    d_quoted = (c != '\"'); // true only when this is the matching double-quote
                    continue;
                }
                switch (c)
                {
                    case '\\': c_quoted = true; continue;
                    case '"': d_quoted = true; continue;
                }
                if (c == ';')   // extra semi-colons are allowed ... ignore extraneous ones here
                {
                    var segment = this.statement.Substring(last, i - last).Trim();
                    if (segment.Length > 0)
                    {
                        polarity.Add(order++, segment);
                    }
                    polarity = positives;
                    last = i + 1;
                }
                else if (c == '!')  // ::clear! syntax counts as a segment delimiter
                {
                    polarity.Add(order++, this.statement.Substring(last, 1 + i - last).Trim());
                    polarity = positives;
                    last = i + 1;
                }
                else if (c == '@')
                {
                    positives.Add(order++, this.statement.Substring(last));
                    break;
                }
                else
                {
                    if (c != '-' || i >= len - 1)
                        continue;

                    char next = this.statement[i + 1];

                    if (last < i)
                    {
                        if (next == '-')
                        {
                            polarity.Add(order++, this.statement.Substring(last, i - last).Trim());
                            polarity = negatives;
                            last = i + 2;
                            i++;
                        }
                    }
                }
            }

            this.segmentation = new Dictionary<UInt32, HMIClause>();
            i = 0;
            // Eliminate duplicate segments (NOTE [-] polarity has precedence over [+] polarity)
            var inventory = new List<string>();
            foreach (var parsed in negatives)
            {
                if (command.errors.Count > 0)
                    break;

                if (parsed.Value.Length > 0)
                {
                    var squenched = SquenchAndNormalizeText(parsed.Value);
                    if (!inventory.Contains(squenched))
                    {
                        var current = HMIClause.CreateVerbClause(this, parsed.Key, HMIPolarity.NEGATIVE, parsed.Value);
                        this.segmentation.Add(parsed.Key, current);
                        i++;
                    }
                }
            }
            foreach (var parsed in positives)
            {
                if (command.errors.Count > 0)
                    break;

                if (parsed.Value.Length > 0)
                {
                    var squenched = SquenchAndNormalizeText(parsed.Value);
                    if (!inventory.Contains(squenched))
                    {
                        var current = HMIClause.CreateVerbClause(this, parsed.Key, HMIPolarity.POSITIVE, parsed.Value);
                        this.segmentation.Add(parsed.Key, current);
                        i++;
                    }
                }
            }
        }

        private void Append(ref HMIClause[] array, HMIClause item)
        {
            if (array == null)
                array = new HMIClause[] { item };
            else
            {
                HMIClause[] updated = new HMIClause[array.Length + 1];
                for (int i = 0; i < array.Length; i++)
                    updated[i] = array[i];
                updated[array.Length] = item;

                array = updated;
            }
        }
        public bool Normalize()
        {
            //           (bool singleton, HMIClause explicitClause, HMIClause[] setters, HMIClause[] removals, HMIClause[] searches)
            this.normalized = (false, null, null, null, null);

            if (this.statement == null || this.segmentation == null)
            {
                this.Notify("error", "Could not normalize statement: driver design error");
                return false;
            }
            int position = 1;
            int last = this.segmentation.Count;
            var clauses = (from key in this.segmentation.Keys orderby key select this.segmentation[key]);
            foreach (var clause in clauses)
            {
                this.normalized.simple = this.normalized.simple || clause.isExplicit();

                if (!clause.isDefined())
                    this.Notify("error", "The type of clause could not be identified");

                if (!clause.isImplicit())
                    this.normalized.explicitClause = clause;

                else if (clause.verb == Actions.Search.FIND)
                    Append(ref this.normalized.searches, clause);

                else if (clause.verb == Actions.Control.CLEAR)
                    Append(ref this.normalized.removals, clause);

                else if (clause.verb == Actions.Control.SET)
                    Append(ref this.normalized.setters, clause);

                else
                    this.Notify("error", "Could not normalize statement: unexpected clause type encountered.");

                if (this.normalized.simple == true && position > 1)
                    this.Notify("error", "More than one clause was combined with a verb that must be a simple statement");
            }
            return this.errors.Count == 0;
        }
        public bool Execute()
        {
            bool ok = (command.errors.Count == 0);

            if (ok)
            {
                if (!this.Normalize())
                    return false;

                if (command.HasMacro())
                {
                    // TODO: some redundancy is going on here
                    var macroDef = command.GetMacroDefinition();
                    var result = QuelleMacro.Create(macroDef.macroName, command.statement.statement);
                    if (result == null)
                    {
                        this.Notify("error", "Could not create macro");
                        return false;
                    }
                    return true;
                }
                if (normalized.simple)
                {
                    ok = normalized.explicitClause.Execute();
                }
                else
                {
                    if (normalized.setters != null)
                        foreach (var clause in normalized.setters)
                        {
                            ok = clause.Execute();
                            if (!ok)
                                break;
                        }
                    if (ok && (normalized.removals != null))
                        foreach (var clause in normalized.removals)
                        {
                            ok = clause.Execute();
                            if (!ok)
                                break;
                        }
                    if (ok && (normalized.searches != null))
                    {
                        foreach (var clause in normalized.searches)
                        {
                            ok = clause.Execute();
                            if (!ok)
                                break;
                        }
                        ok = ok && this.command.Search();
                    }
                    if (ok)
                    {
                        if (normalized.explicitClause != null)  // DO WE STILL NEED THIS?  OR CAN WE JUST DO DEFINE/PRINT LAST?
                        {
                            ok = normalized.explicitClause.Execute();
                        }
                        else if (normalized.searches != null)
                        {
                            ;   // default search summarization out goes here
                        }
                    }
                }
            }
            return ok;
        }
    }
}

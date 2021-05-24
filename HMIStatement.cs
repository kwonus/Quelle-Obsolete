using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using QuelleHMI.Actions;
using QuelleHMI.Definitions;

namespace QuelleHMI
{
    public class HMIStatement
    {
        public HMICommand command { get; protected set; }
        public void Notify(string mode, string message)
        {
            if (this.command != null)
                this.command.Notify(mode, message);
        }
        protected List<string> errors { get => this.command.errors; }
        protected List<string> warnings { get => this.command.warnings; }

        public string statement { get; private set; }
        public Dictionary<UInt32, Actions.Action> segmentation { get; private set; }

        protected (bool simple, Actions.Action explicitAction, Actions.Action[] setters, Actions.Action[] removals, Actions.Action[] searches) normalized;

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
                else if (c == '-' && i < len-1) // look for --
                {
                    char next = this.statement[i+1];
                    if (next == '-' && last < i)
                    {
                        polarity.Add(order++, this.statement.Substring(last, i - last).Trim());
                        polarity = negatives;
                        last = i + 2;
                        i++;
                    }
                }
            }
            this.segmentation = new Dictionary<UInt32, Actions.Action>();
            i = 0;
            // Eliminate duplicate segments (NOTE [-] polarity has precedence over [+] polarity)
            var inventory = new List<string>();
            foreach (var parse in negatives)
            {
                if (parse.Value.Length > 0)
                {
                    var squenched = SquenchAndNormalizeText(parse.Value);
                    if (!inventory.Contains(squenched))
                    {
                        var current = Actions.Action.CreateAction(this, parse.Key, Actions.Action.HMIPolarity.NEGATIVE, parse.Value);
                        if (current.Parse())
                        {
                            this.segmentation.Add(parse.Key, current);
                            i++;
                        }
                        else Notify("error", "Unable to parse: " + squenched);
                    }
                }
            }
            foreach (var parse in positives)
            {
                if (parse.Value.Length > 0)
                {
                    var squenched = SquenchAndNormalizeText(parse.Value);
                    if (!inventory.Contains(squenched))
                    {
                        var current = Actions.Action.CreateAction(this, parse.Key, Actions.Action.HMIPolarity.POSITIVE, parse.Value);
                        if ((current != null) && current.Parse())
                        {
                            this.segmentation.Add(parse.Key, current);
                            i++;
                        }
                        else Notify("error", "Unable to parse: " + squenched);
                    }
                }
            }
        }

        private void Append(ref Actions.Action[] array, Actions.Action item)
        {
            if (array == null)
                array = new Actions.Action[] { item };
            else
            {
                Actions.Action[] updated = new Actions.Action[array.Length + 1];
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
                {
                    if (this.normalized.explicitAction == null)
                        this.normalized.explicitAction = clause;
                    else
                        this.Notify("error", "Cannot have more than one explicit action. Both " + this.normalized.explicitAction.verb + " and " + clause.verb + " encountered in the same statement");
                }

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
        public bool Execute(ISearchProvider provider)
        {
            bool ok = (command.errors.Count == 0);

            if (ok)
            {
                if (!this.Normalize())
                    return false;

                if (normalized.simple)
                {
                    ok = normalized.explicitAction.Execute();
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
                        ok = ok && this.command.Search(provider);
                    }
                    if (ok)
                    {
                        if (normalized.explicitAction != null)  // DO WE STILL NEED THIS?  OR CAN WE JUST DO DEFINE/PRINT LAST?
                        {
                            ok = normalized.explicitAction.Execute();
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

using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using static QuelleHMI.HMIClause;

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
        public HMIScope scope { get; private set; }
         public Dictionary<(UInt32 order, HMIPolarity polarity, string segment), HMIClause> segmentation { get; private set; }

        protected (HMIClause singleton, HMIClause suborinate, HMIClause[] setters, HMIClause[] removals, HMIClause[] searches) normalized;

        public HMIStatement(HMICommand command, string statement)
        {
            this.normalized = (null, null, null, null, null);
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
            for (i = 0; /**/; i++)  // looking for "//" or "/-"
            {
                if (i >= len && last < len)
                {
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
                    case '|': break;                          // this should never see this trigger, but add for fail-safety and QC breakpoint during debugging
                }
                if (i >= len - 1)
                    continue;

                if (c != '/')
                    continue;

                char next = this.statement[i + 1];

                if (last < i)
                {
                    switch (next)
                    {
                        case '/':
                        case '-':
                            polarity.Add(order++, this.statement.Substring(last, i - last).Trim());
                            break;
                        default: continue;
                    }
                }
                switch (next)
                {
                    case '/': polarity = positives; break;
                    case '-': polarity = negatives; break;
                }
                last = i + 2;
                i++;
            }
            // Delete redundant [+] and [-] polarities
            var removals = new List<UInt32>();
            foreach (var segment in negatives.Values)
                foreach (var record in positives)
                    if (record.Value.Equals(segment, StringComparison.InvariantCultureIgnoreCase))
                        removals.Add(record.Key);

            foreach (var cancel in removals)
                positives.Remove(cancel);

            this.segmentation = new Dictionary<(UInt32 order, HMIPolarity polarity, string segment), HMIClause>();
            i = 0;
            foreach (var parsed in positives)
            {
                if (command.errors != null)
                    break;

                if (parsed.Value.Length > 0)
                {
                    var current = HMIClause.CreateVerbClause(this, parsed.Key, HMIPolarity.POSITIVE, parsed.Value);
                    var tuple = (parsed.Key, HMIPolarity.POSITIVE, parsed.Value);
                    this.segmentation.Add(tuple, current);
                    i++;
                }
            }
            foreach (var parsed in negatives)
            {
                if (command.errors != null)
                    break;

                if (parsed.Value.Length > 0)
                {
                    var current = HMIClause.CreateVerbClause(this, parsed.Key, HMIPolarity.NEGATIVE, parsed.Value);
                    var tuple = (parsed.Key, HMIPolarity.NEGATIVE, parsed.Value);
                    this.segmentation.Add(tuple, current);
                    i++;
                }
            }
            HMIScope? scope = null;
            foreach (var phrase in this.segmentation.Values)
            {
                if (phrase.maximumScope != HMIScope.Undefined)
                {
                    if (!scope.HasValue)
                        scope = phrase.maximumScope;
                    else if ((int)phrase.maximumScope < (int)scope.Value)
                        scope = phrase.maximumScope;
                }
            }
            this.scope = scope.HasValue ? scope.Value : HMIScope.Undefined;
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
        protected bool Normalize()
        {
            //      (HMIClause singleton, HMIClause suborinate, HMIClause[] setters, HMIClause[] removals, HMIClause[] searches)
            this.normalized = (null, null, null, null, null);

            if (this.statement == null || this.segmentation == null)
            {
                this.Notify("error", "Could not normalize statement: driver design error");
                return false;
            }
            int position = 1;
            int last = this.segmentation.Count;
            var clauses = (from key in this.segmentation.Keys orderby key.order select this.segmentation[key]);
            foreach (var clause in clauses)
            {
                if (clause.type.IsSimple())
                    this.normalized.singleton = clause;
                else if (clause.type.IsDependent())
                    this.normalized.suborinate = clause;
                else if (clause.verb == Verbs.Search.VERB)
                    Append(ref this.normalized.searches, clause);
                else if (clause.verb == Verbs.Clear.VERB)
                    Append(ref this.normalized.removals, clause);
                else if (clause.verb == Verbs.Set.VERB)
                    Append(ref this.normalized.setters, clause);
                else
                    this.Notify("error", "Could not normalize statement: unexpected clause type encountered.");

                var simpleOutOfPlace = clause.type.IsSimple() && (position != 1);
                var dependentOutOfPlace = clause.type.IsDependent() && (position == 1 || position != last);

                if (simpleOutOfPlace && !clause.type.IsDependent())
                {
                    this.Notify("error", "The verb " + clause.verb + "can only be used to construct a simple statement");
                }
                else if (dependentOutOfPlace && !clause.type.IsSimple())
                {
                    this.Notify("error", "The verb " + clause.verb + "can only be used as a dependent clause");
                }
                else if (simpleOutOfPlace && dependentOutOfPlace)
                {
                    this.Notify("error", "The verb " + clause.verb + "can only be used to construct a simple statement or as the final dependent clause of the statement");
                }
            }
            return this.errors.Count == 0;
        }
        public bool Execute()
        {
            if (command.errors.Count == 0)
            {
                if (!this.Normalize())
                    return false;

                if (command.HasMacro() != HMIScope.Undefined)
                {
                    var macroDef = command.GetMacroDefinition();
                    var result = HMICommand.Driver.Write("quelle.macro." + macroDef.macroName, macroDef.macroScope, command.statement.statement);
                    if (result.errors != null)
                    {
                        foreach (var error in result.errors)
                            this.Notify("error", error);

                        return false;
                    }
                    if (!result.success)
                    {
                        this.Notify("error", "Unspecific macro error; Please contact vendor about this Quelle driver implementation");
                        return false;
                    }
                    return true;
                }

                if (normalized.singleton != null)
                {
                    normalized.singleton.Execute();
                }
                else
                {
                    if (normalized.setters != null)
                        foreach (var clause in normalized.setters)
                        {
                            clause.Execute();
                            if (this.errors.Count > 0)
                                return false;
                        }
                    if (normalized.removals != null)
                        foreach (var clause in normalized.removals)
                        {
                            clause.Execute();
                            if (this.errors.Count > 0)
                                return false;
                        }
                    if (normalized.searches != null)
                    {
                        foreach (var clause in normalized.searches)
                        {
                            clause.Execute();
                            if (this.errors.Count > 0)
                                return false;
                        }
                        if (!this.command.Search())
                            return false;
                    }
                    if (normalized.suborinate != null)
                    {
                        normalized.suborinate.Execute();
                        if (this.errors.Count > 0)
                            return false;
                    }
                    else if (normalized.searches != null)
                    {
                        ;   // default search summarization out goes here
                    }
                }
            }
            return true;
        }
    }
}

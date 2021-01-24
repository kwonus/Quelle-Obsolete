using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using static QuelleHMI.HMISegment;

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
        private HMICommand command;
        public void Notify(string mode, string message)
        {
            if (this.command != null)
                this.command.Notify(mode, message);
        }
        public UInt16 span { get; private set; }
        public string statement { get; private set; }
        public String[] rawSegments { get; private set; }  // right-side: each segment has a POLARITY
        private HMIPolarity[] polarities;
        private Dictionary<(UInt32 order, HMIPolarity polarity, string segment), HMISegment> segmentation;
        public Dictionary<UInt64, HMISegment> segments { get; private set; }

        public HMIStatement(HMICommand command, UInt16 span, string statement)
        {
            this.command = command;
            this.span = 0;
            this.statement = statement.Trim();

            if (this.statement.Length < 1)
                return;

            int len = this.statement.Length;

            // create segmentation boundaries, carefully ignoring quoted delimiters (can't use String.Split())
            //
            var c_quoted = false;   // character-quoted means that crrent character is quoted with \
            var d_quoted = false;   // double-quoted means that this character is enclosed in double-quotes
            var negatives = new Dictionary<UInt32, string>(); // order, rawSegment
            var positives = new Dictionary<UInt32, string>(); // order, rawSegment
            var polarity  = positives;
            int last = 0;
            UInt32 order = 1;
            int i;
            for (i = 0; /**/; i++)  // looking for "//" or "/-"
            {
                if (i >= len && last < len)
                {
                    polarity.Add(order++, this.statement.Substring(last, len).Trim());
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
                    case '\\':  c_quoted = true; continue;
                    case '"':   d_quoted = true; continue;
                    case '|':   break;                          // this should never see this trigger, but add for fail-safety and QC breakpoint during debugging
                }
                if (i >= len - 1)
                    continue;

                if (c != '/')
                    continue;

                char next = this.statement[i+1];

                if (last < i)
                { 
                    switch (next)
                    {
                        case '/':
                        case '-': polarity.Add(order++, this.statement.Substring(last, i).Trim());
                                  break;
                        default:  continue;
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

            this.rawSegments = new string[positives.Count + negatives.Count];
            this.polarities = new HMIPolarity[this.rawSegments.Length];

            this.segmentation = new Dictionary<(UInt32 order, HMIPolarity polarity, string segment), HMISegment>();
            this.segments = new Dictionary<ulong, HMISegment>();
            i = 0;
            foreach (var parsed in positives)
            {
                if (command.errors != null)
                    break;
                
                if (parsed.Value.Length > 0)
                {
                    this.rawSegments[i] = parsed.Value;
                    this.polarities[i] = HMIPolarity.POSITIVE;

                    var current = new HMISegment(this, parsed.Key, span, this.polarities[i], parsed.Value, HMIClauseType.INDEPENDENT);
                    var tuple = (parsed.Key, polarities[i], parsed.Value);
                    this.segmentation.Add(tuple, current);
                    this.segments.Add(current.sequence, current);
                    i++;
                }
            }
            foreach (var parsed in negatives)
            {
                if (command.errors != null)
                    break;

                if (parsed.Value.Length > 0)
                {
                    this.rawSegments[i] = parsed.Value;
                    this.polarities[i] = HMIPolarity.NEGATIVE;

                    var current = new HMISegment(this, parsed.Key, span, this.polarities[i], parsed.Value, HMIClauseType.INDEPENDENT);
                    var tuple = (parsed.Key, polarities[i], parsed.Value);
                    this.segmentation.Add(tuple, current);
                    this.segments.Add(current.sequence, current);
                    i++;
                }
            }
        }

        public (HMIScope scope, string[] errors, Dictionary<string, HMISegment[]> normalization) NormalizeStatement()
        {
            var results = new Dictionary<string, HMISegment[]>();

            if (this.statement == null || this.segments == null)
                return (HMIScope.Undefined, new string[] { "Driver design error" }, results);

            var errors = new List<string>();

            //  (HMIScope scope, string verb, HMISegment[] segments)

            var search      = NormalizeSearchSegments(errors);
            var file        = NormalizeFileSegments(errors);
            var persistence = NormalizePersistenceSegments(errors, (search.scope == HMIScope.Session) || (file.scope == HMIScope.Session) ? HMIScope.Session : HMIScope.Session);   // FILE or SEARCH segments coerce PERSISTENCE segments to Statement scope
            var status      = NormalizeStatusSegments(errors);
            var removal     = NormalizeRemovalSegments(errors);


            if (file.scope != HMIScope.Undefined && search.scope != HMIScope.Undefined)
                errors.Add("Cannot combine search segments with file segments.");


            int nonPersitence = (removal.scope != HMIScope.Undefined ? 1 : 0)
                              + (status.scope != HMIScope.Undefined ? 1 : 0);

            if (nonPersitence > 0)
            {
                if (file.scope != HMIScope.Undefined)
                    errors.Add("Cannot combine file segments with status or removal segments.");
                if (search.scope != HMIScope.Undefined)
                    errors.Add("Cannot combine search segments with status or removal segments.");
            }

            int configCount = (removal.scope     != HMIScope.Undefined ? 1 : 0)
                            + (status.scope      != HMIScope.Undefined ? 1 : 0)
                            + (persistence.scope != HMIScope.Undefined ? 1 : 0);

            if (configCount == 3)
            {
                if (removal.scope != HMIScope.Undefined)
                    errors.Add("Cannot combine removal segments with status or persistence segments.");
                if (status.scope != HMIScope.Undefined)
                    errors.Add("Cannot combine status segments with removal or persistence segments.");
                if (persistence.scope != HMIScope.Undefined)
                    errors.Add("Cannot combine persistence segments with status or removal segments.");
            }

            HMIScope minimalScope = errors.Count > 0 ? HMIScope.Undefined : HMIScope.System;

            if (minimalScope != HMIScope.Undefined)
                foreach (var scope in new HMIScope[]{ file.scope, search.scope, status.scope, persistence.scope, removal.scope})
                    if (scope != HMIScope.Undefined && (int)scope < (int)minimalScope)
                        minimalScope = scope;

            foreach (var command in new (HMIScope scope, string verb, HMISegment[] segments)[] { persistence, search, file, status, removal })
                if (command.verb != null && command.segments != null && command.scope != HMIScope.Undefined)
                    results.Add(command.verb, command.segments);

            return (minimalScope, errors.Count > 0 ? errors.ToArray() : null, results);
        }

        protected (HMIScope scope, string verb, HMISegment[] segments) NormalizeSearchSegments(List<string> errors)
        {
            if (this.statement == null || this.segments == null)
                return (HMIScope.Undefined, null, null);

            int cnt = 0;
            var verbs = new List<string>();
            var conformingSegments = new List<HMISegment>();

            foreach (var segment in this.segments.Values)
            {
                var verb = (from candidate in HMISegment.SearchVerbs where candidate == segment.verb select candidate).FirstOrDefault();
                if (verb != null)
                {
                    conformingSegments.Add(segment);
                    AddConformingVerb(verbs, verb);
                    cnt++;
                }
            }
            var normalizedVerb = verbs.Count >= 1 ? verbs[0] : null;
            var scoped = normalizedVerb != null ? HMIScope.Session : HMIScope.Undefined;
            if (verbs.Count >= 2)
            {
                normalizedVerb = null;  // // if it cannot be normalized/upgraded to a find verb, then the list of verbs cannot be normalized
                foreach (var verb in verbs)
                    if (verb == HMISegment.SearchVerbs[0] /*"find"*/)
                    {
                        normalizedVerb = HMISegment.SearchVerbs[0];
                        break;
                    }
                if (normalizedVerb == null)
                {
                    GetSearchMultiverbErrorMessage(verbs);
                    scoped = HMIScope.Undefined;
                }
            }
            return (scoped, normalizedVerb, conformingSegments.Count > 0 ? conformingSegments.ToArray() : null);
        }

        protected (HMIScope scope, string verb, HMISegment[] segments) NormalizeFileSegments(List<string> errors)
        {
            if (this.statement == null || this.segments == null)
                return (HMIScope.Undefined, null, null);

            int cnt = 0;
            var verbs = new List<string>();
            var conformingSegments = new List<HMISegment>();

            foreach (var segment in this.segments.Values)
            {
                var verb = (from candidate in HMISegment.SimpleDisplayVerbs where candidate == segment.verb select candidate).FirstOrDefault();
                if (verb != null)
                {
                    conformingSegments.Add(segment);
                    AddConformingVerb(verbs, verb);
                    cnt++;
                }
            }
            var normalizedVerb = verbs.Count == 1 ? verbs[0] : null;
            var scoped = verbs.Count == 1 ? HMIScope.Session : HMIScope.Undefined;
            if (verbs.Count > 1)
                errors.Add(GetStandardMultiverbErrorMessage(verbs));

            return (scoped, normalizedVerb, conformingSegments.Count > 0 ? conformingSegments.ToArray() : null);
        }

        protected (HMIScope scope, string verb, HMISegment[] segments) NormalizeRemovalSegments(List<string> errors)
        {
            if (this.statement == null || this.segments == null)
                return (HMIScope.Undefined, null, null);

            var verbs = new List<string>();
            var conformingSegments = new List<HMISegment>();

            var scope = HMIScope.System;
            int scopeLevel = (int)scope;

            int cnt = 0;
            foreach (var segment in this.segments.Values)
            {
                var verb = (from candidate in HMISegment.RemovalVerbs
                            where candidate == segment.verb
                            select char.IsLetter(candidate[0]) ? candidate : candidate.Substring(1)).FirstOrDefault();

                if (verb != null)
                {
                    conformingSegments.Add(segment);
                    AddConformingVerb(verbs, verb, ref scope);
                    cnt++;
                }
            }
            var scoped = verbs.Count == 1 ? (HMIScope)scope : HMIScope.Undefined;
            var normalizedVerb = verbs.Count == 1 ? verbs[0] : null;
            if (verbs.Count > 1)
            {
                scoped = HMIScope.Undefined;
                errors.Add(GetStandardMultiverbErrorMessage(verbs));
            }
            if (verbs.Count >= 1 && normalizedVerb != HMISegment.RemovalVerbs[0] && cnt > 1)
            {
                scoped = HMIScope.Undefined;
                string error = GetConflictingVerbsErrorMessage(HMISegment.CLEAR, HMISegment.REMOVE);
                errors.Add(error);
            }
            return (scoped, normalizedVerb, conformingSegments.Count > 0 ? conformingSegments.ToArray() : null);
        }

        protected (HMIScope scope, string verb, HMISegment[] segments) NormalizeStatusSegments(List<string> errors)
        {
            if (this.statement == null || this.segments == null)
                return (HMIScope.Undefined, null, null);

            var verbs = new List<string>();
            var conformingSegments = new List<HMISegment>();

            var scope = HMIScope.System;
            int scopeLevel = (int)scope;

            int cnt = 0;
            foreach (var segment in this.segments.Values)
            {
                var verb = (from candidate in HMISegment.GetterVerbs
                            where candidate == segment.verb
                            select char.IsLetter(candidate[0]) ? candidate : candidate.Substring(1)).FirstOrDefault();

                if (verb != null)
                {
                    conformingSegments.Add(segment);
                    AddConformingVerb(verbs, verb, ref scope);
                    cnt++;
                }
            }
            var normalizedVerb = verbs.Count == 1 ? verbs[0] : null;
            var scoped = verbs.Count == 1 ? (HMIScope)scope : HMIScope.Undefined;
            if (verbs.Count > 1)
            {
                scoped = HMIScope.Undefined;
                errors.Add(GetStandardMultiverbErrorMessage(verbs));
            }
            if (verbs.Count >= 1 && normalizedVerb != HMISegment.RemovalVerbs[0] && cnt > 1)
            {
                scoped = HMIScope.Undefined;
                string error = GetConflictingVerbsErrorMessage(HMISegment.SET, HMISegment.EXPAND);
                errors.Add(error);
            }
            return (scoped, normalizedVerb, conformingSegments.Count > 0 ? conformingSegments.ToArray() : null);
        }

        protected (HMIScope scope, string verb, HMISegment[] segments) NormalizePersistenceSegments(List<string> errors, HMIScope maxScope)
        {
            if (this.statement == null || this.segments == null)
                return (HMIScope.Undefined, null, null);

            var verbs = new List<string>();
            var conformingSegments = new List<HMISegment>();

            var scope = maxScope;
            int scopeLevel = (int)scope;

            int cnt = 0;
            foreach (var segment in this.segments.Values)
            {
                var verb = (from candidate in HMISegment.SetterVerbs
                            where candidate == segment.verb
                            select char.IsLetter(candidate[0]) ? candidate : candidate.Substring(1)).FirstOrDefault();

                if (verb != null)
                {
                    conformingSegments.Add(segment);
                    AddConformingVerb(verbs, verb, ref scope);
                    cnt++;
                }
            }
            var normalizedVerb = verbs.Count == 1 ? verbs[0] : null;
            var scoped = verbs.Count == 1 ? (HMIScope) scope : HMIScope.Undefined;
            if (verbs.Count > 1)
            {
                scoped = HMIScope.Undefined;
                string error = GetStandardMultiverbErrorMessage(verbs);
                errors.Add(error);
            }
            return (scoped, normalizedVerb, conformingSegments.Count > 0 ? conformingSegments.ToArray() : null);
        }

        protected static void AddConformingVerb(List<string> verbs, string verb, ref HMIScope scope)
        {
            if (scope != HMIScope.Session)
            {
                var segmentScope = GetScope(verb[0]);
                if ((int)segmentScope < (int)scope)
                    scope = segmentScope;
            }
            AddConformingVerb(verbs, verb);
        }
        protected static void AddConformingVerb(List<string> verbs, string verb)
        {
            if (!verbs.Contains(verb))
                verbs.Add(verb);
        }
        protected static HMIScope GetScope(char scope)
        {
            switch (scope)
            {
                case '#': return HMIScope.System;
                default: return HMIScope.Session;
            }
        }
        protected static string GetStandardMultiverbErrorMessage(List<string> verbs)
        {
            if (verbs != null && verbs.Count > 1)
            {
                var type = HMISegment.IsVerb(verbs[0]).directive.ToLower();

                string error = "More than one " + type + " verb was provided.  All verbs have to be consistant across all segments. Please submit only one of these verbs";
                var comma = ": ";
                foreach (string verb in verbs)
                {
                    error += comma;
                    error += verb;
                    comma = ", ";
                }
                return error + ".";
            }
            return null;
        }
        protected static string GetSearchMultiverbErrorMessage(List<string> verbs)
        {
            if (verbs != null && verbs.Count > 1)
            {
                string error = "More than one search verb was provided.  All search verbs can be promoted to 'find' when the 'find' verb is provided.  Otherwise all verbs have to be consistant across all segments. Please submit only one of these verbs";
                var comma = ": ";
                foreach (string verb in verbs)
                {
                    error += comma;
                    error += verb;
                    comma = ", ";
                }
                return error + ".";
            }
            return null;
        }
        protected static string GetConflictingVerbsErrorMessage(string verb1, string verb2)
        {
            var type = HMISegment.IsVerb(verb1).directive.ToLower();
            return "Only the verb '" + verb1 + "' can be combined into multiple " + type + " segments. Other verbs such as '" + verb2 + "' cannot be combined";
        }
    }
}

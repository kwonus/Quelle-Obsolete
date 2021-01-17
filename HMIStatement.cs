using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace ClarityHMI
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
        private char[] polarities;
        private Dictionary<string, HMISegment> segmentation;
        public Dictionary<UInt64, HMISegment> segments { get; private set; }

        private static string[] plus = new string[] { "[+]", "(+)", " + ", "\t+ ", " +\t" };
        private static string[] minus = new string[] { "[-]", "(-)", " - ", "\t- ", " -\t" };
        private static string[] delimiter = null;
        ///xxx
        public HMIStatement(HMICommand command, UInt16 span, string statement)
        {
            this.command = command;
            this.span = 0;
            this.statement = statement.Trim();

            if (delimiter == null)
            {
                delimiter = new string[plus.Length + minus.Length];

                for (int i = 0; i < plus.Length; i++)
                    delimiter[i] = plus[i];
                for (int i = 0; i < minus.Length; i++)
                    delimiter[plus.Length + i] = plus[i];
            }
            this.span = span;
            this.statement = statement.Trim();
            string normalized = this.statement;
            if (normalized.StartsWith('-'))
                normalized = minus[0] + statement.Substring(1);
            else if (normalized.StartsWith('+'))
                normalized = plus[0] + statement.Substring(1);
            else
                normalized = plus[0] + statement;

            var rawSegments = normalized.Split(delimiter, StringSplitOptions.None);
            this.rawSegments = new string[rawSegments.Length - 1];
            Array.Copy(rawSegments, 1, this.rawSegments, 0, this.rawSegments.Length);
            this.polarities = new char[this.rawSegments.Length];

            this.segmentation = new Dictionary<string, HMISegment>();
            this.segments = new Dictionary<ulong, HMISegment>();

            for (UInt32 i = 0; i < this.rawSegments.Length; i++)
            {
                int next = 3 + this.rawSegments[i].Length;
                this.polarities[i] = normalized[1];
                normalized = normalized.Substring(next);

                this.rawSegments[i] = this.rawSegments[i].Trim();
                string key = "[" + this.polarities[i] + "] " + this.rawSegments[i].ToLower();
                var current = new HMISegment(this, i + 1, span, polarities[i], this.rawSegments[i]);
                this.segmentation.Add(key, current);
                this.segments.Add(current.sequence, current);

                if (command.errors != null)
                    break;
            }

            // Delete redundant [+] and [-] polarities
            foreach (string key in this.segmentation.Keys)
            {
                if (key[1] == '-')
                {
                    string cancel = "[+]" + key.Substring(3);
                    if (this.segmentation.ContainsKey(cancel))
                        this.segments.Remove(this.segmentation[cancel].sequence);
                }
            }
        }

        public (HMIScope scope, string[] errors, Dictionary<string, HMISegment[]> normalization) NormalizeStatement()
        {
            var results = new Dictionary<string, HMISegment[]>();

            if (this.statement == null || this.segments == null)
                return (HMIScope.Undefined, new string[] { "river design error" }, results);

            var errors = new List<string>();

            //  (HMIScope scope, string verb, HMISegment[] segments)

            var search      = NormalizeSearchSegments(errors);
            var file        = NormalizeFileSegments(errors);
            var persistence = NormalizePersistenceSegments(errors, (search.scope == HMIScope.Statement) || (file.scope == HMIScope.Statement) ? HMIScope.Statement : HMIScope.Cloud);   // FILE or SEARCH segments coerce PERSISTENCE segments to Statement scope
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

            HMIScope minimalScope = errors.Count > 0 ? HMIScope.Undefined : HMIScope.Cloud;

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
                var verb = (from candidate in HMISegment.Searches where candidate == segment.verb select candidate).FirstOrDefault();
                if (verb != null)
                {
                    conformingSegments.Add(segment);
                    AddConformingVerb(verbs, verb);
                    cnt++;
                }
            }
            var normalizedVerb = verbs.Count >= 1 ? verbs[0] : null;
            var scoped = normalizedVerb != null ? HMIScope.Statement : HMIScope.Undefined;
            if (verbs.Count >= 2)
            {
                normalizedVerb = null;  // // if it cannot be normalized/upgraded to a find verb, then the list of verbs cannot be normalized
                foreach (var verb in verbs)
                    if (verb == HMISegment.Searches[0] /*"find"*/)
                    {
                        normalizedVerb = HMISegment.Searches[0];
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
                var verb = (from candidate in HMISegment.Files where candidate == segment.verb select candidate).FirstOrDefault();
                if (verb != null)
                {
                    conformingSegments.Add(segment);
                    AddConformingVerb(verbs, verb);
                    cnt++;
                }
            }
            var normalizedVerb = verbs.Count == 1 ? verbs[0] : null;
            var scoped = verbs.Count == 1 ? HMIScope.Statement : HMIScope.Undefined;
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

            var scope = HMIScope.Cloud;
            int scopeLevel = (int)scope;

            int cnt = 0;
            foreach (var segment in this.segments.Values)
            {
                var verb = (from candidate in HMISegment.Removals
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
            if (verbs.Count >= 1 && normalizedVerb != HMISegment.Removals[0] && cnt > 1)
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

            var scope = HMIScope.Cloud;
            int scopeLevel = (int)scope;

            int cnt = 0;
            foreach (var segment in this.segments.Values)
            {
                var verb = (from candidate in HMISegment.Statuses
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
            if (verbs.Count >= 1 && normalizedVerb != HMISegment.Removals[0] && cnt > 1)
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
                var verb = (from candidate in HMISegment.Persistences
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
            if (scope != HMIScope.Statement)
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
                case '@': return HMIScope.Cloud;
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

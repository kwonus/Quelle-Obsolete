using QuelleHMI.Definitions;
using QuelleHMI.Actions;
using System.Linq;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace QuelleHMI
{ 
    [DataContract]
    public class QRequestSearch
    {
        public QRequestSearch(bool quoted)
        {
            this.clauses  = null;
            this.controls = new QSearchControls(true);
            this.count    = 10;
        }

        public QRequestSearch(HMIStatement statement, UInt64 cnt = 10)
        {
            int size = 0;
            foreach (var clause in statement.segmentation.Values)
            {
                if (clause.verb == Search.FIND)
                    size++;
            }
            this.clauses = new QClauseSearch[size];
            size = 0;
            var searches = (from key in statement.segmentation.Keys orderby key select statement.segmentation[key]);
            foreach (var clause in searches)
            {
                if (clause.verb == Search.FIND)
                    this.clauses[size++] = new QClauseSearch((Search) clause);
            }
            this.controls = new QSearchControls(true);
            this.count = cnt;
        }
        [DataMember]
        public QClauseSearch[] clauses { get; set; }

        [DataMember]
        public QSearchControls controls;
        [DataMember]
        public Guid session { get; set; }
        [DataMember]
        public UInt64 cursor { get; set; }
        [DataMember]
        public UInt64 count { get; set; }

        public QRequestSearch(IQuelleSearchRequest irequest, UInt64 cnt = 10)
        {
            this.clauses = new QClauseSearch[irequest.clauses.Length];
            for (int i = 0; i < irequest.clauses.Length; i++)
                this.clauses[i] = new QClauseSearch(irequest.clauses[i]);
            this.count = cnt;
            this.controls = new QSearchControls(true);
            this.session = irequest.session;
            this.cursor = irequest.cursor;
            this.count = irequest.count;
        }
    }
    [DataContract]
    public class QSearchResult : IQuelleSearchResult
    {
        public QSearchResult() { /*for serialization*/ }

        [DataMember]
        public HashSet<UInt64> segments { get; set; } 
        [DataMember]
        public Dictionary<UInt32, UInt64> tokens { get; set; }
        [DataMember]
        public string summary { get; set; }
        [DataMember]
        public Guid session { get; set; } // MD5/GUID
        [DataMember]
        public Dictionary<UInt32, String> abstracts { get; set; }
        [DataMember]
        public UInt64 cursor { get; set; }
        [DataMember]
        public UInt64 count { get; set; }
        [DataMember]
        public UInt64 remainder { get; set; }
        [DataMember]
        public Dictionary<string, List<string>> messages { get; set; }

        public QSearchResult(IQuelleSearchResult iresult)
        {
            this.summary = iresult.summary;
            this.tokens = iresult.tokens;
            this.abstracts = iresult.abstracts;
            this.cursor = iresult.cursor;
            this.count = iresult.count;
            this.remainder = iresult.remainder;
            this.session = iresult.session;
            this.messages = iresult.messages;
        }

        public void AddWarning(string message)
        {
            if (this.messages == null)
                this.messages = new Dictionary<string, List<string>>();
            var list = this.messages.ContainsKey("warnings") ? this.messages["warnings"] : null;
            if (list == null)
            {
                list = new List<string>();
                this.messages.Add("warnings", list);
            }
            list.Add(message);
        }
        public void AddError(string message)
        {
            if (this.messages == null)
                this.messages = new Dictionary<string, List<string>>();
            var list = this.messages.ContainsKey("errors") ? this.messages["errors"] : null;
            if (list == null)
            {
                list = new List<string>();
                this.messages.Add("errors", list);
            }
            list.Add(message);
        }
    }
 }

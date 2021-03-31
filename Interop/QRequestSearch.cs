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
        public QRequestSearch()
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
        public UInt64 count { get; set; }

        public QRequestSearch(IQuelleSearchRequest irequest, UInt64 cnt = 10)
        {
            this.clauses = new QClauseSearch[irequest.clauses.Length];
            for (int i = 0; i < irequest.clauses.Length; i++)
                this.clauses[i] = new QClauseSearch(irequest.clauses[i]);
            this.count = cnt;
            this.controls = new QSearchControls(true);
        }
    }
    [DataContract]
    public class QSearchResult : QResultFetch, IQuelleSearchResult
    {
        public QSearchResult() { /*for serialization*/ }

        [DataMember]
        public Dictionary<byte, Dictionary<byte, Dictionary<byte, Dictionary<byte, UInt64>>>> records { get; set; }
        [DataMember]
        public string summary { get; set; }

        QSearchResult(IQuelleSearchResult iresult): base((IQuelleFetchResult) iresult)
        {
            this.summary = iresult.summary;
            this.records = iresult.records;
        }
    }
 }

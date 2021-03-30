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
        public QRequestSearch() { /*for serialization*/ }

        public QRequestSearch(HMIStatement statement)
        {
            int cnt = 0;
            foreach (var clause in statement.segmentation.Values)
            {
                if (clause.verb == Search.FIND)
                    cnt++;
            }
            this.Clauses = new QClauseSearch[cnt];
            cnt = 0;
            var searches = (from key in statement.segmentation.Keys orderby key select statement.segmentation[key]);
            foreach (var clause in searches)
            {
                if (clause.verb == Search.FIND)
                    this.Clauses[cnt++] = new QClauseSearch((Search) clause);
            }
            this.Controls = new QSearchControls();
            this.Controls.domain = QuelleControlConfig.search.domain;
            this.Controls.exact  = QuelleControlConfig.search.exact.Value;
            this.Controls.span   = QuelleControlConfig.search.span.Value;
        }
        [DataMember]
        public QClauseSearch[] Clauses { get; set; }

        [DataMember]
        public QSearchControls Controls;
        [DataMember]
        public UInt64 Count { get; set; }

        public QRequestSearch(IQuelleSearchRequest irequest)
        {
            this.Clauses = new QClauseSearch[irequest.clauses.Length];
            for (int i = 0; i < irequest.clauses.Length; i++)
                this.Clauses[i] = new QClauseSearch(irequest.clauses[i]);
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

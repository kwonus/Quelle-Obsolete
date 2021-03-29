using QuelleHMI.Definitions;
using QuelleHMI.Actions;
using System.Linq;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace QuelleHMI
{
    [DataContract]
    public class QRequestSearchBrief
    {
        public QRequestSearchBrief() { /*for msgpack*/ }

        public QRequestSearchBrief(IQuelleSearchRequest statement)
        {
            this.clauses = new string[statement.clauses.Length];
            for (int i = 0; i < this.clauses.Length; i++)
            {
                this.clauses[i] = statement.clauses[i].polarity == '-' ? "-- " : "";
                this.clauses[i] += statement.clauses[i].segment;
            }
            /*
            this.qcontrols = new QSearchControls();
            this.qcontrols.domain = QuelleControlConfig.search.domain;
            this.qcontrols.exact = QuelleControlConfig.search.exact.Value;
            this.qcontrols.span = QuelleControlConfig.search.span.Value;
            */
            this.controls = new Dictionary<string, string>();
            if (QuelleControlConfig.search.domain != null)
                this.controls.Add("domain", QuelleControlConfig.search.domain);
            if (QuelleControlConfig.search.exact != null)
                this.controls.Add("exact", QuelleControlConfig.search.exact.Value ? "1" : "0");
            if (QuelleControlConfig.search.span != null)
                this.controls.Add("span", QuelleControlConfig.search.span.ToString());
            if (QuelleControlConfig.search.host != null)
                this.controls.Add("host", QuelleControlConfig.search.host);
        }

        [DataMember]
        public string[] clauses;
        [DataMember]
        public Dictionary<string, string> controls;
        //        public QSearchControls qcontrols;
        [DataMember]
        public UInt64 count { get; set; }
    }
    [DataContract]
    public class QRequestSearch : IQuelleSearchRequest
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

        [IgnoreDataMember]
        public IQuelleSearchClause[] clauses
        {
            get => this.Clauses;
            set
            {
                this.clauses = new QClauseSearch[value.Length];
                int i = 0;
                foreach (var val in value)
                    this.clauses[i++] = new QClauseSearch(val);
            }
        }
        [IgnoreDataMember]
        public IQuelleSearchControls controls
        {
            get => this.Controls;
            set => this.Controls = new QSearchControls(value);
        }
        [IgnoreDataMember]
        public UInt64 count
        {
            get => this.Count;
            set => this.Count = value;
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
                this.Clauses[i] = new QClauseSearch(clauses[i]);
        }
    }
    [DataContract]
    public class QSearchResult : QResultFetch, IQuelleSearchResult
    {
        public QSearchResult() { /*for msgpack*/ }

        [DataMember]
        public string summary { get; set; }
        public IQuelleSearchRequest enrichedRequest
        {
            get => this.qEnrichedRequest;
            set => this.qEnrichedRequest = new QRequestSearch(value);
        }
        [DataMember]
        public QRequestSearch qEnrichedRequest { get; set; }

        QSearchResult(IQuelleSearchResult iresult): base((IQuelleFetchResult) iresult)
        {
            this.summary = iresult.summary;
            this.qEnrichedRequest = new QRequestSearch(iresult.enrichedRequest);
        }
    }
 }

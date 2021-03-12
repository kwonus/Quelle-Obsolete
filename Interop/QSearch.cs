using MessagePack;
using QuelleHMI.Controls;
using QuelleHMI.Verbs;
using System.Linq;
using System;

namespace QuelleHMI
{
    [MessagePackObject]
    public class QSearchRequest : IQuelleSearchRequest
    {
        public QSearchRequest() { /*for msgpack*/ }

        public QSearchRequest(HMIStatement statement)
        {
            int cnt = 0;
            foreach (var clause in statement.segmentation.Values)
            {
                if (clause.verb == Search.VERB)
                    cnt++;
            }
            this.qclauses = new QSearchClause[cnt];
            cnt = 0;
            var searches = (from key in statement.segmentation.Keys orderby key select statement.segmentation[key]);
            foreach (var clause in searches)
            {
                if (clause.verb == Search.VERB)
                    this.qclauses[cnt++] = new QSearchClause((Search) clause);
            }
            this.qcontrols = new QSearchControls();
            var controls = HMICommand.configuration.search;
            this.qcontrols.domain = controls.domain;
            this.qcontrols.exact  = controls.exact;
            this.qcontrols.span   = controls.span;
        }

        [IgnoreMember]
        public IQuelleSearchClause[] clauses
        {
            get => this.qclauses;
            set
            {
                this.qclauses = new QSearchClause[value.Length];
                int i = 0;
                foreach (var val in value)
                    this.qclauses[i++] = new QSearchClause(val);
            }
        }
        [IgnoreMember]
        public IQuelleSearchControls controls
        {
            get => this.qcontrols;
            set => this.qcontrols = new QSearchControls(value);
        }
        [Key(1)]
        public QSearchClause[] qclauses { get; set; }

        [Key(2)]
        public QSearchControls qcontrols;
        [Key(3)]
        public UInt64 count { get; set; }

        public QSearchRequest(IQuelleSearchRequest irequest)
        {
            this.qclauses = new QSearchClause[irequest.clauses.Length];
            for (int i = 0; i < irequest.clauses.Length; i++)
                this.qclauses[i] = new QSearchClause(irequest.clauses[i]);
        }
    }
    [MessagePackObject]
    public class QSearchResult : QFetchResult, IQuelleSearchResult
    {
        public QSearchResult() { /*for msgpack*/ }

        [Key(8)]
        public string summary { get; set; }
        public IQuelleSearchRequest enrichedRequest
        {
            get => this.qEnrichedRequest;
            set => this.qEnrichedRequest = new QSearchRequest(value);
        }
        [Key(9)]
        public QSearchRequest qEnrichedRequest { get; set; }

        QSearchResult(IQuelleSearchResult iresult): base((IQuelleFetchResult) iresult)
        {
            this.summary = iresult.summary;
            this.qEnrichedRequest = new QSearchRequest(iresult.enrichedRequest);
        }
    }
 }

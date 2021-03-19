using MessagePack;
using QuelleHMI.Definitions;
using QuelleHMI.Actions;
using System.Linq;
using System;

namespace QuelleHMI
{
    [MessagePackObject]
    public class QRequestSearch : IQuelleSearchRequest
    {
        public QRequestSearch() { /*for msgpack*/ }

        public QRequestSearch(HMIStatement statement)
        {
            int cnt = 0;
            foreach (var clause in statement.segmentation.Values)
            {
                if (clause.verb == Search.FIND)
                    cnt++;
            }
            this.qclauses = new QClauseSearch[cnt];
            cnt = 0;
            var searches = (from key in statement.segmentation.Keys orderby key select statement.segmentation[key]);
            foreach (var clause in searches)
            {
                if (clause.verb == Search.FIND)
                    this.qclauses[cnt++] = new QClauseSearch((Search) clause);
            }
            this.qcontrols = new QSearchControls();
            this.qcontrols.domain = QuelleControlConfig.search.domain;
            this.qcontrols.exact  = QuelleControlConfig.search.exact.Value;
            this.qcontrols.span   = QuelleControlConfig.search.span.Value;
        }

        [IgnoreMember]
        public IQuelleSearchClause[] clauses
        {
            get => this.qclauses;
            set
            {
                this.qclauses = new QClauseSearch[value.Length];
                int i = 0;
                foreach (var val in value)
                    this.qclauses[i++] = new QClauseSearch(val);
            }
        }
        [IgnoreMember]
        public IQuelleSearchControls controls
        {
            get => this.qcontrols;
            set => this.qcontrols = new QSearchControls(value);
        }
        [Key(1)]
        public QClauseSearch[] qclauses { get; set; }

        [Key(2)]
        public QSearchControls qcontrols;
        [Key(3)]
        public UInt64 count { get; set; }

        public QRequestSearch(IQuelleSearchRequest irequest)
        {
            this.qclauses = new QClauseSearch[irequest.clauses.Length];
            for (int i = 0; i < irequest.clauses.Length; i++)
                this.qclauses[i] = new QClauseSearch(irequest.clauses[i]);
        }
    }
    [MessagePackObject]
    public class QSearchResult : QResultFetch, IQuelleSearchResult
    {
        public QSearchResult() { /*for msgpack*/ }

        [Key(8)]
        public string summary { get; set; }
        public IQuelleSearchRequest enrichedRequest
        {
            get => this.qEnrichedRequest;
            set => this.qEnrichedRequest = new QRequestSearch(value);
        }
        [Key(9)]
        public QRequestSearch qEnrichedRequest { get; set; }

        QSearchResult(IQuelleSearchResult iresult): base((IQuelleFetchResult) iresult)
        {
            this.summary = iresult.summary;
            this.qEnrichedRequest = new QRequestSearch(iresult.enrichedRequest);
        }
    }
 }

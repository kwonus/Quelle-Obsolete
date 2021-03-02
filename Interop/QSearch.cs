using MessagePack;
using QuelleHMI.Controls;
using QuelleHMI.Verbs;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace QuelleHMI
{
    [MessagePackObject]
    public class QSearchRequest : IQuelleSearchRequest
    {
        public QSearchRequest() { /*for protobuf*/ }

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
    public class PBSearchResult : PBFetchResult, IQuelleSearchResult
    {
        public PBSearchResult() { /*for protobuf*/ }

        [Key(8)]
        public string summary { get; set; }
        public IQuelleSearchRequest enrichedRequest
        {
            get => this.pbEnrichedRequest;
            set => this.pbEnrichedRequest = new QSearchRequest(value);
        }
        [Key(9)]
        public QSearchRequest pbEnrichedRequest { get; set; }

        PBSearchResult(IQuelleSearchResult iresult): base((IQuelleFetchResult) iresult)
        {
            this.summary = iresult.summary;
            this.pbEnrichedRequest = new QSearchRequest(iresult.enrichedRequest);
        }
    }
 }

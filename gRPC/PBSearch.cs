using ProtoBuf;
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
    [ProtoContract]
    public class PBSearchRequest : IQuelleSearchRequest
    {
        public PBSearchRequest() { /*for protobuf*/ }

        [ProtoIgnore]
        public IQuelleSearchClause[] clauses
        {
            get => this.pbclauses;
            set
            {
                this.pbclauses = new PBSearchClause[value.Length];
                int i = 0;
                foreach (var val in value)
                    this.pbclauses[i++] = new PBSearchClause(val);
            }
        }
        [ProtoIgnore]
        public IQuelleSearchControls controls
        {
            get => this.pbcontrols;
            set => this.pbcontrols = new PBSearchControls(value);
        }
        [ProtoMember(1)]
        public PBSearchClause[] pbclauses { get; set; }

 //     [ProtoMember(2)]
        [ProtoIgnore]
        public PBSearchControls pbcontrols;
        [ProtoMember(3)]
        public UInt64 count { get; set; }

        public PBSearchRequest(IQuelleSearchRequest irequest)
        {
            this.pbclauses = new PBSearchClause[irequest.clauses.Length];
            for (int i = 0; i < irequest.clauses.Length; i++)
                this.pbclauses[i] = new PBSearchClause(irequest.clauses[i]);
        }
    }
    [ProtoContract]
    public class PBSearchResult : PBFetchResult, IQuelleSearchResult
    {
        public PBSearchResult() { /*for protobuf*/ }

        [ProtoMember(8)]
        public string summary { get; set; }
        public IQuelleSearchRequest enrichedRequest
        {
            get => this.pbEnrichedRequest;
            set => this.pbEnrichedRequest = new PBSearchRequest(value);
        }
        [ProtoMember(9)]
        public PBSearchRequest pbEnrichedRequest { get; set; }

        PBSearchResult(IQuelleSearchResult iresult): base((IQuelleFetchResult) iresult)
        {
            this.summary = iresult.summary;
            this.pbEnrichedRequest = new PBSearchRequest(iresult.enrichedRequest);
        }
    }
 }

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
    [DataContract]
    public class PBSearchRequest : IQuelleSearchRequest
    {
        public IQuelleSearchClause[] clauses
        {
            get => this.pbclauses;
        }
        public IQuelleSearchControls controls
        {
            get => this.pbcontrols;
        }
        [DataMember(Order = 1)]
        public PBSearchClause[] pbclauses { get; set; }

        [DataMember(Order = 2)]
        public PBSearchControls pbcontrols;
        [DataMember(Order = 3)]
        public UInt64 count { get; set; }

        public PBSearchRequest(IQuelleSearchRequest irequest)
        {
            this.pbclauses = new PBSearchClause[irequest.clauses.Length];
            for (int i = 0; i < irequest.clauses.Length; i++)
                this.pbclauses[i] = new PBSearchClause(irequest.clauses[i]);
        }
    }
    public class PBSearchResult : PBFetchResult, IQuelleSearchResult
    {
        [DataMember(Order = 8)]
        public string summary { get; set; }
        public IQuelleSearchRequest enrichedRequest
        {
            get => this.pbEnrichedRequest;
        }
        [DataMember(Order = 9)]
        public PBSearchRequest pbEnrichedRequest { get; set; }

        PBSearchResult(IQuelleSearchResult iresult): base((IQuelleFetchResult) iresult)
        {
            this.summary = iresult.summary;
            this.pbEnrichedRequest = new PBSearchRequest(iresult.enrichedRequest);
        }
    }
 }

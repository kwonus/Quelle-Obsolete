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
    public class PBPageRequest : IQuellePageRequest
    {
        [DataMember(Order = 1)]
        public Guid session { get; set; }
        [DataMember(Order = 2)]
        public string format { get; set; }
        [DataMember(Order = 3)]
        public UInt64 page { get; set; }

        public PBPageRequest(IQuellePageRequest irequest)
        {
            this.session = irequest.session;
            this.format = irequest.format;
            this.page = irequest.page;
        }
    }
    [DataContract]
    public class PBPageResult : PBQuelleResult, IQuellePageResult
    {
        [DataMember(Order = 4)]
        public string result { get; set; }
        public IQuellePageRequest request
        {
            get => this.pbRequest;
        }
        [DataMember(Order = 5)]
        public PBPageRequest pbRequest { get; set; }

        public PBPageResult(IQuellePageResult iresult) : base((IQuelleResult)iresult)
        {
            this.result = iresult.result;
            this.pbRequest = new PBPageRequest(iresult.request);
        }
    }
}

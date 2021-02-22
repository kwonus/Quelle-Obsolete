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
    public class PBPageRequest : IQuellePageRequest
    {
        public PBPageRequest() { /*for protobuf*/ }

        [ProtoMember(1)]
        public Guid session { get; set; }
        [ProtoMember(2)]
        public string format { get; set; }
        [ProtoMember(3)]
        public UInt64 page { get; set; }

        public PBPageRequest(IQuellePageRequest irequest)
        {
            this.session = irequest.session;
            this.format = irequest.format;
            this.page = irequest.page;
        }
    }
    [ProtoContract]
    public class PBPageResult : PBQuelleResult, IQuellePageResult
    {
        public PBPageResult() { /*for protobuf*/ }

        [ProtoMember(4)]
        public string result { get; set; }
        [ProtoIgnore]
        public IQuellePageRequest request
        {
            get => this.pbRequest;
            set => this.pbRequest = new PBPageRequest(value);
        }
        [ProtoMember(5)]
        public PBPageRequest pbRequest { get; set; }

        public PBPageResult(IQuellePageResult iresult) : base((IQuelleResult)iresult)
        {
            this.result = iresult.result;
            this.pbRequest = new PBPageRequest(iresult.request);
        }
    }
}

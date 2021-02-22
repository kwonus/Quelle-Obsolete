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
    public class PBStatusRequest : IQuelleStatusRequest
    {
        public PBStatusRequest() { /*for protobuf*/ }

        public PBStatusRequest(IQuelleStatusRequest irequest)
        {
            ;
        }
    }
    [ProtoContract]
    public class PBStatusResult: PBQuelleResult, IQuelleStatusResult
    {
        public PBStatusResult() { /*for protobuf*/ }

        [ProtoMember(1)]
        public Guid[] sessions { get; set; }

        public PBStatusResult(IQuelleStatusResult iresult): base((IQuelleResult)iresult)
        {
            this.sessions = iresult.sessions;
        }
    }
}

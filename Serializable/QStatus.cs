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
    public class QStatusRequest : IQuelleStatusRequest
    {
        public QStatusRequest() { /*for protobuf*/ }

        public QStatusRequest(IQuelleStatusRequest irequest)
        {
            ;
        }
    }
    [MessagePackObject]
    public class PBStatusResult: QResult, IQuelleStatusResult
    {
        public PBStatusResult() { /*for protobuf*/ }

        [Key(1)]
        public Guid[] sessions { get; set; }

        public PBStatusResult(IQuelleStatusResult iresult): base((IQuelleResult)iresult)
        {
            this.sessions = iresult.sessions;
        }
    }
}

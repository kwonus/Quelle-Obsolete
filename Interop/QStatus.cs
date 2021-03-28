using System;
using System.Runtime.Serialization;

namespace QuelleHMI
{
    [DataContract]
    public class QStatusResult: QResult, IQuelleStatusResult
    {
        public QStatusResult() { /*for msgpack*/ }

        [DataMember]
        public Guid[] sessions { get; set; }
        [DataMember]
        public string summary { get; set; }


        public QStatusResult(IQuelleStatusResult iresult): base((IQuelleResult)iresult)
        {
            this.sessions = iresult.sessions;
        }
    }
}

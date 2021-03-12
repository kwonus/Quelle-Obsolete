using MessagePack;
using System;

namespace QuelleHMI
{
    [MessagePackObject]
    public class QStatusResult: QResult, IQuelleStatusResult
    {
        public QStatusResult() { /*for msgpack*/ }

        [Key(4)]
        public Guid[] sessions { get; set; }
        [Key(5)]
        public string summary { get; set; }


        public QStatusResult(IQuelleStatusResult iresult): base((IQuelleResult)iresult)
        {
            this.sessions = iresult.sessions;
        }
    }
}

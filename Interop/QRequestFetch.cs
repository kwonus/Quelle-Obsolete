using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace QuelleHMI
{
    [DataContract]
    public class QRequestFetch: IQuelleFetchRequest
    {
        public QRequestFetch() : base() { /*for msgpack*/ }

        [DataMember]
        public Guid session { get; set; }
        [DataMember]
        public UInt64 cursor { get; set; }
        [DataMember]
        public UInt64 count { get; set; }

        public QRequestFetch(IQuelleFetchRequest irequest)
        {
            this.session = irequest.session;
            this.cursor = irequest.cursor;
            this.count = irequest.count;
        }
    }

    [DataContract]
    public class QResultFetch : QResult, IQuelleFetchResult
    {
        public QResultFetch(): base() { /*for msgpack*/ }

        [DataMember]
        public UInt64 cursor { get; set; }
        [DataMember]
        public UInt64 remainder { get; set; }
        [DataMember]
        public Guid session { get; set; }
        [DataMember]
        public Dictionary<UInt64, string> records { get; set; }

        public QResultFetch(IQuelleFetchResult iresult) : base ((IQuelleResult) iresult)
        {
            this.cursor = iresult.cursor;
            this.remainder = iresult.remainder;
            this.session = iresult.session;
            this.records = iresult.records;
        }
    }
}

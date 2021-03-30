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
    public class QResultFetch: IQuelleFetchResult
    {
        public QResultFetch() { /*for serialization*/ }

        [DataMember]
        public byte[] session { get; set; } // MD5/GUID
        [DataMember]
        public Dictionary<UInt32, String> abstracts { get; set; }
        [DataMember]
        public UInt64 cursor { get; set; }
        [DataMember]
        public UInt64 count { get; set; }
        [DataMember]
        public UInt64 remainder { get; set; }
        [DataMember]
        public Dictionary<string, string> messages { get; set; }

        public QResultFetch(IQuelleFetchResult iresult)
        {
            this.abstracts = iresult.abstracts;
            this.cursor = iresult.cursor;
            this.count = iresult.count;
            this.remainder = iresult.remainder;
            this.session = iresult.session;
            this.messages = iresult.messages;
        }
    }
}

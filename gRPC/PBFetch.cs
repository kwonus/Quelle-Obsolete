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
    public class PBFetchRequest : IQuelleFetchRequest
    {
        [ProtoMember(1)]
        public Guid session { get; set; }
        [ProtoMember(2)]
        public UInt64 cursor { get; set; }
        [ProtoMember(3)]
        public UInt64 count { get; set; }

        public PBFetchRequest(IQuelleFetchRequest irequest)
        {
            this.session = irequest.session;
            this.cursor = irequest.cursor;
            this.count = irequest.count;
        }
    }

    [ProtoContract]
    public class PBFetchResult : PBQuelleResult, IQuelleFetchResult
    {
        public PBFetchResult(): base() { /*for protobuf*/ }

        [ProtoMember(4)]
        public UInt64 cursor { get; set; }
        [ProtoMember(5)]
        public UInt64 remainder { get; set; }
        [ProtoMember(6)]
        public Guid session { get; set; }
        [ProtoMember(7)]
        public Dictionary<UInt64, string> records { get; set; }

        public PBFetchResult(IQuelleFetchResult iresult) : base ((IQuelleResult) iresult)
        {
            this.cursor = iresult.cursor;
            this.remainder = iresult.remainder;
            this.session = iresult.session;
            this.records = iresult.records;
        }
    }
}

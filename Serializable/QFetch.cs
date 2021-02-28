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
    public class QFetchRequest : IQuelleFetchRequest
    {
        [Key(1)]
        public Guid session { get; set; }
        [Key(2)]
        public UInt64 cursor { get; set; }
        [Key(3)]
        public UInt64 count { get; set; }

        public QFetchRequest(IQuelleFetchRequest irequest)
        {
            this.session = irequest.session;
            this.cursor = irequest.cursor;
            this.count = irequest.count;
        }
    }

    [MessagePackObject]
    public class PBFetchResult : QResult, IQuelleFetchResult
    {
        public PBFetchResult(): base() { /*for protobuf*/ }

        [Key(4)]
        public UInt64 cursor { get; set; }
        [Key(5)]
        public UInt64 remainder { get; set; }
        [Key(6)]
        public Guid session { get; set; }
        [Key(7)]
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

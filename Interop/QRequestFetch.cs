using MessagePack;
using System;
using System.Collections.Generic;

namespace QuelleHMI
{
    [MessagePackObject]
    public class QRequestFetch: IQuelleFetchRequest
    {
        public QRequestFetch() : base() { /*for msgpack*/ }

        [Key(1)]
        public Guid session { get; set; }
        [Key(2)]
        public UInt64 cursor { get; set; }
        [Key(3)]
        public UInt64 count { get; set; }

        public QRequestFetch(IQuelleFetchRequest irequest)
        {
            this.session = irequest.session;
            this.cursor = irequest.cursor;
            this.count = irequest.count;
        }
    }

    [MessagePackObject]
    public class QResultFetch : QResult, IQuelleFetchResult
    {
        public QResultFetch(): base() { /*for msgpack*/ }

        [Key(4)]
        public UInt64 cursor { get; set; }
        [Key(5)]
        public UInt64 remainder { get; set; }
        [Key(6)]
        public Guid session { get; set; }
        [Key(7)]
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

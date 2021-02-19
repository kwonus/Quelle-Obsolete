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
    [DataContract]
    public class PBFetchRequest : IQuelleFetchRequest
    {
        [DataMember(Order = 1)]
        public Guid session { get; set; }
        [DataMember(Order = 2)]
        public UInt64 cursor { get; set; }
        [DataMember(Order = 3)]
        public UInt64 count { get; set; }
    }
    [DataContract]
    public class PBFetchResult : PBQuelleResult, IQuelleFetchResult
    {
        [DataMember(Order = 4)]
        public UInt64 cursor { get; set; }
        [DataMember(Order = 5)]
        public UInt64 remainder { get; set; }
        [DataMember(Order = 6)]
        public Guid session { get; set; }
        [DataMember(Order = 7)]
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

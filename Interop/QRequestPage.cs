using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace QuelleHMI
{
    [DataContract]
    public class QRequestPage: IQuellePageRequest
    {
        public QRequestPage(): base() { /*for serialization*/ }

        [DataMember]
        public Guid session { get; set; }
        [DataMember]
        public string format { get; set; }
        [DataMember]
        public UInt64 page { get; set; }

        public QRequestPage(IQuellePageRequest irequest)
        {
            this.session = irequest.session;
            this.format = irequest.format;
            this.page = irequest.page;
        }
    }
    [DataContract]
    public class QPageResult : IQuellePageResult
    {
        public QPageResult() { /*for serialization*/ }

        [DataMember]
        public string result { get; set; }
        [DataMember]
        public Dictionary<string, string> messages { get; set; }

        public QPageResult(IQuellePageResult iresult)
        {
            this.result = iresult.result;
            this.messages = iresult.messages;
        }
    }
}

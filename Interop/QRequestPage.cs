using System;
using System.Runtime.Serialization;

namespace QuelleHMI
{
    [DataContract]
    public class QRequestPage: IQuellePageRequest
    {
        public QRequestPage(): base() { /*for msgpack*/ }

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
    public class QPageResult : QResult, IQuellePageResult
    {
        public QPageResult() { /*for msgpack*/ }

        [DataMember]
        public string result { get; set; }
        [IgnoreDataMember]
        public IQuellePageRequest request
        {
            get => this.qRequest;
            set => this.qRequest = new QRequestPage(value);
        }
        [DataMember]
        public QRequestPage qRequest { get; set; }

        public QPageResult(IQuellePageResult iresult) : base((IQuelleResult)iresult)
        {
            this.result = iresult.result;
            this.qRequest = new QRequestPage(iresult.request);
        }
    }
}

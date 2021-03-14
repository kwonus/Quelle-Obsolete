using MessagePack;
using System;

namespace QuelleHMI
{
    [MessagePackObject]
    public class QRequestPage: IQuellePageRequest
    {
        public QRequestPage(): base() { /*for msgpack*/ }

        [Key(1)]
        public Guid session { get; set; }
        [Key(2)]
        public string format { get; set; }
        [Key(3)]
        public UInt64 page { get; set; }

        public QRequestPage(IQuellePageRequest irequest)
        {
            this.session = irequest.session;
            this.format = irequest.format;
            this.page = irequest.page;
        }
    }
    [MessagePackObject]
    public class QPageResult : QResult, IQuellePageResult
    {
        public QPageResult() { /*for msgpack*/ }

        [Key(4)]
        public string result { get; set; }
        [IgnoreMember]
        public IQuellePageRequest request
        {
            get => this.qRequest;
            set => this.qRequest = new QRequestPage(value);
        }
        [Key(5)]
        public QRequestPage qRequest { get; set; }

        public QPageResult(IQuellePageResult iresult) : base((IQuelleResult)iresult)
        {
            this.result = iresult.result;
            this.qRequest = new QRequestPage(iresult.request);
        }
    }
}

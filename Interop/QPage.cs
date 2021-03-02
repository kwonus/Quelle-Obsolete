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
    public class QPageRequest : IQuellePageRequest
    {
        public QPageRequest() { /*for protobuf*/ }

        [Key(1)]
        public Guid session { get; set; }
        [Key(2)]
        public string format { get; set; }
        [Key(3)]
        public UInt64 page { get; set; }

        public QPageRequest(IQuellePageRequest irequest)
        {
            this.session = irequest.session;
            this.format = irequest.format;
            this.page = irequest.page;
        }
    }
    [MessagePackObject]
    public class PBPageResult : QResult, IQuellePageResult
    {
        public PBPageResult() { /*for protobuf*/ }

        [Key(4)]
        public string result { get; set; }
        [IgnoreMember]
        public IQuellePageRequest request
        {
            get => this.pbRequest;
            set => this.pbRequest = new QPageRequest(value);
        }
        [Key(5)]
        public QPageRequest pbRequest { get; set; }

        public PBPageResult(IQuellePageResult iresult) : base((IQuelleResult)iresult)
        {
            this.result = iresult.result;
            this.pbRequest = new QPageRequest(iresult.request);
        }
    }
}


using System.Runtime.Serialization;

namespace QuelleHMI
{
    [DataContract]
    public class QResult : IQuelleResult
    {
        public QResult() { /*for msgpack*/ }

        [DataMember]
        public bool success { get; set; }
        [DataMember]
        public string[] errors { get; set; }
        [DataMember]
        public string[] warnings { get; set; }

        public QResult(IQuelleResult iresult)
        {
            this.success = iresult.success;
            this.errors = iresult.errors;
            this.warnings = iresult.warnings;
        }
    }
}

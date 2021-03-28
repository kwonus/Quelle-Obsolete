using QuelleHMI.Tokens;
using System.Runtime.Serialization;

namespace QuelleHMI
{
    [DataContract]
    public class QTokenMatch : IQuelleTokenMatch
    {
        public QTokenMatch() { /*for msgpack*/ }

        [DataMember]
        public string condition { get; set; }
        //[IgnoreMember]
        public IQuelleTokenFeature[] anyFeature
        {
            get => this.qAnyFeature;
            set
            {
                this.qAnyFeature = new QTokenFeature[value.Length];
                int i = 0;
                foreach (var frag in value)
                    this.qAnyFeature[i] = new QTokenFeature(value[i++]);
            }
        }
        [DataMember]
        public QTokenFeature[] qAnyFeature { get; set; }

        public QTokenMatch(IQuelleTokenMatch imatch)
        {
            this.condition = imatch.condition;
            this.qAnyFeature = imatch.anyFeature != null ? new QTokenFeature[imatch.anyFeature.Length] : null;
            for (int i = 0; i < imatch.anyFeature.Length; i++)
                this.qAnyFeature[i] = new QTokenFeature(imatch.anyFeature[i]);
        }
    }
}

using QuelleHMI.Tokens;
using System.Runtime.Serialization;

namespace QuelleHMI
{
    [DataContract]
    public class QTokenFeature : IQuelleTokenFeature
    {
        public QTokenFeature() { /*for msgpack*/ }

        [DataMember]
        public string feature { get; set; }

        public QTokenFeature(IQuelleTokenFeature ifeature)
        {
            this.feature = ifeature.feature;
        }
    }
}

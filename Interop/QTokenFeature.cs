using MessagePack;
using QuelleHMI.Tokens;

namespace QuelleHMI
{
    [MessagePackObject]
    public class QTokenFeature : IQuelleTokenFeature
    {
        public QTokenFeature() { /*for protobuf*/ }

        [Key(1)]
        public string feature { get; set; }

        public QTokenFeature(IQuelleTokenFeature ifeature)
        {
            this.feature = ifeature.feature;
        }
    }
}

using MessagePack;
using QuelleHMI.Tokens;

namespace QuelleHMI
{
    [MessagePackObject]
    public class QTokenMatch : IQuelleTokenMatch
    {
        public QTokenMatch() { /*for protobuf*/ }

        [Key(1)]
        public string condition { get; set; }
        [IgnoreMember]
        public IQuelleTokenFeature[] anyFeature
        {
            get => this.pbAnyFeature;
            set
            {
                this.pbAnyFeature = new QTokenFeature[value.Length];
                int i = 0;
                foreach (var frag in value)
                    this.pbAnyFeature[i] = new QTokenFeature(value[i++]);
            }
        }
        [Key(2)]
        public QTokenFeature[] pbAnyFeature { get; set; }

        public QTokenMatch(IQuelleTokenMatch imatch)
        {
            this.condition = imatch.condition;
            this.pbAnyFeature = imatch.anyFeature != null ? new QTokenFeature[imatch.anyFeature.Length] : null;
            for (int i = 0; i < imatch.anyFeature.Length; i++)
                this.pbAnyFeature[i] = new QTokenFeature(imatch.anyFeature[i]);
        }
    }
}

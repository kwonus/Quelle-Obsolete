using MessagePack;
using QuelleHMI.Tokens;

namespace QuelleHMI
{
    [MessagePackObject]
    public class QTokenVector : IQuelleTokenVector
    {
        public QTokenVector() { /*for protobuf*/ }

        [Key(1)]
        public string specification { get; set;  }
        [IgnoreMember]
        public IQuelleTokenMatch[] matchAll
        {
            get => this.pbMatchAll;
            set
            {
                this.pbMatchAll = new QTokenMatch[value.Length];
                int i = 0;
                foreach (var frag in value)
                    this.pbMatchAll[i] = new QTokenMatch(value[i++]);
            }
        }
        [Key(2)]
        public QTokenMatch[] pbMatchAll { get; set; }

        public QTokenVector(IQuelleTokenVector ivector)
        {
            this.specification = ivector.specification;

            this.pbMatchAll = ivector.matchAll != null ? new QTokenMatch[ivector.matchAll.Length] : null;
            if (this.pbMatchAll != null)
                for (int i = 0; i < ivector.matchAll.Length; i++)
                    this.pbMatchAll[i] = new QTokenMatch(ivector.matchAll[i]);
        }

    }
}

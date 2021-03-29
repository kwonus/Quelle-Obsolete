using QuelleHMI.Tokens;
using System.Runtime.Serialization;

namespace QuelleHMI
{
    [DataContract]
    public class QTokenVector : IQuelleTokenVector
    {
        public QTokenVector() { /*for msgpack*/ }

        [DataMember]
        public string specification { get; set;  }
        [IgnoreDataMember]
        public IQuelleTokenMatch[] matchAll
        {
            get => this.qMatchAll;
            set
            {
                this.qMatchAll = new QTokenMatch[value.Length];
                int i = 0;
                foreach (var frag in value)
                    this.qMatchAll[i] = new QTokenMatch(value[i++]);
            }
        }
        [DataMember]
        public QTokenMatch[] qMatchAll { get; set; }

        public QTokenVector(IQuelleTokenVector ivector)
        {
            this.specification = ivector.specification;

            this.qMatchAll = ivector.matchAll != null ? new QTokenMatch[ivector.matchAll.Length] : null;
            if (this.qMatchAll != null)
                for (int i = 0; i < ivector.matchAll.Length; i++)
                    this.qMatchAll[i] = new QTokenMatch(ivector.matchAll[i]);
        }

    }
}

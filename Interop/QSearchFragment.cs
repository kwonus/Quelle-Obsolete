using MessagePack;
using QuelleHMI.Fragments;
using QuelleHMI.Tokens;

namespace QuelleHMI
{
    [MessagePackObject]
    public class QSearchFragment : IQuelleSearchFragment
    {
        public QSearchFragment() { /*for msgpack*/ }

        [Key(1)]
        public uint[] positionAspects { get; set; }
        [IgnoreMember]
        public IQuelleTokenVector[] anyOf
        {
            get => this.qAnyOf;
            set
            {
                this.qAnyOf = new QTokenVector[value.Length];
                int i = 0;
                foreach (var frag in value)
                    this.qAnyOf[i] = new QTokenVector(value[i++]);
            }
        }
        [Key(2)]
        public QTokenVector[] qAnyOf { get; set;  }
        [Key(3)]
        public string text { get; set; }

        public QSearchFragment (IQuelleSearchFragment ifragment)
        {
            this.positionAspects = ifragment.positionAspects;
            this.qAnyOf = ifragment.anyOf != null ? new QTokenVector[ifragment.anyOf.Length] : null;
            this.text = ifragment.text;

            if (this.qAnyOf != null)
                for (int i = 0; i < ifragment.anyOf.Length; i++)
                    this.qAnyOf[i] = new QTokenVector(ifragment.anyOf[i]);
        }
    }
}

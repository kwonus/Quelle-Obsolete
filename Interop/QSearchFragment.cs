using MessagePack;
using QuelleHMI.Fragments;
using QuelleHMI.Tokens;

namespace QuelleHMI
{
    [MessagePackObject]
    public class QSearchFragment : IQuelleSearchFragment
    {
        public QSearchFragment() { /*for protobuf*/ }

        [Key(1)]
        public uint[] positionAspects { get; set; }
        [IgnoreMember]
        public IQuelleTokenVector[] anyOf
        {
            get => this.pbanyOf;
            set
            {
                this.pbanyOf = new QTokenVector[value.Length];
                int i = 0;
                foreach (var frag in value)
                    this.pbanyOf[i] = new QTokenVector(value[i++]);
            }
        }
        [Key(2)]
        public QTokenVector[] pbanyOf { get; set;  }
        [Key(3)]
        public string text { get; set; }

        public QSearchFragment (IQuelleSearchFragment ifragment)
        {
            this.positionAspects = ifragment.positionAspects;
            this.pbanyOf = ifragment.anyOf != null ? new QTokenVector[ifragment.anyOf.Length] : null;
            this.text = ifragment.text;

            if (this.pbanyOf != null)
                for (int i = 0; i < ifragment.anyOf.Length; i++)
                    this.pbanyOf[i] = new QTokenVector(ifragment.anyOf[i]);
        }
    }
}

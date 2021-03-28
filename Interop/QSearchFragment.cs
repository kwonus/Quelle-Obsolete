using QuelleHMI.Fragments;
using QuelleHMI.Tokens;
using System.Runtime.Serialization;

namespace QuelleHMI
{
    [DataContract]
    public class QSearchFragment : IQuelleSearchFragment
    {
        public QSearchFragment() { /*for msgpack*/ }

        [DataMember]
        public uint[] positionAspects { get; set; }
        //[IgnoreMember]
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
        [DataMember]
        public QTokenVector[] qAnyOf { get; set;  }
        [DataMember]
        public string text { get; set; }

        public QSearchFragment(IQuelleSearchFragment ifragment)
        {
            this.positionAspects = ifragment.positionAspects;
            this.qAnyOf = ifragment.anyOf != null ? new QTokenVector[ifragment.anyOf.Length] : null;
            this.text = HMIStatement.SquenchText(ifragment.text);

            if (this.qAnyOf != null)
                for (int i = 0; i < ifragment.anyOf.Length; i++)
                    this.qAnyOf[i] = new QTokenVector(ifragment.anyOf[i]);
        }
    }
}

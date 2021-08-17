using QuelleHMI.Fragments;
using QuelleHMI.Tokens;
using System.Runtime.Serialization;
using System;

namespace QuelleHMI
{
    [DataContract]
    public class QSearchFragment : IQuelleSearchFragment
    {
        private QSearchFragment() { /*for serialization ; make public when needed */ }

        [DataMember]
        public byte adjacency { get; protected set; }
        [DataMember]
        public byte bracketed { get; protected set; }
        private IQuelleFeatureSpec[] _specs { get; set; }
        [DataMember]
        public IQuelleFeatureSpec[] specifications
        {
            get
            {
                if (this._specs == null)
                {
                    var specs = this.text.Split(QuelleHMI.Fragment.whitespace, StringSplitOptions.RemoveEmptyEntries);
                    this._specs = new FeatureSpec[specs.Length];
                    int i = 0;
                    foreach (var spec in specs)
                    {
                        this._specs[i++] = new FeatureSpec(spec.Trim());
                    }
                }
                return this._specs;
            }
            set
            {
                this._specs = value;
            }
        }
        [DataMember]
        public string text { get; set; }

        [DataMember]
        public UInt64 bit { get; set; }
        [DataMember]
        public UInt16 span { get; set; }

        public QSearchFragment(IQuelleSearchFragment ifragment)
        {
            this.adjacency = ifragment.adjacency;
            this.bracketed = ifragment.bracketed;
            this._specs = ifragment.specifications;
            this.text = HMIStatement.SquenchText(ifragment.text);
            this.span = ifragment.span;
            this.bit = ifragment.bit;
        }
    }
}

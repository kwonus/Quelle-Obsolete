using QuelleHMI.Fragments;
using QuelleHMI.Tokens;
using System.Runtime.Serialization;
using System;

namespace QuelleHMI
{
    [DataContract]
    public class QSearchFragment : IQuelleSearchFragment
    {
        public QSearchFragment() { /*for serialization*/ }

        [DataMember]
        public byte adjacency { get; protected set; }
        [DataMember]
        public byte group { get; protected set; }
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

        public QSearchFragment(IQuelleSearchFragment ifragment)
        {
            this.adjacency = ifragment.adjacency;
            this.group = ifragment.group;
            this._specs = ifragment.specifications;
            this.text = HMIStatement.SquenchText(ifragment.text);
        }
    }
}

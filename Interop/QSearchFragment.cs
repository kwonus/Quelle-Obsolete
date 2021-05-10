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
        public byte AdjacencyOrAnchorage { get; set; }
        [DataMember]
        public UInt16 UnorderedSubgroupIndex { get; set; }
        [DataMember]
        public IQuelleFeatureSpec[] spec { get; set; }
        [DataMember]
        public string text { get; set; }

        public QSearchFragment(IQuelleSearchFragment ifragment)
        {
            this.AdjacencyOrAnchorage = ifragment.AdjacencyOrAnchorage;
            this.UnorderedSubgroupIndex = ifragment.UnorderedSubgroupIndex;
            this.spec = ifragment.spec;
            this.text = HMIStatement.SquenchText(ifragment.text);
        }
    }
}

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

        [IgnoreDataMember]
        public UInt32[] positionAspects { get; set; }
        [IgnoreDataMember]
        public IQuelleFeatureSpec[] spec { get; set; }
        [DataMember]
        public string text { get; set; }

        public QSearchFragment(IQuelleSearchFragment ifragment)
        {
            this.positionAspects = ifragment.positionAspects;
            this.spec = ifragment.spec;
            this.text = HMIStatement.SquenchText(ifragment.text);
        }
    }
}

using ProtoBuf;
using QuelleHMI.Controls;
using QuelleHMI.Fragments;
using QuelleHMI.Verbs;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace QuelleHMI
{
    [ProtoContract]
    public class PBSearchClause : IQuelleSearchClause
    {
        public PBSearchClause() { /*for protobuf*/ }

        [ProtoMember(1)]
        public string syntax { get; set; }
        [ProtoIgnore]
        public IQuelleSearchFragment[] fragments
        {
            get => this.pbfragments;
            set
            {
                this.pbfragments = new PBSearchFragment[value.Length];
                int i = 0;
                foreach (var frag in value)
                    this.pbfragments[i] = new PBSearchFragment(value[i++]);
            }
        }
        [ProtoMember(2)]
        public PBSearchFragment[] pbfragments { get; set; }
        [ProtoMember(3)]
        public string segment { get; set; }
        [ProtoMember(4)]
        public HMIClause.HMIPolarity polarity { get; }

        public PBSearchClause(IQuelleSearchClause iclause)
        {
            this.syntax = iclause.syntax;
            this.pbfragments = iclause.fragments != null ? new PBSearchFragment[iclause.fragments.Length] : null;
            this.segment = iclause.segment;
            this.polarity = iclause.polarity;

            if (this.pbfragments != null)
                for (int i = 0; i < iclause.fragments.Length; i++)
                    this.pbfragments[i] = new PBSearchFragment(iclause.fragments[i]);
        }
    }
}

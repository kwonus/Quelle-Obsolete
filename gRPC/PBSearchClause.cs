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
    [DataContract]
    public class PBSearchClause : IQuelleSearchClause
    {
        [DataMember(Order = 1)]
        public string syntax { get; set; }
        public IQuelleSearchFragment[] fragments
        {
            get => this.pbfragments;
        }
        [DataMember(Order = 2)]
        public PBSearchFragment[] pbfragments { get; set; }
        [DataMember(Order = 3)]
        public string segment { get; set; }
        [DataMember(Order = 4)]
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

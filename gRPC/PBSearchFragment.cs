using ProtoBuf;
using QuelleHMI.Controls;
using QuelleHMI.Fragments;
using QuelleHMI.Tokens;
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
    public class PBSearchFragment : IQuelleSearchFragment
    {
        public PBSearchFragment() { /*for protobuf*/ }

        [ProtoMember(1)]
        public uint[] positionAspects { get; set; }
        [ProtoIgnore]
        public IQuelleTokenVector[] anyOf
        {
            get => this.pbanyOf;
            set
            {
                this.pbanyOf = new PBTokenVector[value.Length];
                int i = 0;
                foreach (var frag in value)
                    this.pbanyOf[i] = new PBTokenVector(value[i++]);
            }
        }
        [ProtoMember(2)]
        public PBTokenVector[] pbanyOf { get; set;  }
        [ProtoMember(3)]
        public string text { get; set; }

        public PBSearchFragment (IQuelleSearchFragment ifragment)
        {
            this.positionAspects = ifragment.positionAspects;
            this.pbanyOf = ifragment.anyOf != null ? new PBTokenVector[ifragment.anyOf.Length] : null;
            this.text = ifragment.text;

            if (this.pbanyOf != null)
                for (int i = 0; i < ifragment.anyOf.Length; i++)
                    this.pbanyOf[i] = new PBTokenVector(ifragment.anyOf[i]);
        }
    }
}

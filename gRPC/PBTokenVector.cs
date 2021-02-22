using ProtoBuf;
using QuelleHMI.Controls;
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
    public class PBTokenVector : IQuelleTokenVector
    {
        public PBTokenVector() { /*for protobuf*/ }

        [ProtoMember(1)]
        public string specification { get; set;  }
        [ProtoIgnore]
        public IQuelleTokenMatch[] matchAll
        {
            get => this.pbMatchAll;
            set
            {
                this.pbMatchAll = new PBTokenMatch[value.Length];
                int i = 0;
                foreach (var frag in value)
                    this.pbMatchAll[i] = new PBTokenMatch(value[i++]);
            }
        }
        [ProtoMember(2)]
        public PBTokenMatch[] pbMatchAll { get; set; }

        public PBTokenVector(IQuelleTokenVector ivector)
        {
            this.specification = ivector.specification;

            this.pbMatchAll = ivector.matchAll != null ? new PBTokenMatch[ivector.matchAll.Length] : null;
            if (this.pbMatchAll != null)
                for (int i = 0; i < ivector.matchAll.Length; i++)
                    this.pbMatchAll[i] = new PBTokenMatch(ivector.matchAll[i]);
        }

    }
}

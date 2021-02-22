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
    public class PBTokenMatch : IQuelleTokenMatch
    {
        public PBTokenMatch() { /*for protobuf*/ }

        [ProtoMember(1)]
        public string condition { get; set; }
        [ProtoIgnore]
        public IQuelleTokenFeature[] anyFeature
        {
            get => this.pbAnyFeature;
            set
            {
                this.pbAnyFeature = new PBTokenFeature[value.Length];
                int i = 0;
                foreach (var frag in value)
                    this.pbAnyFeature[i] = new PBTokenFeature(value[i++]);
            }
        }
        [ProtoMember(2)]
        public PBTokenFeature[] pbAnyFeature { get; set; }

        public PBTokenMatch(IQuelleTokenMatch imatch)
        {
            this.condition = imatch.condition;
            this.pbAnyFeature = imatch.anyFeature != null ? new PBTokenFeature[imatch.anyFeature.Length] : null;
            for (int i = 0; i < imatch.anyFeature.Length; i++)
                this.pbAnyFeature[i] = new PBTokenFeature(imatch.anyFeature[i]);
        }
    }
}

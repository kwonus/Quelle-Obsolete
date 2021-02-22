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
    public class PBTokenFeature : IQuelleTokenFeature
    {
        public PBTokenFeature() { /*for protobuf*/ }

        [ProtoMember(1)]
        public string feature { get; set; }

        public PBTokenFeature(IQuelleTokenFeature ifeature)
        {
            this.feature = ifeature.feature;
        }
    }
}

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
    [DataContract]
    public class PBTokenMatch : IQuelleTokenMatch
    {
        [DataMember(Order = 1)]
        public string condition { get; set; }
        public IQuelleTokenFeature[] anyFeature
        {
            get => this.pbAnyFeature;
        }
        [DataMember(Order = 2)]
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

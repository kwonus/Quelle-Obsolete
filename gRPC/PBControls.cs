using QuelleHMI.Controls;
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
    public class PBSearchControls: IQuelleSearchControls
    {
        [DataMember(Order = 1)]
        public string domain { get; set; }
        [DataMember(Order = 2)]
        public int span { get; set; }
        [DataMember(Order = 3)]
        public int strict { get; set; }

        public PBSearchControls(IQuelleSearchControls icontrols)
        {
            this.domain = icontrols.domain;
            this.span = icontrols.span;
            this.strict = icontrols.strict;
        }
    }
}

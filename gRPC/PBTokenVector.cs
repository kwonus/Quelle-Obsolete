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
    public class PBTokenVector : IQuelleTokenVector
    {
        [DataMember(Order = 1)]
        public string specification { get; set;  }
        public IQuelleTokenMatch[] matchAll
        {
            get => this.pbMatchAll;
        }
        [DataMember(Order = 2)]
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

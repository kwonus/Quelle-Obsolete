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
    public class PBQuelleResult : IQuelleResult
    {
        [DataMember(Order = 1)]
        public bool success { get; set; }
        [DataMember(Order = 2)]
        public string[] errors { get; set; }
        [DataMember(Order = 3)]
        public string[] warnings { get; set; }

        public PBQuelleResult(IQuelleResult iresult)
        {
            this.success = iresult.success;
            this.errors = iresult.errors;
            this.warnings = iresult.warnings;
        }
    }
}

using MessagePack;
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
    [MessagePackObject]
    public class QTokenFeature : IQuelleTokenFeature
    {
        public QTokenFeature() { /*for protobuf*/ }

        [Key(1)]
        public string feature { get; set; }

        public QTokenFeature(IQuelleTokenFeature ifeature)
        {
            this.feature = ifeature.feature;
        }
    }
}

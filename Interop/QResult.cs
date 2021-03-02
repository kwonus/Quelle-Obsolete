using MessagePack;
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
    [MessagePackObject]
    public class QResult : IQuelleResult
    {
        public QResult() { /*for protobuf*/ }

        [Key(1)]
        public bool success { get; set; }
        [Key(2)]
        public string[] errors { get; set; }
        [Key(3)]
        public string[] warnings { get; set; }

        public QResult(IQuelleResult iresult)
        {
            this.success = iresult.success;
            this.errors = iresult.errors;
            this.warnings = iresult.warnings;
        }
    }
}

using QuelleHMI.Definitions;
using System.Runtime.Serialization;

namespace QuelleHMI
{
    [DataContract]
    public class QSearchControls: IQuelleSearchControls
    {
        public QSearchControls(): base() { /*for serialization*/ }

        public string host { get; set; }
        public string domain { get; set; }
        public uint? span { get; set; }
        public bool? exact { get; set; }

        [DataMember]
        public string Host
        {
            get => host;
            set => host = value;
        }
        [DataMember]
        public string Domain
        {
            get => domain;
            set => domain = value;
        }
        [DataMember]
        public uint Span
        {
            get => span.HasValue ? span.Value : CTLSearch.defaultSpan;
            set => span = value;
        }
        [DataMember]
        public bool Exact
        {
            get => exact.HasValue ? exact.Value : CTLSearch.defaultExact;
            set => exact = value;
        }
        public QSearchControls(IQuelleSearchControls icontrols)
        {
            this.host   = icontrols.host;
            this.domain = icontrols.domain;
            this.span   = icontrols.span.Value;
            this.exact  = icontrols.exact.Value;
        }
    }
}

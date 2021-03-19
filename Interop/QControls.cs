using MessagePack;
using QuelleHMI.Definitions;

namespace QuelleHMI
{
    [MessagePackObject]
    public class QSearchControls: IQuelleSearchControls
    {
        public QSearchControls(): base() { /*for msgpack*/ }

        public string host { get; set; }
        public string domain { get; set; }
        public uint? span { get; set; }
        public bool? exact { get; set; }

        [Key(1)]
        public string Host
        {
            get => host;
            set => host = value;
        }
        [Key(2)]
        public string Domain
        {
            get => domain;
            set => domain = value;
        }
        [Key(3)]
        public uint Span
        {
            get => span.HasValue ? span.Value : CTLSearch.defaultSpan;
            set => span = value;
        }
        [Key(4)]
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

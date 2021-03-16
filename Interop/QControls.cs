using MessagePack;
using QuelleHMI.Definitions;

namespace QuelleHMI
{
    [MessagePackObject]
    public class QSearchControls: IQuelleSearchControls
    {
        public QSearchControls(): base() { /*for msgpack*/ }

        [Key(1)]
        public string domain { get; set; }
        [Key(2)]
        public uint span { get; set; }
        [Key(3)]
        public bool exact { get; set; }

        public QSearchControls(IQuelleSearchControls icontrols)
        {
            this.domain = icontrols.domain;
            this.span = icontrols.span;
            this.exact = icontrols.exact;
        }
    }
}

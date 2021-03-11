using MessagePack;
using QuelleHMI.Controls;

namespace QuelleHMI
{
    [MessagePackObject]
    public class QSearchControls: IQuelleSearchControls
    {
        public QSearchControls() { /*for protobuf*/ }

        [Key(1)]
        public string domain { get; set; }
        [Key(2)]
        public int span { get; set; }
        [Key(3)]
        public int strict { get; set; }

        public QSearchControls(IQuelleSearchControls icontrols)
        {
            this.domain = icontrols.domain;
            this.span = icontrols.span;
            this.strict = icontrols.strict;
        }
    }
}

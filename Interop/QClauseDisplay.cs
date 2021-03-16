using MessagePack;
using QuelleHMI.Fragments;
using QuelleHMI.Actions;

namespace QuelleHMI
{
    [MessagePackObject]
    public class QClauseDisplay : IQuelleDisplayClause
    {
        public QClauseDisplay() { /*for msgpack*/ }

        [Key(1)]
        public string[] specification { get; set; }

        public QClauseDisplay(IQuelleDisplayClause iclause)
        {
            this.specification = iclause.specification;
        }
    }
}

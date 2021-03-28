using QuelleHMI.Fragments;
using QuelleHMI.Actions;
using System.Runtime.Serialization;

namespace QuelleHMI
{
    [DataContract]
    public class QClauseDisplay : IQuelleDisplayClause
    {
        public QClauseDisplay() { /*for msgpack*/ }

        [DataMember]
        public string[] specification { get; set; }

        public QClauseDisplay(IQuelleDisplayClause iclause)
        {
            this.specification = iclause.specification;
        }
    }
}

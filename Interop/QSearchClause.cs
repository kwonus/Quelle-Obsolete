using MessagePack;
using QuelleHMI.Fragments;
using QuelleHMI.Verbs;

namespace QuelleHMI
{
    [MessagePackObject]
    public class QSearchClause : IQuelleSearchClause
    {
        public QSearchClause() { /*for protobuf*/ }

        [Key(1)]
        public string syntax { get; set; }
        [IgnoreMember]
        public IQuelleSearchFragment[] fragments
        {
            get => this.pbfragments;
            set
            {
                this.pbfragments = new QSearchFragment[value.Length];
                int i = 0;
                foreach (var frag in value)
                    this.pbfragments[i] = new QSearchFragment(value[i++]);
            }
        }
        [Key(2)]
        public QSearchFragment[] pbfragments { get; set; }
        [Key(3)]
        public string segment { get; set; }
        [Key(4)]
        public HMIClause.HMIPolarity polarity { get; }

        public QSearchClause(IQuelleSearchClause iclause)
        {
            this.syntax = iclause.syntax;
            this.pbfragments = iclause.fragments != null ? new QSearchFragment[iclause.fragments.Length] : null;
            this.segment = iclause.segment;
            this.polarity = iclause.polarity;

            if (this.pbfragments != null)
                for (int i = 0; i < iclause.fragments.Length; i++)
                    this.pbfragments[i] = new QSearchFragment(iclause.fragments[i]);
        }
    }
}

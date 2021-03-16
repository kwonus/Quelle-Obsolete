using MessagePack;
using QuelleHMI.Fragments;
using QuelleHMI.Actions;

namespace QuelleHMI
{
    [MessagePackObject]
    public class QClauseSearch : IQuelleSearchClause
    {
        public QClauseSearch() { /*for msgpack*/ }

        [IgnoreMember]
        public IQuelleSearchFragment[] fragments
        {
            get => this.qfragments;
            set
            {
                this.qfragments = new QSearchFragment[value.Length];
                int i = 0;
                foreach (var frag in value)
                    this.qfragments[i] = new QSearchFragment(value[i++]);
            }
        }
        [Key(1)]
        public QSearchFragment[] qfragments { get; set; }
        [Key(2)]
        public string segment { get; set; }
        [Key(3)]
        public char polarity { get; }

        public QClauseSearch(IQuelleSearchClause iclause)
        {
            this.qfragments = iclause.fragments != null ? new QSearchFragment[iclause.fragments.Length] : null;
            this.segment = HMIStatement.SquenchText(iclause.segment);
            this.polarity = iclause.polarity;

            if (this.qfragments != null)
                for (int i = 0; i < iclause.fragments.Length; i++)
                    this.qfragments[i] = new QSearchFragment(iclause.fragments[i]);
        }
        public QClauseSearch(Search hclause)
        {
            int cnt = hclause.fragments != null ? hclause.fragments.Length : 0;
            this.qfragments = cnt > 0 ? new QSearchFragment[cnt] : null;

            for (int i = 0; i < cnt; i++)
                this.qfragments[i] = new QSearchFragment(hclause.fragments[i]);
            this.segment = hclause.segment;
            this.polarity = hclause.polarity;
        }
    }
}

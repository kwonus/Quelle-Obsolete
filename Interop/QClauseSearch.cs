using QuelleHMI.Fragments;
using QuelleHMI.Actions;
using System.Runtime.Serialization;

namespace QuelleHMI
{
    [DataContract]
    public class QClauseSearch
    {
        public QClauseSearch() { /*for serialization*/ }

        [DataMember]
        public QSearchFragment[] fragments { get; set; }
        [DataMember]
        public string segment { get; set; }
        [DataMember]
        public byte polarity { get; set; }

        public QClauseSearch(IQuelleSearchClause iclause)
        {
            if (iclause != null)
            {
                this.segment = HMIStatement.SquenchText(iclause.segment);
                this.polarity = iclause.polarity;

                this.fragments = iclause.fragments != null ? new QSearchFragment[iclause.fragments.Length] : null;
                if (this.fragments != null)
                {
                    int i = 0;
                    foreach (var f in iclause.fragments)
                    {
                        this.fragments[i] = new QSearchFragment();
                        this.fragments[i].text = f.text;
                        i++;
                    }
                }
            }
        }
        public QClauseSearch(Search hclause)
        {
            if (hclause != null)
            {
                this.segment = HMIStatement.SquenchText(hclause.segment);
                this.polarity = hclause.polarity;

                this.fragments = hclause.fragments != null ? new QSearchFragment[hclause.fragments.Length] : null;
                if (this.fragments != null)
                {
                    int i = 0;
                    foreach (var f in hclause.fragments)
                    {
                        this.fragments[i] = new QSearchFragment(f);
                        i++;
                    }
                }
            }
        }
    }
}

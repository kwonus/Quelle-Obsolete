using QuelleHMI.Fragments;
using QuelleHMI.Actions;
using System.Runtime.Serialization;
using System;

namespace QuelleHMI
{
    [DataContract]
    public class QClauseSearch: IQuelleSearchClause
    {
        public QClauseSearch() { /*for serialization*/ }

        [DataMember]
        public IQuelleSearchFragment[] fragments { get; set; }
        [DataMember]
        public string segment { get; set; }
        [DataMember]
        public byte polarity { get; set; }
        [DataMember]
        public bool quoted { get; set; }
        [DataMember]
        public UInt16 index { get; set; }

        public QClauseSearch(IQuelleSearchClause iclause)
        {
            if (iclause != null)
            {
                this.segment = HMIStatement.SquenchText(iclause.segment);
                this.polarity = iclause.polarity;
                this.quoted = iclause.quoted;

                this.fragments = iclause.fragments != null ? new QSearchFragment[iclause.fragments.Length] : null;
                if (this.fragments != null)
                {
                    int i = 0;
                    foreach (var f in iclause.fragments)
                    {
                        this.fragments[i] = new QSearchFragment();
                        ((QSearchFragment)this.fragments[i]).text = f.text;
                        i++;
                    }
                }
            }
        }
        public QClauseSearch(Search hclause, UInt16 idx)
        {
            if (hclause != null)
            {
                this.index = idx;

                this.segment = HMIStatement.SquenchText(hclause.segment);
                this.polarity = hclause.polarity;
                this.quoted = hclause.quoted;

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
            else
            {
                this.index = 0;
            }
        }
    }
}

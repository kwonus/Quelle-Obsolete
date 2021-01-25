using System;
using System.Collections.Generic;
using System.Text;

namespace QuelleHMI
{
    public class HMIPrintClause : HMIDependentClause
    {
        public HMIPrintClause(HMIPhrase subordinate) : base(subordinate)
        {
            this.directive = HMIPhrase.DISPLAY;
        }
    }
}

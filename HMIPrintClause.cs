using System;
using System.Collections.Generic;
using System.Text;

namespace QuelleHMI
{
    public class HMIPrintClause : HMIDependentClause
    {
        public HMIPrintClause(HMISegment subordinate) : base(subordinate)
        {
            this.directive = HMISegment.DISPLAY;
        }
    }
}

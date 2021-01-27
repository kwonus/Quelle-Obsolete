using System;
using System.Collections.Generic;
using System.Text;

namespace QuelleHMI
{
    public class HMIPrintClause : HMIDependentClause
    {
        public HMIPrintClause(HMIClause subordinate) : base(subordinate)
        {
            this.directive = HMIClause.DISPLAY;
        }
    }
}

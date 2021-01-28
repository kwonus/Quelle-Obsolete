using System;
using System.Collections.Generic;
using System.Text;

namespace QuelleHMI.Verbs
{
    public class Set : HMIClause
    {
        public const string VERB = "set";

        public Set(HMIStatement statement, UInt32 segmentOrder, string segment)
    : base(statement, segmentOrder, HMIPolarity.UNDEFINED, segment, HMIClauseType.ORDINARY)
        {
            this.maximumScope = HMIScope.System;
            this.verb = VERB;
        }
        protected override bool Parse()
        {
            throw new NotImplementedException();
        }
    }
}

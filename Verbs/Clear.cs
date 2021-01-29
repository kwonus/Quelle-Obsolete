using System;
using System.Collections.Generic;
using System.Text;

namespace QuelleHMI.Verbs
{
    public class Clear: HMIClause
    {
        public const string VERB = "@clear";
        public Clear(HMIStatement statement, UInt32 segmentOrder, string segment)
    : base(statement, segmentOrder, HMIPolarity.UNDEFINED, segment, HMIClauseType.ORDINARY)
        {
            this.maximumScope = HMIScope.System;
            this.verb = VERB;
        }
        protected override bool Parse()
        {
            throw new NotImplementedException();
        }
        public override bool Execute()
        {
            throw new NotImplementedException();
        }
    }
}

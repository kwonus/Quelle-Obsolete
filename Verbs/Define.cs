using System;
using System.Collections.Generic;
using System.Text;

namespace QuelleHMI.Verbs
{
    public class Define : HMIClause
    {
        public const string VERB = "@define";
        public string macroName { get; private set; }
        public HMIScope macroScope { get; private set; }

        public Define(HMIStatement statement, UInt32 segmentOrder, string segment)
    : base(statement, segmentOrder, HMIPolarity.UNDEFINED, segment, HMIClauseType.DEPENDENT)
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
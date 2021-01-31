using System;
using System.Collections.Generic;
using System.Text;

namespace QuelleHMI.Verbs
{
    public class Exit : HMIClause
    {
        public const string SYNTAX = "SYSTEM";
        public override string syntax { get => SYNTAX; }
        public const string VERB = "@exit";

        public Exit(HMIStatement statement, string segment)
        : base(statement, 1, HMIPolarity.UNDEFINED, segment, HMIClauseType.SIMPLE)
        {
            this.maximumScope = HMIScope.Undefined;
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

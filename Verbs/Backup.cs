using System;
using System.Collections.Generic;
using System.Text;

namespace QuelleHMI.Verbs
{
    public class Backup : HMIClause
    {
        public const string VERB = "@backup";
        public Backup(HMIStatement statement, string segment)
    : base(statement, 1, HMIPolarity.UNDEFINED, segment, HMIClauseType.SIMPLE)
        {
            this.maximumScope = HMIScope.Undefined;
            this.verb = VERB;
        }
        protected override bool Parse()
        {
            throw new NotImplementedException();
        }
    }
}

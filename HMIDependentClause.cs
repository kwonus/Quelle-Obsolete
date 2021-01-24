using System;
using System.Collections.Generic;
using System.Text;

namespace QuelleHMI
{
    public class HMIDependentClause
    {
        public HMISegment subordinate;

        public string directive { get; protected set; }

        private HMIDependentClause()
        {
            this.subordinate = null;
            this.directive = null;
        }
        protected HMIDependentClause(HMISegment subordinate)
        {
            this.subordinate = subordinate;
            this.directive = null;
        }

        public static HMIDependentClause Create(HMIStatement statement, string segment)    // FACTORY
        {
            HMISegment subordinate = new HMISegment(statement, 1, 0, HMISegment.HMIPolarity.UNDEFINED, segment, HMISegment.HMIClauseType.SUBORDINATE);
            var info = HMISegment.IsVerb(segment);
 
            if (info.type == HMISegment.HMIClauseType.SUBORDINATE)
            {
                if (info.directive == HMISegment.MACRODEF)
                    return new HMIMacroDefintion(subordinate);

                else if (info.directive == HMISegment.DISPLAY)
                    return new HMIPrintClause(subordinate);
            }
            var verb = info.verb != null ? "the verb '" + info.verb + "' is not allowed on depenent clauses." : "no verb was found.";
            statement.Notify("error", "A dependent clause was identified, but " + verb);
            return new HMIDependentClause(subordinate);
        }
    }
}

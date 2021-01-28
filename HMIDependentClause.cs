using System;
using System.Collections.Generic;
using System.Text;

namespace QuelleHMI
{
    public class HMIDependentClause
    {
        public HMIClause subordinate;

        public string directive { get; protected set; }

        private HMIDependentClause()
        {
            this.subordinate = null;
            this.directive = null;
        }
        protected HMIDependentClause(HMIClause subordinate)
        {
            this.subordinate = subordinate;
            this.directive = null;
        }

        public static HMIDependentClause Create(HMIStatement statement, string segment)    // FACTORY
        {
            HMIClause subordinate = new HMIClause(statement, 1, HMIClause.HMIPolarity.UNDEFINED, segment, HMIClause.HMIClauseType.DEPENDENT);
            var info = HMIClause.IsVerb(subordinate.verb);
 
            if (info.type.IsDependent())
            {
                if (info.directive == HMIClause.MACRODEF)
                    return new HMIMacroDefintion(subordinate);

                else if (info.directive == HMIClause.DISPLAY)
                    return new HMIPrintClause(subordinate);
            }
            var verb = info.verb != null ? "the verb '" + info.verb + "' is not allowed on depenent clauses." : "no verb was found.";
            statement.Notify("error", "A dependent clause was identified, but " + verb);
            return new HMIDependentClause(subordinate);
        }
    }
}

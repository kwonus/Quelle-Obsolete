using System;
using System.Collections.Generic;
using System.Text;

namespace QuelleHMI
{
    public class HMIDependentClause
    {
        public HMIPhrase subordinate;

        public string directive { get; protected set; }

        private HMIDependentClause()
        {
            this.subordinate = null;
            this.directive = null;
        }
        protected HMIDependentClause(HMIPhrase subordinate)
        {
            this.subordinate = subordinate;
            this.directive = null;
        }

        public static HMIDependentClause Create(HMIStatement statement, string segment)    // FACTORY
        {
            HMIPhrase subordinate = new HMIPhrase(statement, 1, 0, HMIPhrase.HMIPolarity.UNDEFINED, segment, HMIPhrase.HMIClauseType.SUBORDINATE);
            var info = HMIPhrase.IsVerb(segment);
 
            if (info.type == HMIPhrase.HMIClauseType.SUBORDINATE)
            {
                if (info.directive == HMIPhrase.MACRODEF)
                    return new HMIMacroDefintion(subordinate);

                else if (info.directive == HMIPhrase.DISPLAY)
                    return new HMIPrintClause(subordinate);
            }
            var verb = info.verb != null ? "the verb '" + info.verb + "' is not allowed on depenent clauses." : "no verb was found.";
            statement.Notify("error", "A dependent clause was identified, but " + verb);
            return new HMIDependentClause(subordinate);
        }
    }
}

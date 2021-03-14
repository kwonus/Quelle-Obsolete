using System;
using System.Collections.Generic;
using System.Text;

namespace QuelleHMI.Verbs
{
    public class Status : HMIClause
    {
        public const string SYNTAX = "SYSTEM";
        public override string syntax { get => SYNTAX; }
        public const string VERB = "@status";
        public string[] parameters;

        public Status(HMIStatement statement, UInt32 segmentOrder, string segment)
    : base(statement, segmentOrder, HMIPolarity.UNDEFINED, segment, HMIClauseType.EXPLICIT_INDEPENDENT)
        {
            this.verb = Show.VERB;
        }
        protected override bool Parse()
        {
            return true;
 //         throw new NotImplementedException();
        }
        public override bool Execute()
        {
            return true;
//          throw new NotImplementedException();
        }
        public static string Help()
        {
            return "";
        }
    }
}
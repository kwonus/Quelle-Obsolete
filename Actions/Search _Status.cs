using QuelleHMI.Fragments;
using System;
using System.Collections.Generic;
using System.Linq;

namespace QuelleHMI.Actions
{
    public class Search_Status : Actions.Action
    {
        public const string SYNTAX = "SEARCH";
        public override string syntax { get => SYNTAX; }
        public const string STATUS = "@status";

        public static readonly List<string> EXPLICIT = new List<string>() { STATUS };

        public override bool Parse()
        {
           return true;
        }
        public override bool Execute()
        {
            return true;
        }

        public Search_Status(HMIStatement statement, HMIPolarity polarity, string segment)
        : base(statement, HMIClauseType.EXPLICIT, 0, segment)
        {
            this.verb = STATUS;
        }
        public static string Help()
        {
            return Search.Help(STATUS);
        }
    }
}

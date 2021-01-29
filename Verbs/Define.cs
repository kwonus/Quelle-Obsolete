using System;
using System.Collections.Generic;
using System.Text;

namespace QuelleHMI.Verbs
{
    public class Define : HMIClause
    {
        public const string VERB = "@define";
        public string macroName { get; private set; }
        public string macroValue { get => this.statement.statement; }
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
        public override bool Execute()
        {
            if (this.errors.Count == 0)
            {
                var result = HMICommand.Driver.Write("quelle.macro." + this.macroName, this.macroScope, this.macroValue);
                if (result.errors != null)
                {
                    foreach (var error in result.errors)
                        this.errors.Add(error);
                }
                else if (!result.success)
                {
                    this.errors.Add("Unspecific macro error; Please contact vendor about this Quelle driver implementation");
                }
                if (result.warnings != null)
                {
                    foreach (var warnings in result.warnings)
                        this.warnings.Add(warnings);
                }
            }
            return (this.errors.Count == 0);
        }
    }
}
using System;
using System.Collections.Generic;
using System.Text;

namespace QuelleHMI
{
    public class HMIMacroDefintion: HMIDependentClause
    {
        public string macroName { get; private set; }
        public HMIScope macroScope { get; private set; }

        public HMIMacroDefintion(HMIPhrase subordinate): base(subordinate)
        {
            this.directive = HMIPhrase.MACRODEF;
            this.macroScope = this.subordinate.verb != null && this.subordinate.verb.Length >= 2
                            ? this.subordinate.verb[0] == '#' ? HMIScope.System : HMIScope.Session
                            : HMIScope.Undefined;
        }
    }
}

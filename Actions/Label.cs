using QuelleHMI.Definitions;
using System;
using System.Collections.Generic;
using System.Text;

namespace QuelleHMI.Actions
{
    public class Label : Action
    {
        public const string SYNTAX = "LABEL";
        public override string syntax { get => SYNTAX; }
        public const string DELETE = "@delete";
        public const string SAVE = "@save";
        public const string SHOW = "@show";
        public const string EXPAND = "execute";

        public static readonly List<string> EXPLICIT = new List<string>() { DELETE, SAVE, SHOW };
        public static readonly List<string> IMPLICIT = new List<string>() { EXPAND };

        public string macroName { get; private set; }
        public string macroValue { get => this.statement.statement; }


        public Label(HMIStatement statement, string segment)
        : base(statement, HMIClauseType.EXPLICIT, 0, segment)
        {
            if (this.verb == null)
                this.verb = this.syntax;
        }
        public override bool Parse()
        {
            throw new NotImplementedException();
        }
        public override bool Execute()
        {
            if (this.errors.Count == 0)
            {
                var result = new Macro(this.macroName, this.statement);
                if (result == null)
                {
                    this.errors.Add("Could not create macro");
                }
            }
            return (this.errors.Count == 0);
        }
        public static string Help(string topic)
        {
            return "";
        }
    }
}
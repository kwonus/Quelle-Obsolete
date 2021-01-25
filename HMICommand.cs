using System;
using System.Collections.Generic;
using System.Linq;

namespace QuelleHMI
{
    public class HMICommand
    {
		public HMIStatement statement { get; private set; }
		public HMIDependentClause dependentClause { get; private set; }
		public string command { get; private set; }
		public List<string> errors { get; private set; }
		public List<string> warnings { get; private set; }
        public object Directives { get; private set; }

        public void Notify(string level, string message)
        {
			if (level != null && level.Equals("warning", StringComparison.InvariantCultureIgnoreCase))
            {
				if (warnings == null)
					warnings = new List<string>();
				warnings.Add(message.Trim());
			}
            else
            {
				if (errors == null)
					errors = new List<string>();
				errors.Add(message.Trim());
			}
		}
		public HMICommand(String command)
        {
			this.command = command.Trim();
			this.dependentClause = null;

			if (command != null)
			{
				string statement = "";

				int pipe;
				int start;
				//	Look for dependent clauses first:
				//
				for (start = 0, pipe = command.IndexOf('|', start); pipe >= start; pipe = command.IndexOf('|', start))
                {
					if (pipe - 1 >= 0 && command[pipe - 1] == '\\') // pipe-symbol is quoted, keep looking ...
						start = pipe + 1;
					else
						break;

					if (start >= command.Length)
                    {
						pipe = (-1); // not found
						break;
                    }
                }
				if (pipe >= 0 && pipe + 1 < command.Length)
				{
					statement = (pipe > 0) ? command.Substring(0, pipe).Trim() : "";

					this.statement = (statement.Length > 0) ? new HMIStatement(this, statement) : null;

					if (this.statement != null)
					{
						this.dependentClause = HMIDependentClause.Create(this.statement, command.Substring(pipe + 1).Trim());
					}
				}
				else if (this.errors == null)
                {
					this.statement = new HMIStatement(this, command.Trim());
				}
			}
        }

		public HMIScope HasMacro()
		{
			if (this.dependentClause != null && this.dependentClause.directive == HMIPhrase.MACRODEF)
				return ((HMIMacroDefintion)this.dependentClause).macroScope;

			return HMIScope.Undefined;
		}
	}
}

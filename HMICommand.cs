using System;
using System.Collections.Generic;
using System.Linq;

namespace QuelleHMI
{
    public class HMICommand
    {
		public HMIStatement statement { get; private set; }
		public HMIClause dependentClause { get; private set; }
		public string command { get; private set; }
		public List<string> errors { get; private set; }
		public List<string> warnings { get; private set; }
		public static IQuelleDriver Driver { get; protected set; } = null;

		public void Notify(string level, string message)
        {
			if (level != null && level.Equals("warning", StringComparison.InvariantCultureIgnoreCase))
            {
				warnings.Add(message.Trim());
			}
            else
            {
				errors.Add(message.Trim());
			}
		}
		public static void Intitialize(IQuelleDriver driver)
        {
			HMICommand.Driver = driver;
        }
		public HMICommand(String command)
        {
			this.command = command.Trim();
			this.dependentClause = null;
			this.warnings = new List<string>();
			this.errors = new List<string>();

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
						;// this.dependentClause = HMIDependentClause.Create(this.statement, command.Substring(pipe + 1).Trim());
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
			if (this.dependentClause != null && this.dependentClause.verb == Verbs.Define.VERB)
				return ((Verbs.Define)this.dependentClause).macroScope;

			return HMIScope.Undefined;
		}
		public Verbs.Define GetMacroSubordinate()
		{
			if (this.dependentClause != null && this.dependentClause.verb == Verbs.Define.VERB)
				return (Verbs.Define)this.dependentClause;

			return null;
		}
		public Verbs.Print GetPrintSubordinate()
		{
			if (this.dependentClause != null && this.dependentClause.verb == Verbs.Print.VERB)
				return (Verbs.Print)this.dependentClause;

			return null;
		}
		public bool Search()
        {
			return false;	// This will call into cloud
        }
	}
}

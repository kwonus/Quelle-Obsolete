using System;
using System.Collections.Generic;
using System.Linq;

namespace QuelleHMI
{
    public class HMICommand
    {
		public HMIStatement statement { get; private set; }
		public HMIClause explicitClause { get; private set; }
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
			this.explicitClause = null;
			this.warnings = new List<string>();
			this.errors = new List<string>();

			if (command != null)
			{
				this.statement = new HMIStatement(this, command.Trim());
			}
        }

		public HMIScope HasMacro()
		{
			if (this.explicitClause != null && this.explicitClause.verb == Verbs.Define.VERB)
				return ((Verbs.Define)this.explicitClause).macroScope;

			return HMIScope.Undefined;
		}
		public Verbs.Define GetMacroDefinition()
		{
			if (this.explicitClause != null && this.explicitClause.verb == Verbs.Define.VERB)
				return (Verbs.Define)this.explicitClause;

			return null;
		}
		public Verbs.Print GetPrintClause()
		{
			if (this.explicitClause != null && this.explicitClause.verb == Verbs.Print.VERB)
				return (Verbs.Print)this.explicitClause;

			return null;
		}
		public bool Search()
        {
			return false;	// This will call into cloud driver
        }
	}
}

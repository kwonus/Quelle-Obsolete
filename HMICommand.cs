﻿using QuelleHMI.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using static QuelleHMI.SearchProviderClient;

namespace QuelleHMI
{
    public class HMICommand
    {
		public HMIStatement statement { get; private set; }
		public HMIClause explicitClause
        {
			get
			{
				return this.statement != null ? statement.explicitClause : null;
			}
        }
		public string command { get; private set; }
		public List<string> errors { get; private set; }
		public List<string> warnings { get; private set; }

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
		public HMICommand(String command)
        {
			this.command = command.Trim();
			this.warnings = new List<string>();
			this.errors = new List<string>();

			if (command != null)
			{
				this.statement = new HMIStatement(this, command.Trim());
			}
        }

		public bool HasMacro()
		{
			if (this.explicitClause != null && this.explicitClause.verb == Verbs.Label.SAVE)
				return true;

			return false;
		}
		public Verbs.Label GetMacroDefinition()
		{
			if (this.explicitClause != null && this.explicitClause.verb == Verbs.Label.SAVE)
				return (Verbs.Label)this.explicitClause;

			return null;
		}
		public Verbs.Display GetPrintClause()
		{
			if (this.explicitClause != null && this.explicitClause.verb == Verbs.Display.PRINT)
				return (Verbs.Display)this.explicitClause;

			return null;
		}
		public bool Search()
        {
			var client = new SearchProviderClient(QuelleControlConfig.search.host);
			var request = new QRequestSearch(this.statement); // (IQuelleSearchRequest)
			IQuelleSearchResult response = client.api.Search(request);

			return (response != null) && response.success;
        }
	}
}

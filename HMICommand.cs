using QuelleHMI.Definitions;
using System;
using System.Collections.Generic;
using System.Linq;
using static QuelleHMI.SearchProviderClient;

namespace QuelleHMI
{
    public class HMICommand
    {
		public static List<HMIStatement> history = new List<HMIStatement>();
		public static HMIStatement current = null;
		public HMIStatement statement { get; private set; }
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
				if (this.errors.Count == 0)
				{
					HMICommand.history.Add(this.statement);
					foreach (var segment in this.statement.segmentation.Values)
					{
						if (segment.verb.Equals(Actions.Search.FIND, StringComparison.InvariantCultureIgnoreCase))
						{
							HMICommand.current = this.statement;
							break;
						}
					}
				}
			}
        }

		public bool Search()
        {
			var client = new SearchProviderClient(QuelleControlConfig.search.host);
			var request = new QRequestSearch(this.statement); // (IQuelleSearchRequest)
			IQuelleSearchResult response = client.api.Search(request);

			return response != null && response.messages.Count == 0;
        }
	}
}

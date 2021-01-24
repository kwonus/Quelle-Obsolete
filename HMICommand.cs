using System;
using System.Collections.Generic;
using System.Linq;

namespace QuelleHMI
{
    public class HMICommand
    {
		public HMIStatement statement { get; private set; }
		public HMISegment dependentClause { get; private set; }
		public string macroName { get; private set; }
		public HMIScope macroScope { get; private set; }
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
		private static string[] reserved = new string[] {	// TODO: Review how these characters are quoted (the list seems too inclusive
			"|", "=", "{", "}", "(", ")", "[", "]", "+", "-", "...", "@", "\\", "/", "#", "?", "*", "%", "&", "|", "\""
		};
		private static string[] reservedQuoted = null;
		private static string[] reservedReplaced = null;

		public HMICommand(String command)
        {
			if (reservedQuoted == null)
            {
				reservedQuoted = new string[reserved.Length];
				for (int i = 0; i < reserved.Length; i++)
                {
					string keyword = reserved[i];
					switch (keyword.Length)
                    {
						case 1: reservedQuoted[i] = "\\" + keyword; break;
						case 2: reservedQuoted[i] = "\\" + keyword[0] + "\\" + keyword[1]; break;
						case 3: reservedQuoted[i] = "\\" + keyword[0] + "\\" + keyword[1] + "\\" + keyword[2]; break;
					}
				}
			}
			if (reservedReplaced == null)
			{
				reservedQuoted = new string[reserved.Length];
				for (int i = 0; i < reserved.Length; i++)
				{
					string encoding = "";
					string keyword = reserved[i];
					foreach (char c in keyword.ToCharArray())
                    {
						UInt32 x = (UInt32) c;
						string e = string.Format("%0x{0:X}%", x);
						encoding += e;
                    }
					reservedQuoted[i] = encoding;
				}
			}

			this.command = command.Trim();
			if (command != null)
			{
				string statement = "";
				int lenStatement = 0;

				int pipe;
				int start;
				//	Process the macro definition first;	 TODO: also look for print dependent clause here, too
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
				int len = 0;
				if (pipe >= 0)
                {
					this.macroName = (pipe > 2) ? command.Substring(pipe).Trim() : "";
					len = macroName.Length;
					if (len > 0)
						switch (macroName[0])
                        {
							case '#':	macroScope = HMIScope.System;
										this.macroName = macroName.Substring(1).Trim();
										len = macroName.Length;
										break;
							case '{':	macroScope = HMIScope.Session;
										break;
						}

					if ((len > 2) && macroName.StartsWith('{') && macroName.EndsWith('}'))
					{
						this.macroName = macroName.Substring(1, len-2).Trim();
						len = this.macroName.Length;

						statement = pipe > 0 ? command.Substring(0, pipe).Trim() : string.Empty;
						lenStatement = statement.Length;
					}
					else
					{
						len = 0;
					}
					if (len < 1)
					{
						this.macroName = command.Substring(0, pipe).Trim();
						this.Notify("error", "Ill-defined label:");
						this.Notify("error", "Command processing has been aborted.");
					}
				}
                else
                {
					this.macroName = null;
					this.macroScope = HMIScope.Undefined;
					statement = command.Trim();
					lenStatement = statement.Length;
				}
				if (lenStatement == 0)
				{
					this.Notify("error", "Ill-defined statement:");
					this.Notify("error", "Command processing has been aborted.");
				}
				if (this.errors == null)
                {
					//	Replace all quoted/reserved characters
					//
					for (int i = 0; i < reservedQuoted.Length; i++)
					{
						if (statement.Contains(reservedQuoted[i]))
							statement = statement.Replace(reservedQuoted[i], reservedReplaced[i]);
					}
					this.statement = new HMIStatement(this, 0, statement);
				}
			}
        }

		public HMIScope HasMacro()
		{
			if (this.statement == null || this.macroName == null)
				return HMIScope.Undefined;

			return this.macroScope;
		}
	}
}

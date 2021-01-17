using System;
using System.Collections.Generic;
using System.Linq;

namespace ClarityHMI
{
    public class HMICommand
    {
		public HMIStatement statement { get; private set; }
		public string macroName { get; private set; }
		public HMIScope macroScope { get; private set; }
		public bool macroSimultaneousExecution { get; private set; }
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
		private static string[] reserved = new string[] {
			":=", "=", "{", "}", "(", ")", "[", "]", "+", "-", "...", "@", "\\", "/", "#", "?", "*", "%", "&", "|", "\""
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

				//	Process the macro definition first
				//
				int equals1 = command.IndexOf(":=");
				int equals2 = command.IndexOf("::");
				// Pick the minimum non-negative value:
				int equals = (equals1 < 0 && equals2 < 0) ? -1 : (equals1 < 0) ? equals2 : (equals2 < 0) ? equals1 : (equals1 < equals2) ? equals1 : equals2;
				int len = 0;
				if (equals >= 0)
                {
					this.macroName = (equals > 2) ? command.Substring(0, equals).Trim() : "";
					this.macroSimultaneousExecution = (equals >= 0) && (equals == equals2);
					len = macroName.Length;
					if (len > 0)
						switch (macroName[0])
                        {
							case '@':	macroScope = HMIScope.Cloud;
										this.macroName = macroName.Substring(1).Trim();
										len = macroName.Length;
										break;
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
						len = macroName.Length;

						if (len > 0)
						{
							statement = command.Substring(equals+2).Trim();
							lenStatement = statement.Length;
						}
					}
					else
					{
						len = 0;
					}
					if (len < 1)
					{
						this.macroName = command.Substring(0, equals).Trim();
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

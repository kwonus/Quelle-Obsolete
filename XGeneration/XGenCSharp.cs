using System;
using System.Collections.Generic;
using System.Text;

namespace QuelleHMI.XGeneration
{
    class XGenCSharp : XGen
	{
		//	C# code-generator:
		public XGenCSharp()
		{
			;
		}
		protected override string additionalImports()
		{
			return "";
		}
		protected override string QImport(String module)
		{
			if (this.Include(module))
			{
				string line;

				line = "using Quelle." + (module.EndsWith("[]") ? module.Substring(0, module.Length - 2) : module) + ";";
				return line + "\n";
			}
			return "";
		}
		protected override string constructor(Dictionary<string, string> accessible)
		{
			return "";
		}
		protected override string getterAndSetter(string name, string type)
		{
			type = type.Replace("String", "string").Replace("Boolean", "bool");

			if (type.StartsWith("HashMap"))
				type = "Dictionary" + type.Substring("HashMap".Length);
			string variable = "\t\tpublic " + type + "\t" + name + ";\n";
			return variable;
		}
		protected override string export(Type type)
		{
			string file = "";
			try
			{
				String parent = null; // QClass(c.BaseType);

				foreach (string k in accessible.Keys)
				{
					string t = accessible[k];
					if (t == null)
						continue;
					file += QImport(t);
				}
				if (parent != null)
					file += QImport(parent);

				string package = "\nnamespace Quelle\n{\n";
				file += package;

				string qname = QClass(type);
				string classname = QClass(type) != null ? qname : "UNKNOWN";
				file += ("\tpublic class " + classname);
				if (parent != null)
				{
					file += ": ";
					file += parent;
				}
				file += "\n\t{\n";
				foreach (string p in accessible.Keys)
				{
					string t = accessible[p];
					file += getterAndSetter(p, t);
				}
				file += "\n\t}\n}\n";
			}
			catch (Exception e)
			{
				file = "ERROR: " + e.Message;
			}
			return file;
		}
	}
}

using System;
using System.Collections.Generic;
using System.Text;

namespace QuelleHMI.XGeneration
{
    class XGenGo : XGen
	{
		//	GoLang code-generator:
		public XGenGo()
		{
			;
		}
		protected override string additionalImports()
		{
			return "";
		}
		protected override string QImport(string module)
		{
			if (this.Include(module))
			{
				string line;

				line = "import \"Quelle." + (module.EndsWith("[]") ? module.Substring(0, module.Length-2) : module) + "\"";
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

			if (type.StartsWith("HashMap<"))
				type = "map[" + type.Substring("HashMap<".Length).Replace(',', ']').Replace(">", "");
			else if (type.EndsWith("[]"))
				type = "[]" + type.Substring(0, type.Length - 2);

			string variable = "\t" + name + " " + type + "\n";
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

				string qname = QClass(type);
				string classname = QClass(type) != null ? qname : "UNKNOWN";
				file += "\ntype " + classname + " struct {\n";

				if (parent != null)
				{
					file += " /* structure inheritance in GoLang ... I don't think so /";
					file += parent;
					file += "/ */";
				}
				foreach (string p in accessible.Keys)
				{
					string t = accessible[p];
					file += getterAndSetter(p, t);
				}
				file += "}";
			}
			catch (Exception e)
			{
				file = "ERROR: " + e.Message;
			}
			return file;
		}
	}
}

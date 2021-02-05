using System;
using System.Collections.Generic;
using System.Text;

namespace QuelleHMI.XGeneration
{
    class XGenJava : XGen
	{
		//	Java code-generator:
		public XGenJava()
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

				line = "import Quelle." + (module.EndsWith("[]") ? module.Substring(0, module.Length - 2) : module) + "\"";
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
			string variable = "\tpublic " + type + "\t" + name + ";\n";
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

				string package = "\npackage Quelle;";
				file += package;

				string qname = QClass(type);
				string classname = QClass(type) != null ? qname : "UNKNOWN";
				file += ("\n\nclass " + classname);
				if (parent != null)
				{
					file += " extends ";
					file += parent;
				}
				file += " {\n";
				foreach (string p in accessible.Keys)
				{
					string t = accessible[p];
					file += getterAndSetter(p, t);
				}
				file += "\n}";
			}
			catch (Exception e)
			{
				file = "ERROR: " + e.Message;
			}
			return file;
		}
	}
}

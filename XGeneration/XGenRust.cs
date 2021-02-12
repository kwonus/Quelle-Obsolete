using System;
using System.Collections.Generic;
using System.Text;

namespace QuelleHMI.XGeneration
{
    class XGenRust : XGen
	{
		//	GoLang code-generator:
		public XGenRust()
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

				line = "use Quelle." + (module.EndsWith("[]") ? module.Substring(0, module.Length - 2) : module) + ";";
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
			var array = false;

			if (type.StartsWith("HashMap<"))
			{
				var t1 = type.Substring("HashMap<".Length);
				var comma = t1.IndexOf(",");
				var t2 = t1.Substring(comma + 1).Replace(">", "");
				t1 = t1.Substring(0, comma);
				type = "HashMap";
			}
			else if (type.EndsWith("[]"))
				array = true;
				type = type.Substring(0, type.Length - 2);

			if (type == "int")
				type = "i32";
			else if (type == "uint")
				type = "u32";
			if (array)
				type = "[" + type + "]";

			string variable = "\t" + name + ": " + type + ",\n";
			return variable;
		}
		protected override string export(Type type)
		{
			string file = "";
			try
			{
				String parent = null; // QClass(c.BaseType);

				if (parent == null)
				{
					foreach (string k in accessible.Keys)
					{
						string t = accessible[k];
						if (t == null)
							continue;
						file += (QImport(t));
					}
				}

				string qname = QClass(type);
				string classname = QClass(type) != null ? qname : "UNKNOWN";
				file += "\nstruct " + classname + " {\n";

				if (parent != null)
				{
					file += " /* structure inheritance in Rust ... not in this library /";
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

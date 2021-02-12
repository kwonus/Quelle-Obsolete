using System;
using System.Collections.Generic;
using System.Text;

namespace QuelleHMI.XGeneration
{
    class XGenProtobuf : XGen
	{
		private uint index;
		//	GoLang code-generator:
		public XGenProtobuf()
		{
			index = 0;
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

				line = "import \"" + (module.EndsWith("[]") ? module.Substring(0, module.Length - 2) : module) + ".proto\";";
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
				var t2 = t1.Substring(comma + 1).Replace(">", "").Trim();
				t1 = t1.Substring(0, comma).Trim();
				if (t1 == "int")
					t1 = "int32";
				else if (t1 == "uint")
					t1 = "uint32";
				if (t2 == "int")
					t2 = "int32";
				else if (t2 == "uint")
					t2 = "uint32";
				type = "map<" + t1 + ", " + t2 + ">";
			}
			else if (type.EndsWith("[]"))
			{
				array = true;
				type = type.Substring(0, type.Length - 2);
			}
			if (type == "int")
				type = "int32";
			else if (type == "uint")
				type = "uint32";
			if (array)
				type = "repeated " + type;

			string variable = "\t" + name + " " + type + " = " + (++index).ToString() + ";\n";
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
						file += QImport(t);
					}
				}

				string qname = QClass(type);
				string classname = QClass(type) != null ? qname : "UNKNOWN";
				file += "\nmessage " + classname + " {\n";

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

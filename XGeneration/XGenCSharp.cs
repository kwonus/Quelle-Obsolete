using System;
using System.Collections.Generic;
using System.Text;

namespace QuelleHMI.XGeneration
{
    class XGenCSharp : XGen
	{
		private bool scoping;
		//	C# code-generator:
		public XGenCSharp()
		{
			scoping = true;
		}
		protected override string additionalImports()
		{
			return "";
		}
		protected override string QImport(Type type)
		{
			var module = type.Name;
			if (this.Include(module))
			{
				module = QClassForImport(type);
				string line;

				line = "using Quelle." + (type.IsArray ? module.Substring(0, module.Length - 2) : module) + ";";
				return line + "\n";
			}
			return "";
		}
		protected override string constructor(Dictionary<string, string> accessible)
		{
			return "";
		}
		protected override string getterAndSetter(string name, Type type)
		{
			string stype = this.QClass(type, "Dictionary<{0}, {1}>");

			string variable = this.scoping
				? "\t\tpublic " + stype + "\t" + name + " { get; }\n"
				: "\t\t" + stype + "\t" + name + " { get; }\n";

			return variable;
		}
		protected override string export(Type type)
		{
			string file = "";
			try
			{
				foreach (string k in accessible.Keys)
				{
					Type t = accessible[k];
					if (t == null)
						continue;
					file += QImport(t);
				}
				string package = "\nnamespace Quelle\n{\n";
				file += package;

				string qname = QClass(type, "Dictionary<{0}, {1}>");
				string classname = qname ?? "UNKNOWN";

				this.scoping = !qname.StartsWith(XGen.InterfacePrefix);

				if (scoping)
					file += ("\tpublic class " + classname);
				else
					file += ("\tpublic interface " + classname);

				file += "\n\t{\n";
				foreach (string p in accessible.Keys)
				{
					Type t = accessible[p];
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

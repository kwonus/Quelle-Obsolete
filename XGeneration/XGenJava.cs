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
			this.types.Add(typeof(string), "String");
			this.types.Add(typeof(bool), "boolean");
			this.types.Add(typeof(Guid), "Guid");
			this.types.Add(typeof(Int16), "int16");
			this.types.Add(typeof(UInt16), "uint16");
			this.types.Add(typeof(Int32), "int32");
			this.types.Add(typeof(UInt32), "uint32");
			this.types.Add(typeof(Int64), "int64");
			this.types.Add(typeof(UInt64), "uint64");
		}
		protected override string additionalImports()
		{
			return "";
		}
		protected override string QImport(Type type)
		{
			string module = type.Name;

			if (this.Include(module))
			{
				string line;

				line = "import Quelle." + module + ";";
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
			string variable = "\tpublic " + type + "\t" + name + ";\n";
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
				string package = "\npackage Quelle;";
				file += package;

				string qname = QClass(type, "HashMap<{0}, {1}>");
				string classname = qname != null ? qname : "UNKNOWN";
				file += ("\n\nclass " + classname);
				file += " {\n";
				foreach (string p in accessible.Keys)
				{
					Type t = accessible[p];
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

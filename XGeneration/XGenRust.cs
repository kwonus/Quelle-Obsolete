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
			this.types.Add(typeof(string), "str");
			this.types.Add(typeof(bool), "bool");
			this.types.Add(typeof(Guid), "str");
			this.types.Add(typeof(Int16), "i16");
			this.types.Add(typeof(UInt16), "u16");
			this.types.Add(typeof(Int32), "i32");
			this.types.Add(typeof(UInt32), "u32");
			this.types.Add(typeof(Int64), "i64");
			this.types.Add(typeof(UInt64), "u64");
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
				module = QClassForImport(type);
				string line;

				line = "use Quelle." + module + ";";
				return line + "\n";
			}
			return "";
		}
		protected override string constructor(Dictionary<string, string> accessible)
		{
			return "";
		}
		private string AdaptType(Type type)
		{
			var stype = QClass(type, "HashMap<{0}, {1}>");

			return type.IsArray ? "Vec<" + stype + ">" : stype;
		}
		protected override string getterAndSetter(string name, Type type)
		{
			var stype = AdaptType(type);

			string variable = "\t" + name + ": " + stype + ",\n";
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
					file += (QImport(t));
				}

				string qname = QClass(type, "map<{0}, {1}>");
				string classname = qname != null ? qname : "UNKNOWN";
				file += "\nstruct " + classname + " {\n";

				foreach (string p in accessible.Keys)
				{
					Type t = accessible[p];
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

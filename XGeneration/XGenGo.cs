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
			this.types.Add(typeof(string), "string");
			this.types.Add(typeof(bool), "bool");
			this.types.Add(typeof(Guid), "string");
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

				line = "\n\t\"Quelle/" + (module.EndsWith("[]") ? module.Substring(0, module.Length-2) : module) + "\"";
				return line;
			}
			return "";
		}
		protected override string constructor(Dictionary<string, string> accessible)
		{
			return "";
		}
		protected override string getterAndSetter(string name, Type type)
		{
			string module = type.Name;

			module = this.QClass(type, "map[{0}] {1}");
			module = module.Replace("String", "string").Replace("Boolean", "bool");

			if (type.IsArray)
				module = "[]" + module;

			string variable = "\t" + name + " " + module + "\n";
			return variable;
		}
		protected override string export(Type type)
		{
			string file = "";

			string package = "package " + QClassForImport(type)	+ "\n\n";
			file += package;

			try
			{
				var test = "";

				foreach (string k in accessible.Keys)
				{
					Type t = accessible[k];
					if (t == null)
						continue;
					test += QImport(t);
				}

				string qname = this.QClass(type, "map[{0}] {1}");
				string classname =qname != null ? qname : "UNKNOWN";
				file += "\ntype " + classname + " struct {\n";

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

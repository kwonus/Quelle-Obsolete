using System;
using System.Collections.Generic;
using System.Text;

namespace QuelleHMI.XGeneration
{
    class XGenC : XGen
	{
		//	C code-generator:
		public XGenC()
		{
			this.types.Add(typeof(string), "char[]");
			this.types.Add(typeof(bool), "bool");
			this.types.Add(typeof(Guid), "char[]");
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
				module = QClassForImport(type);
				string line;

				line = "#include \"" + (module.EndsWith("[]") ? module.Substring(0, module.Length - 2) : module) + ".h\"";
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
			string stype = this.QClass(type, "map<{0}], {1}>", null);

			if (type.IsArray)
				stype = type.Name + "*";


			string variable = "\t" + type + "\t" + name + ";\n";
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
				string qname = this.QClass(type, "map<{0}], {1}>", null);
				string classname = qname != null ? qname : "UNKNOWN";
				file += ("\nstruct " + classname);
				file += "\n{\n";
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

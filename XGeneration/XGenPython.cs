using System;
using System.Collections.Generic;
using System.Text;

namespace QuelleHMI.XGeneration
{
	class XGenPython : XGen
	{
		private bool importJson;

		//	Python code-generator:
		public XGenPython()
        {
			this.importJson = false;
		}
		protected override string additionalImports()
		{
			string imports = importJson ? "import json\n" : "";
			return imports;
		}
		protected override string QImport(String module)
		{
			if (module != null && module.Length > 0 && module.ToLower().StartsWith("hmi") == true)
			{
				string line;

				line = "import " + module;
				return line + "\n";
			}
			return "";
		}
		protected override string constructor(Dictionary<string, string> accessible)
		{
			string def = "\tdef __init__(self, dictionary=None";
			foreach (string p in accessible.Keys)
			{
				def += ", ";
				def += p;
				def += "=None";
			}
			def += "):\n";
			def += "\t\tif dictionary is not None:\n";
			def += "\t\t\tself.dictionary = dictionary\n";
			def += "\t\telse:\n";
			def += "\t\t\tself.dictionary = {}\n\n";
			foreach (string p in accessible.Keys)
			{
				def += "\t\tif " + p + " is not None:\n";
				def += "\t\t\tself." + p + " = " + p + "\n";
			}
			return def;
		}
		protected override string getterAndSetter(string name, string type)
		{
			if (type.StartsWith("HashMap"))
				type = "dictionary";

			string getter = "\t@property\n";
			string setter = "\t@" + name + ".setter\n";

			getter += ("\tdef " + name + "(self):\n");
			getter += ("\t\tif \"" + name + "\" in self.dictionary.keys():\n");
			if (type != null)
				getter += ("\t\t\treturn " + type + "(self.dictionary[\"" + name + "\"])\n");
			else
				getter += ("\t\t\treturn self.dictionary[\"" + name + "\"]\n");
			getter += ("\t\telse:\n");
			getter += ("\t\t\treturn None\n");

			setter += ("\tdef " + name + "(self, value):\n");
			setter += ("\t\tif value is not None:\n");
			if ((type != null) && !this.importJson)
				this.importJson = true;
			if (type != null)
				setter += ("\t\t\tself.dictionary[\"" + name + "\"] = value.dictionary\n");
			else
				setter += ("\t\t\tself.dictionary[\"" + name + "\"] = value\n");

			setter += ("\t\telif self.dictionary[\"" + name + "\"] is not None:\n");
			setter += ("\t\t\tdel self.dictionary[\"" + name + "\"]\n");

			return getter + "\n" + setter + "\n";
		}
		protected override string export(Type c)
		{
			string file = "";
			try
			{
				foreach (string k in accessible.Keys)
				{
					string t = accessible[k];
					if (t == null)
						continue;
					file += QImport(t);
				}
				/*
				String parent = QClass(c.BaseType);
				if (parent != null)
					file += QImport(parent);
				*/
				string qname = QClass(c);
				string classname = QClass(c) != null ? qname : "UNKNOWN";
				file += ("\nclass " + classname);
				/*
				if (parent != null)
				{
					file += "(";
					file += parent;
					file += ")";
				}
				*/
				file += ":\n";
				file += "\tdictionary = None\n\n";

				foreach (string p in accessible.Keys)
				{
					string t = accessible[p];
					file += getterAndSetter(p, t);
				}
				file += constructor(accessible);
			}
			catch (Exception e)
			{
				file = "ERROR: " + e.Message;
			}
			return additionalImports() + file;
		}

	}
}

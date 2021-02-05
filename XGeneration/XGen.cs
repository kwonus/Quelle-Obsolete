using QuelleHMI.Controls;
using System;
using System.Collections.Generic;
using System.Text;

namespace QuelleHMI
{
    public abstract class XGen
    {
		protected abstract string export(Type c);
		protected abstract string additionalImports();
		protected abstract string QImport(String module);
		protected abstract string constructor(Dictionary<string, string> accessible);
		protected abstract string getterAndSetter(string name, string type);

		protected Dictionary<string, string> accessible;

		protected XGen()
        {
			this.accessible = new Dictionary<string, string>();
        }
		public static XGen Factory(string type)
        {
			if (type != null)
			{
				switch (type.ToLower())
				{
					case "golang":
					case "go":		return new XGeneration.XGenGo();

					case "c":
					case "c++":
					case "cpp":		return new XGeneration.XGenC();

					case "c#":
					case "csharp":	return new XGeneration.XGenCSharp();

					case "java":	return new XGeneration.XGenJava();

					case "python":	return new XGeneration.XGenPython();	// Python code-gen is pretty narly looking, so not advertised.  Better to use dictionary types in json and ignore this
				}
			}
			return null;
		}
		protected bool Include(string module)
        {
			if (module == null || module.Length < 1)
				return false;

			return module.StartsWith("HMI", StringComparison.InvariantCultureIgnoreCase)
				|| module.StartsWith("CTL", StringComparison.InvariantCultureIgnoreCase)
				|| module.StartsWith("CloudSearch", StringComparison.InvariantCultureIgnoreCase);
		}
		protected virtual string GetTypeName(System.Reflection.FieldInfo info)
		{
			var name = info.Name.ToLower();
			var type = info.FieldType;
			if (type.IsEnum)
				return "int";
			if (type.Name.StartsWith("dictionary", StringComparison.InvariantCultureIgnoreCase))
			{
				switch (name)
				{
					case "segmentation": return "HashMap<uint, HMIClause>";
					case "fragments": return "HashMap<uint, HMIFragment>";
					default: return "HashMap<String, String>";
				}
			}
			if (type.Name.StartsWith("uint", StringComparison.InvariantCultureIgnoreCase))
				return "uint";
			if (type.Name.StartsWith("int", StringComparison.InvariantCultureIgnoreCase))
				return "int";

			return type.Name;
		}
		protected virtual string GetTypeName(System.Reflection.PropertyInfo info)
        {
			var name = info.Name.ToLower();
			var type = info.PropertyType;
			if (type.IsEnum)
				return "int";
			if (type.Name.StartsWith("dictionary", StringComparison.InvariantCultureIgnoreCase))
			{
				switch (name)
                {
					case "segmentation": return "HashMap<uint, HMIClause>";
					case "fragments":    return "HashMap<uint, HMIFragment>";
					default:			 return "HashMap<String, String>";
				}
			}
			if (type.Name.StartsWith("uint", StringComparison.InvariantCultureIgnoreCase))
				return "uint";
			if (type.Name.StartsWith("int", StringComparison.InvariantCultureIgnoreCase))
				return "int";


			return type.Name;
        }
		public string export(string className, int indents = 0)
		{
			Type item;
			string test = "." + className.ToLower();

			if (typeof(CloudSearch).ToString().ToLower().EndsWith(test))
				item = typeof(CloudSearch);
			else if (typeof(HMIStatement).ToString().ToLower().EndsWith(test))
				item = typeof(HMIStatement);
			else if (typeof(HMIClause).ToString().ToLower().EndsWith(test))
				item = typeof(HMIClause);
			else if (typeof(HMIFragment).ToString().ToLower().EndsWith(test))
				item = typeof(HMIFragment);
			else if (typeof(CTLSearch).ToString().ToLower().EndsWith(test))
				item = typeof(CTLSearch);
			else if (typeof(CTLDisplay).ToString().ToLower().EndsWith(test))
				item = typeof(CTLDisplay);
			else if (typeof(CTLQuelle).ToString().ToLower().EndsWith(test))
				item = typeof(CTLQuelle);
			else
				return "Unknown serialization class";

			var properties = item.GetProperties();
			var fields = item.GetFields(System.Reflection.BindingFlags.Public);
			foreach (var f in fields)
			{
				if (accessible.ContainsKey(f.Name))
					accessible[f.Name] = GetTypeName(f);
			}
			foreach (var p in properties)
			{
				if (p.CanRead && !accessible.ContainsKey(p.Name))
					accessible[p.Name] = GetTypeName(p);
			}

			string text = export(item);

			if (indents >= 1 && indents <= 16)
			{   // really? (more than 16 spaces for indentation ... sorry)
				StringBuilder spaces = new StringBuilder("");
				spaces.Append(' ', indents);
				text = text.Replace("\t", spaces.ToString());
			}
			return text;
		}

		protected string QClass(Type c)
		{
			string ctype = c != null ? c.ToString() : "";
			string[] tparts = ctype.Split(new char[] { '.' });
			ctype = tparts[tparts.Length - 1];
			return ctype.Length > 0 ? ctype : null;
		}
    }
}

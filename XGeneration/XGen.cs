﻿using QuelleHMI.Controls;
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

					case "rust":	return new XGeneration.XGenRust();

					case "c":
					case "c++":
					case "cpp":		return new XGeneration.XGenC();

					case "c#":
					case "csharp":	return new XGeneration.XGenCSharp();

					case "java":	return new XGeneration.XGenJava();

					case "python":	return new XGeneration.XGenPython();    // Python code-gen is pretty narly looking, so not advertised.  Better to use dictionary types in json and ignore this

					case "protobufs":
					case "protobuf":
					case "pb":	    return new XGeneration.XGenProtobuf();
				}
			}
			return null;
		}
		protected bool Include(string module)
        {
			if (module == null || module.Length < 1)
				return false;

			return this.GetType(module) != null;
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
			bool array = type.Name.EndsWith("[]");
			if (type.Name.StartsWith("uint", StringComparison.InvariantCultureIgnoreCase))
				return array ? "uint[]" : "uint";
			if (type.Name.StartsWith("int", StringComparison.InvariantCultureIgnoreCase))
				return array ? "int[]" : "int";


			return type.Name;
        }
		protected Type GetType(string className)
        {
			var test = "." + className.ToLower();
			if (test.EndsWith("[]"))
				test = test.Substring(0, test.Length - 2);

			Type item;

			if (typeof(CloudSearch).ToString().ToLower().EndsWith(test))
				item = typeof(CloudSearch);
			else if (typeof(HMIStatement).ToString().ToLower().EndsWith(test))
				item = typeof(HMIStatement);
			else if (typeof(HMIClause).ToString().ToLower().EndsWith(test))
				item = typeof(HMIClause);
			else if (typeof(Fragment).ToString().ToLower().EndsWith(test))
				item = typeof(Fragment);
			else if (typeof(Fragments.SearchFragment).ToString().ToLower().EndsWith(test))
				item = typeof(Fragments.SearchFragment);
			else if (typeof(Verbs.Search).ToString().ToLower().EndsWith(test))
				item = typeof(Verbs.Search);
			else if (typeof(Tokens.TokenFeature).ToString().ToLower().EndsWith(test))
				item = typeof(Tokens.TokenFeature);
			else if (typeof(Tokens.TokenMatch).ToString().ToLower().EndsWith(test))
				item = typeof(Tokens.TokenMatch);
			else if (typeof(Tokens.TokenVector).ToString().ToLower().EndsWith(test))
				item = typeof(Tokens.TokenVector);
			else if (typeof(CTLSearch).ToString().ToLower().EndsWith(test))
				item = typeof(CTLSearch);
			else if (typeof(CTLDisplay).ToString().ToLower().EndsWith(test))
				item = typeof(CTLDisplay);
			else if (typeof(CTLQuelle).ToString().ToLower().EndsWith(test))
				item = typeof(CTLQuelle);
			else
				item = null;

			return item;
		}
		public string export(string className, int indents = 0)
		{
			Type item = GetType(className);
			if (item == null)
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

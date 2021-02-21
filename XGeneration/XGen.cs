using QuelleHMI.Controls;
using QuelleHMI.Fragments;
using QuelleHMI.Verbs;
using System;
using System.Collections.Generic;
using System.Text;

namespace QuelleHMI
{
	public abstract class XGen
	{
		protected Dictionary<Type, string> types;

		public const string InterfacePrefix = "IQuelle";
		public static readonly string[] InterfaceSuffixes = { "Request", "Result" };

		public static Type[] Interfaces = new Type[] {
			typeof(IQuelleSearchRequest), typeof(IQuelleSearchResult),
			typeof(IQuelleFetchRequest), typeof(IQuelleFetchResult),
			typeof(IQuellePageRequest), typeof(IQuellePageResult),

			typeof(IQuelleSearchControls), typeof(IQuelleSearchClause),
			typeof(IQuelleSearchFragment),
			typeof(Tokens.IQuelleTokenFeature), typeof(Tokens.IQuelleTokenMatch), typeof(Tokens.IQuelleTokenVector)
		};

		public static Type[] Enumerations = new Type[] {
			typeof(HMIClause.HMIPolarity)
		};

		public static Type[] OtherTypes = new Type[] {
			typeof(HMIStatement), typeof(HMIClause)
		};

		protected abstract string export(Type c);
		protected abstract string additionalImports();
		protected abstract string QImport(Type module);
		protected abstract string constructor(Dictionary<string, string> accessible);
		protected abstract string getterAndSetter(string name, Type type);

		protected Dictionary<string, Type> accessible;

		protected XGen()
		{
			this.accessible = new Dictionary<string, Type>();
			this.types = new Dictionary<Type, string>();
		}
		public static XGen Factory(string type)
		{
			if (type != null)
			{
				switch (type.ToLower())
				{
					case "golang":
					case "go": return new XGeneration.XGenGo();

					case "rust": return new XGeneration.XGenRust();

					case "c":
					case "c++":
					case "cpp": return new XGeneration.XGenC();

					case "c#":
					case "csharp": return new XGeneration.XGenCSharp();

					case "java": return new XGeneration.XGenJava();

					case "protobufs":
					case "protobuf":
					case "pb": return new XGeneration.XGenProtobuf(false);

					case "idl":
					case "grpc": return new XGeneration.XGenProtobuf(true);
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
		protected Type TestType(Type type, string test)
		{
			return type.ToString().ToLower().EndsWith(test) ? type : null;
		}
		private Type GetType(string className)
		{
			var test = "." + className.ToLower();
			if (test.EndsWith("[]"))
				test = test.Substring(0, test.Length - 2);

			foreach (var type in XGen.OtherTypes)
			{
				var found = TestType(type, test);
				if (found != null)
					return found;
			}

			foreach (var type in XGen.Interfaces)
			{
				var found = TestType(type, test);
				if (found != null)
					return found;
			}
			return null;
		}
		public string export(string className, int indents = 0)
		{
			Type item = (className != null && className != "*") ? GetType(className) : typeof(IQuelleSearchRequest);
			if (item == null)
				return "Unknown serialization class";

			addAccessibleMembers(item);

			var text = export(item);

			if (indents >= 1 && indents <= 16)
			{   // really? (more than 16 spaces for indentation ... sorry)
				StringBuilder spaces = new StringBuilder("");
				spaces.Append(' ', indents);
				text = text.Replace("\t", spaces.ToString());
			}
			return text;
		}
		protected void addAccessibleMembers(Type type)
		{
			if (type == null)
				return;

			if (type.BaseType != null)
				addAccessibleMembers(type.BaseType);

			foreach (var i in type.GetInterfaces())
				addAccessibleMembers(i);

			if (type.IsInterface)
			{
				var members = type.GetMembers();
				foreach (System.Reflection.MemberInfo m in members)
				{
					var name = m.Name;
					if (name.StartsWith("get_"))
					{
						name = name.Substring(4);
						var info = (System.Reflection.MethodInfo)m;
						if (!accessible.ContainsKey(name))
							accessible[name] = info.ReturnType;
					}
				}
			}
			else
			{
				if (type.BaseType != null)
					addAccessibleMembers(type.BaseType);

				var properties = type.GetProperties();
				var fields = type.GetFields(System.Reflection.BindingFlags.Public);
				foreach (var f in fields)
					if (!accessible.ContainsKey(f.Name))
					{
						accessible[f.Name] = f.FieldType;
					}
				foreach (var p in properties)
					if (p.CanRead && !accessible.ContainsKey(p.Name))
					{
						accessible[p.Name] = p.PropertyType;
					}
			}
		}

		protected string QClass(Type c, string mapFormat /* = "HashMap<{0}, {1}>" */, bool? arrayPrefix = false)
		{
			if (types.ContainsKey(c))
				return types[c];

			string ctype = c != null ? c.ToString() : "";
			if (c.IsArray)
			{
				ctype = ctype.Substring(0, ctype.Length - 2);

				var rawType = Type.GetType(ctype);
				if (types.ContainsKey(rawType))
					return types[rawType];
			}
			string[] tparts = ctype.Split(new char[] { '.', '+' });
			if (tparts.Length >= 4 && tparts[3].StartsWith("Dictionary"))
			{
				tparts = ctype.Split(new char[] { '.', '+' }, 4);

				var mapper = tparts[3].Split(new char[] { '[', ']', ',' }, StringSplitOptions.RemoveEmptyEntries);
				if (mapper.Length == 3)
				{
					var key = QClass(Type.GetType(mapper[1]), mapFormat);
					var val = QClass(Type.GetType(mapper[2]), mapFormat);
					ctype = string.Format(mapFormat, key, val);
				}
			}
			else
			{
				ctype = tparts[tparts.Length - 1];
			}
			if (ctype.Length < 1)
				return null;

			if (ctype.StartsWith("HMI"))
				ctype = ctype.Substring(3);

			else if (ctype.StartsWith(InterfacePrefix))
				ctype = ctype.Substring(InterfacePrefix.Length);

			if ((!arrayPrefix.HasValue) || (!c.IsArray))    // C does not decorate types with [], those gop on the variable names.
				return ctype;

			return arrayPrefix.Value ? "[]" + ctype : ctype + "[]";
		}
	
		protected string QClassForImport(Type c)
		{
			return QClass(c, "", null);
		}
	}
}

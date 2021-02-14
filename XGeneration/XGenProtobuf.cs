using System;
using System.Collections.Generic;
using System.Text;

namespace QuelleHMI.XGeneration
{
    class XGenProtobuf : XGen
	{
		private bool grpc;
		private uint index;
		//	GoLang code-generator:
		public XGenProtobuf(bool grpc)
		{
			this.index = 0;
			this.grpc = grpc;
			this.messages = null;
			this.actions = new List<string>();
			this.enumerations = new Dictionary<Type, string>();
		}
		protected override string additionalImports()
		{
			return "";
		}
		protected Dictionary<Type, string> enumerations;
		protected List<string> actions;
		protected string messages;

		private string CreateMessages()
		{
			if (this.grpc)
			{
				this.messages = "";

				foreach (var type in XGen.Enumerations)
				{
					string expansion = "\tenum " + this.AdaptType(type.Name) + " {\n";
					Array values = type.GetEnumValues();
					Type arrayType = type.GetEnumUnderlyingType();
					foreach (var val in values)
					{
						var key = type.GetEnumName(val);
						if (arrayType == typeof(string))
							expansion += ("\t\t" + key + " = \"" + (string) val + "\";\n");
						else if (arrayType == typeof(char))
							expansion += ("\t\t" + key + " = '" + (char) val + "';\n");
						else
							expansion += ("\t\t" + key + " = " + (int) val + ";\n");
					}
					expansion += "\t}\n";
					this.enumerations.Add(type, expansion);
				}

				foreach (var type in XGen.Interfaces)
				{
					var name = type.Name;
					if (name.StartsWith(XGen.InterfacePrefix) && name.EndsWith(XGen.InterfaceSuffixes[0]))
					{
						var action = name.Substring(XGen.InterfacePrefix.Length);
						var len = action.Length - XGen.InterfaceSuffixes[0].Length;
						action = action.Substring(0, len);
						this.actions.Add(action);
					}
					name = this.AdaptType(name);
					if (name.StartsWith(XGen.InterfacePrefix))
						name = name.Substring(1);
					else if (name.StartsWith("HMI"))
						name = name.Substring(3);

					this.messages += ("message " + name + " {\n");
					uint idx = 0;

					var properties = type.GetProperties();
					foreach (var p in properties)
					{
						var member = p.Name;
						var memberType = p.PropertyType;
						var memeberTypeName = this.AdaptType(memberType.Name);

						if (this.enumerations.ContainsKey(memberType))
							this.messages += this.enumerations[memberType];
						this.messages += "\t";
						this.messages += memeberTypeName;
						this.messages += " ";
						this.messages += member;
						this.messages += " = ";
						this.messages += (++idx).ToString();
						this.messages += ";\n";
					}
					this.messages += "}\n";
				}
			}
			return this.messages;
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

		private string AdaptType(string type)
		{
			var array = type.EndsWith("[]");
			if (array)
				type = AdaptType(type.Substring(0, type.Length - 2));

			var guid = type.Equals("guid", StringComparison.InvariantCultureIgnoreCase) || type.Equals("uuid", StringComparison.InvariantCultureIgnoreCase);

			if (type.Equals("string", StringComparison.InvariantCultureIgnoreCase))
				type = "string";
			else if (type.Equals("bool", StringComparison.InvariantCultureIgnoreCase) || type.Equals("boolean", StringComparison.InvariantCultureIgnoreCase))
				type = "bool";
			else if (guid)
				type = "string";
			else if (type.Equals("int", StringComparison.InvariantCultureIgnoreCase))
				type = "int64";
			else if (type.Equals("uint", StringComparison.InvariantCultureIgnoreCase))
				type = "uint64";
			else if (type.StartsWith("uint", StringComparison.InvariantCultureIgnoreCase))
				type = "ui" + type.Substring(2);
			else if (type.StartsWith("int", StringComparison.InvariantCultureIgnoreCase))
				type = "i" + type.Substring(1);
			else if (type.StartsWith("HMI"))
				type = type.Substring("HMI".Length);
			else if (type.StartsWith("Dictionary", StringComparison.InvariantCultureIgnoreCase))
				type = "map<uint64, string>";
			
			else if (type.StartsWith("HashMap<"))
			{
				var t1 = type.Substring("HashMap<".Length);
				var comma = t1.IndexOf(",");
				var t2 = this.AdaptType(t1.Substring(comma + 1).Replace(">", "").Trim());
				t1 = this.AdaptType(t1.Substring(0, comma).Trim());
				type = "map<" + t1 + ", " + t2 + ">";
			}
			else foreach (var i in XGen.Interfaces)
            {
					if (i.Name == type)
					{
						type = type.Substring(1);   // remove I prefix
					}
            }

			return (array ? "repeated " : "")  + type;
		}
		protected override string getterAndSetter(string name, string type)
		{
			string variable = "\t" + this.AdaptType(type) + " = " + (++index).ToString() + ";\n";
			return variable;
		}
		protected override string export(Type type)
		{
			string file;

			if (grpc)
            {
				this.CreateMessages();

				file  = "syntax = \"proto3\";\npackage Quelle;\n\n";
				file += "service SearchProvider {\n";

				foreach (var action in this.actions)
                {
					file += ("\trpc " + action + "(" + XGen.InterfacePrefix.Substring(1) + action + XGen.InterfaceSuffixes[0] + ") ");
					file += ("returns (" + XGen.InterfacePrefix.Substring(1) + action + XGen.InterfaceSuffixes[1] + ") {};\n");
				}
				file += "}\n";
				file += this.messages;
			}
			else
			{
				file = "";
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
			}
			return file;
		}
	}
}

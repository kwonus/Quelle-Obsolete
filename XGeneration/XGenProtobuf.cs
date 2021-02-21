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
					string expansion = "\tenum " + this.AdaptType(type) + " {\n";
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
					name = this.AdaptType(type);
					if (name.StartsWith(XGen.InterfacePrefix))
						name = name.Substring(XGen.InterfacePrefix.Length);
					else if (name.StartsWith("HMI"))
						name = name.Substring(3);

					this.messages += ("message " + name + " {\n");
					uint idx = 0;

					this.accessible.Clear();
					this.addAccessibleMembers(type);

					foreach (var member in this.accessible.Keys)
                    {
						var memberType = this.accessible[member];
						var memeberTypeName = this.AdaptType(memberType);

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

		protected override string QImport(Type type)
		{
			string module = type.Name;

			if (this.Include(module))
			{
				string line;

				line = "import \"" + module + ".proto\";";
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
			var stype = this.QClass(type, "map<{0}, {1}>", null);

			return (type.IsArray ? "repeated " : "")  + stype;
		}
		protected override string getterAndSetter(string name, Type type)
		{
			string variable = "\t" + this.AdaptType(type) + " " + name + " = " + (++index).ToString() + ";\n";
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
					file += ("\trpc " + action + "(" + action + XGen.InterfaceSuffixes[0] + ") ");
					file += ("returns (" + action + XGen.InterfaceSuffixes[1] + ") {};\n");
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
							Type t = accessible[k];
							if (t == null)
								continue;
							file += QImport(t);
						}
					}
					string qname = this.QClass(type, "map<{0}, {1}>", null);
					string classname = qname != null ? qname : "UNKNOWN";
					file += "\nmessage " + classname + " {\n";

					if (parent != null)
					{
						file += " /* structure inheritance in GoLang ... I don't think so /";
						file += parent;
						file += "/ */";
					}
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
			}
			return file;
		}
	}
}

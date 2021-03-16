using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace QuelleHMI.Verbs
{
    public class System : HMIClause
    {
        public const string SYNTAX = "SYSTEM";
        public override string syntax { get => SYNTAX; }
        public const string EXIT = "@exit";
        public const string HELP = "@help";
        public const string GENERATE = "@generate";
        public const string REGENERATE = "@generate!";

        public static readonly List<string> EXPLICIT = new List<string>() { EXIT, HELP, GENERATE, REGENERATE };

        public string[] parameters;

        private string className;
        private XGen language;

        private string output;
        private bool overwrite;
        private int arguments;

        public System(HMIStatement statement,string segment, string verb)
        : base(statement, 1, HMIPolarity.UNDEFINED, segment, HMIClauseType.EXPLICIT_INDEPENDENT)
        {
            this.verb = verb;
        }
        protected override bool Parse()
        {
            if (this.segment == null)
                return false;

            var expanded = this.statement.statement.ToLower().Replace(">", " > ");
            var max = expanded.Contains('!') ? 6 : 5;
            var tokens = expanded.Split(HMIClause.Whitespace, max, StringSplitOptions.RemoveEmptyEntries);

            if (tokens[0] == System.GENERATE || tokens[0] == System.REGENERATE)
            {
                this.verb = tokens[0];
                this.arguments = (tokens.Length == 3)
                               ?  3
                               : (tokens.Length == 5) && (tokens[3] == ">")
                               ?  5
                               :  0;

                if (this.arguments >= 3)
                {
                    this.language = XGen.Factory(tokens[1]);
                    this.className = tokens[2];
                }
                if (this.language != null)
                {
                    if (this.arguments == 5)
                    {
                        this.output = tokens[4];
                        this.overwrite = this.verb.Equals(System.REGENERATE, StringComparison.InvariantCultureIgnoreCase);
                     }
                    return true;
                }
                this.Notify("error", "The language provided (" + tokens[1] + ") was not recognized.");
            }
            return false;
        }
        public override bool Execute()
        {
            if (this.arguments != 0)
            {
                var code = language.export(this.className);

                if (this.output == null)
                    Console.WriteLine(code);
                else
                {
                    var exists = File.Exists(this.output);
                    if (exists && !this.overwrite)
                    {
                        this.Notify("error", "The file already exists. Use ! to override.");
                        return false;
                    }
                    else if (exists)
                    {
                        try
                        {
                            File.Delete(this.output);
                        }
                        catch
                        {
                            this.Notify("error", "Could not overwrite file.");
                            return false;
                        }
                    }
                    using (StreamWriter writer = new StreamWriter(this.output))
                    {
                        writer.Write(code);
                    }
                    return true;
                }
            }
            return false;
        }
        public static string Help()
        {
            return "";
        }
    }
}
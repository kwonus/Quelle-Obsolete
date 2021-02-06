using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace QuelleHMI.Verbs
{
    public class Generate : HMIClause
    {
        public const string SYNTAX = "SYSTEM";
        public override string syntax { get => SYNTAX; }
        public const string VERB = "@generate";

        private string className;
        private XGen language;

        private string output;
        private bool overwrite;
        private int arguments;

        public Generate(HMIStatement statement,string segment)
        : base(statement, 1, HMIPolarity.UNDEFINED, segment, HMIClauseType.SIMPLE)
        {
            this.maximumScope = HMIScope.System;
            this.verb = VERB;
        }
        protected override bool Parse()
        {
            if (this.segment == null)
                return false;

            var expanded = this.statement.statement.ToLower().Replace("!", " ! ").Replace(">", " > ");
            var max = expanded.Contains('!') ? 6 : 5;
            var tokens = expanded.Split(HMIClause.Whitespace, max, StringSplitOptions.RemoveEmptyEntries);

            if (tokens[0] == "@generate")
            {
                this.arguments = (tokens.Length == 3)
                               ?  3
                               : (tokens.Length == 5) && (tokens[3] == ">")
                               ?  5
                               : (tokens.Length == 6) && ( (tokens[3] == "!" && tokens[4] == ">") || (tokens[3] == ">" && tokens[4] == "!") )
                               ?  6
                               :  0;

                if (this.arguments > 0)
                {
                    this.language = XGen.Factory(tokens[1]);
                    this.className = tokens[2];
                }
                if (this.language != null)
                {
                    switch (this.arguments)
                    {
                        case 5: this.output = tokens[4];
                                this.overwrite = false;
                                break;
                        case 6: this.output = tokens[5];
                                this.overwrite = true;
                                break;
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
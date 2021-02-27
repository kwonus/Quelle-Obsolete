using System;
using System.Net.Http;
using QuelleHMI;
using QuelleHMI.Controls;
using QuelleHMI.Verbs;
using static QuelleHMI.HMISession;

//  This driver implementation depends upon QuelleHMI library
//
//  The source code here, illustrates the simplicity of creating a Quelle driver using the Quelle-HMI library.
//  This code can be used as a template to create your own driver, or it can be subclassed to extend behavior:
//  Specifically, the Cloud*() methods need implementations in the subclass to provide the actual search functionality.
//
//  A tandem project in github provides an interpreter.  Between these two projects, there are less than 500 
//  lines of code to extend and/or modify.  The value proposition here is that parsing is tedious. And starting
//  your search CLI with a concise syntax with an easy to digest parsing library could easily save your team
//  a person-year in design-time and coding.  Quelle* source code is licensed with an MIT/BSD-style license.
//  The specific licence file will be added to the github projects shortly (perhaps it is already here).
//
//  The design incentive for QuelleHMI and the standard driver is to support the broader Digital-AV effort:
//  That project [Digital-AV] provides a command-line interface for searching and publishing the KJV bible.
//  Every attempt has been made to keep the Quelle syntax agnostic about the search domain, yet the Quelle
//  user documentation itself [coming soon to github] is heavily biased in its syntax examples. Still, the search
//  domain of the Standard Quelle Driver remains unbiased.

namespace Quelle.DriverDefault
{
    public class QuelleDriver : IQuelleDriver
    {
        protected HMIConfigurationDefault configuration;
        public string CloudHostName
        {
            get;
            protected set;
        }
        public HMICloud CloudHost
        {
            get;
            protected set;
        }
        public QuelleDriver(string host)
        {
            this.CloudHostName = host;
            this.configuration = new HMIConfigurationDefault();
        }

        public QuelleDriver()
        {
            this.configuration = new HMIConfigurationDefault();
        }
        public IQuelleResultString Read(string setting)
        {
            //  Add specialized behavior before following through to HMISession static implementation
            //
            var result = this.configuration.Read(setting);
            return result;
        }
        public IQuelleResultInt ReadInt(string setting)
        {
            //  Add specialized behavior before following through to HMISession static implementation
            //
            var result = this.configuration.ReadInt(setting);
            return result;
        }

        public IQuelleResult Remove(string setting)
        {
            //  Add specialized behavior before following through to HMISession static implementation
            //
            return this.configuration.Remove(setting);
        }

        public IQuelleResult Write(string setting, string value)
        {
            //  Add specialized behavior before following through to HMISession static implementation
            //
            var result = this.configuration.Write(setting, value);
            return result;
        }
        public IQuelleResult Write(string setting, Int64 value)
        {
            //  Add specialized behavior before following through to HMISession static implementation
            //
            var result = this.configuration.Write(setting, value);
            return result;
        }

        public IQuelleSearchResult Search(HMIStatement statement)
        {
            if (statement == null)
                return null;

            if (this.CloudHost == null && this.CloudHostName != null)
                this.CloudHost = new HMICloud(this.CloudHostName);

            if (this.CloudHost != null)
            {
                var search = new CloudSearchRequest(statement, (CTLSearch)this.configuration.seachConf);
                return this.CloudHost.Post(search);
            }
            return null;
        }
        public IQuelleFetchResult Fetch(Guid session, uint cursor, uint count)
        {
            return null;
        }

        public IQuelleResultString Get(Guid session, UInt16 key)
        {
            return null;
        }
        public IQuelleResultObject Display(HMIStatement statement, string specification)
        {
            return new HMIResultObject("Developer has not implemeneted the Display method of the driver");
        }
        public string Help()
        {
            string text = "Help is available on each of these topics:\n";

            foreach (var verb in new string[] { QuelleHMI.Verbs.Search.VERB, Control.SET, Control.CLEAR, Define.SAVE, Define.DELETE, Define.REVIEW, Print.VERB, Show.VERB, Generate.GENERATE, Generate.REGENERATE, "@exit" })
                text += ("\n\t" + (verb.StartsWith('@') ? verb.Substring(1) : verb));

            text += "\n\n";
            text += "For extensive extensive help, consult the project README:\n";
            text += "https://github.com/kwonus/Quelle/blob/master/Quelle.md";

            return text;
        }
        public string Help(string topic)
        {
            if (string.IsNullOrWhiteSpace(topic))
                return this.Help();

            //  This block makes the @ optional on all verbs
            var help = topic.Trim().ToLower();
            if (help.StartsWith('@'))
                help = help.Substring(1).Trim();
            foreach (var verb in new string[] { QuelleHMI.Verbs.Search.VERB, Control.SET, Control.CLEAR, Print.VERB, Define.SAVE, Define.DELETE, Define.REVIEW, Show.VERB, Generate.GENERATE, Generate.REGENERATE, "@exit" })
                if (verb.EndsWith(help) && verb.StartsWith('@'))
                {
                    help = '@' + help;
                    break;
                }

            switch (help)
            {
                case QuelleHMI.Verbs.Search.VERB:   return QuelleHMI.Verbs.Search.Help();
                case Control.SET:                   return Control.Help(Control.SET);
                case Control.CLEAR:                 return Control.Help(Control.CLEAR);
                case Print.VERB:                    return Print.Help();
                case Define.SAVE:
                case Define.DELETE:
                case Define.REVIEW:                 return Define.Help(help);
                case Show.VERB:                     return Show.Help();
                case Generate.GENERATE:
                case Generate.REGENERATE:           return Generate.Help();
                case "@exit":                       return "@exit\n... on a line by itself followed by the <Enter> key will cause the program to exit.\n";
            }
            return this.Help();
        }
    }
}

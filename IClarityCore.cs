using System;
using System.Collections.Generic;
using System.Text;

namespace ClarityHMI
{
    interface IClarityResult
    {
        bool success { get; }
        string[] errors { get; }
        string[] warnings { get;  }
    }
    interface IClarityResultObject : IClarityResult
    {
        object result { get;  }
    }

    interface IClarityResultString : IClarityResult
    {
        string result { get; }
    }
    interface IClarityResultInt : IClarityResult
    {
        Int64 result { get; }
    }
    interface IClaritResultStringyArray : IClarityResult
    {
        string[] results { get; }
    }

    interface IClarityConfig
    {
        IClarityResultString        Read(string setting, HMIScope scope);                     // Show *
        IClaritResultStringyArray   ReadArray(string setting, HMIScope scope);                // SHow
        IClarityResultInt           ShowInt(string setting, HMIScope scope);                  // Show

        IClarityResultString        Reomove(string setting, HMIScope scope);                  // Remove *

        IClarityResultString        Write(string setting, HMIScope scope, string value);     // Config *
        IClarityResultString        Write(string setting, HMIScope scope, string[] value);   // Config
        IClarityResultString        Write(string setting, HMIScope scope, Int64 value);      // Config

        protected static readonly char[] dot = new char[] { '.' };
        protected static readonly string[] delimiter = new string[] { "<|||||||>" };
    }
    interface IClarityDriver : IClarityConfig
    {
        IClarityResultObject    Search(HMIStatement statement);
        IClarityResultString    Summarize(HMIStatement statement);
        IClarityResultString    Summarize(object searchResult);                             
        IClarityResultString    Import(string setting, HMIScope scope, string value, bool silent = false);
        IClarityResultString    Export(string setting, HMIScope scope, string value, bool silent = false);

    }
}

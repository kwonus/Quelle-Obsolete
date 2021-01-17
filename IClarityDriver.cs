using System;
using System.Collections.Generic;
using System.Text;

namespace ClarityHMI
{
    public interface IClarityResult
    {
        bool success { get; }
        string[] errors { get; }
        string[] warnings { get;  }
    }
    public interface IClarityResultObject : IClarityResult
    {
        object result { get;  }
    }

    public interface IClarityResultString : IClarityResult
    {
        string result { get; }
    }
    public interface IClarityResultInt : IClarityResult
    {
        Int64 result { get; }
    }
    public interface IClaritResultStringyArray : IClarityResult
    {
        string[] results { get; }
    }

    public interface IClarityConfig
    {
        IClarityResultString        Read(string setting, HMIScope scope);                     // Show *
        IClaritResultStringyArray   ReadArray(string setting, HMIScope scope);                // SHow
        IClarityResultInt           ReadInt(string setting, HMIScope scope);                  // Show

        IClarityResult              Remove(string setting, HMIScope scope);                  // Remove *

        IClarityResult              Write(string setting, HMIScope scope, string value);     // Config *
        IClarityResult              Write(string setting, HMIScope scope, string[] value);   // Config
        IClarityResult              Write(string setting, HMIScope scope, Int64 value);      // Config
    }
    public interface IClarityDriver : IClarityConfig
    {
        IClarityResultObject    Search(HMIStatement statement);
        IClarityResultString    Summarize(HMIStatement statement);
        IClarityResultString    Summarize(object searchResult);                             
        IClarityResultObject    Import(HMIStatement statement);
        IClarityResultObject    Export(HMIStatement statement);

    }
}

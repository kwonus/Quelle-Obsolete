using System;
using System.Collections.Generic;
using System.Text;

namespace QuelleHMI
{
    public interface IQuelleResult
    {
        bool success { get; }
        string[] errors { get; }
        string[] warnings { get;  }
    }
    public interface IQuelleResultObject : IQuelleResult
    {
        object result { get;  }
    }

    public interface IQuelleResultString : IQuelleResult
    {
        string result { get; }
    }
    public interface IQuelleResultInt : IQuelleResult
    {
        Int64 result { get; }
    }
    public interface IClaritResultStringyArray : IQuelleResult
    {
        string[] results { get; }
    }

    public interface IQuelleConfig
    {
        IQuelleResultString        Read(string setting, HMIScope scope);                     // Show *
        IQuelleResultInt           ReadInt(string setting, HMIScope scope);                  // Show

        IQuelleResult              Remove(string setting, HMIScope scope);                  // Remove *

        IQuelleResult              Write(string setting, HMIScope scope, string value);     // Config *
        IQuelleResult              Write(string setting, HMIScope scope, Int64 value);      // Config
    }
    public interface IQuelleHelp
    {
        string Help();
        string Help(string topic);

    }
    public interface IQuelleDriver : IQuelleConfig, IQuelleHelp
    {
        IQuelleResultObject    Search(HMIStatement statement);
        IQuelleResultObject    Display(HMIStatement statement, string specification);                        
    }

}

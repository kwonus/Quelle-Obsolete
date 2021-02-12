using QuelleHMI.Controls;
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
    public interface IQuelleCloudSearchRequest
    {
        Verbs.Search[] clauses { get;  }
        CTLSearch controls { get; }
        uint count { get; }

    }
    public interface IQuelleCloudFetchRequest
    {
        Guid session { get; }
        uint cursor { get; }
        uint count { get; }
    }
    public interface IQuelleCloudFetchhResult : IQuelleResult
    {
        uint cursor { get; }
        uint remainder { get; }
    }
    public interface IQuelleCloudSearchResult : IQuelleCloudFetchhResult
    {
        string summary { get; }
        Guid session { get; }
        Dictionary<UInt16, string> records { get; }     // The UInt16 is a key to be used with the session to retrieve a specific result
                                                        // the string is th abstract for the record    }
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
        IQuelleResultString        Read(string setting);                     // Show *
        IQuelleResultInt           ReadInt(string setting);                  // Show

        IQuelleResult              Remove(string setting);                  // Remove *

        IQuelleResult              Write(string setting, string value);     // Config *
        IQuelleResult              Write(string setting, Int64 value);      // Config
    }
    public interface IQuelleHelp
    {
        string Help();
        string Help(string topic);
    }
    public interface IQuelleDriver : IQuelleConfig, IQuelleHelp
    {
        IQuelleCloudSearchResult Search(HMIStatement statement);

        IQuelleCloudFetchhResult Fetch(Guid session, uint cursor, uint count);

        IQuelleResultString Get(Guid session, UInt16 key);
    }
}

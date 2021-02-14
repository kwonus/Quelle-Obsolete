using QuelleHMI.Controls;
using QuelleHMI.Verbs;
using System;
using System.Collections.Generic;
using System.Text;

namespace QuelleHMI
{
    public interface IQuelleResult
    {
        bool success { get; }
        string[] errors { get; }
        string[] warnings { get; }
    }
    public interface IQuelleSearchRequest
    {
        IQuelleSearchClause[] clauses { get;  }
        IQuelleSearchControls controls { get; }
        UInt64 count { get; }

    }
    public interface IQuelleFetchRequest
    {
        Guid session { get; }
        UInt64 cursor { get; }
        UInt64 count { get; }
    }
    public interface IQuelleFetchResult : IQuelleResult
    {
        UInt64 cursor { get; }
        UInt64 remainder { get; }
        Guid session { get; }
        Dictionary<UInt64, string> records { get; }     // The UInt16 is a key to be used with the session to retrieve a specific result
                                                        // the string is th abstract for the record
    }
    public interface IQuelleSearchResult : IQuelleFetchResult
    {
        string summary { get; }
        IQuelleSearchRequest enrichedRequest { get; }
    }
    public interface IQuelleStatusRequest
    {
        
    }
    public interface IQuelleStatusResult : IQuelleResult
    {
    }
    public interface IQuellePageRequest
    {
        Guid session { get; }
        string format { get; }
        UInt64 page { get; }
    }
    public interface IQuellePageResult : IQuelleResult
    {
        string result { get; }
        IQuellePageRequest request { get; }
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
        IQuelleSearchResult Search(HMIStatement statement);

        IQuelleFetchResult Fetch(Guid session, uint cursor, uint count);

        IQuelleResultString Get(Guid session, UInt16 key);
    }
}

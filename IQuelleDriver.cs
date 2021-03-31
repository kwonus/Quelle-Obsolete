using QuelleHMI.Definitions;
using QuelleHMI.Actions;
using System;
using System.Collections.Generic;
using System.Text;

namespace QuelleHMI
{
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
    public interface IQuelleFetchResult
    {
        Guid session { get; } // MD5/GUID
        Dictionary<UInt32, String> abstracts { get; }
        UInt64 cursor { get; }
        UInt64 count { get; }
        UInt64 remainder { get; }
        Dictionary<string, string> messages { get; }
    }
    public interface IQuelleSearchResult : IQuelleFetchResult
    {
        Dictionary<byte, Dictionary<byte, Dictionary<byte, Dictionary<byte, UInt64>>>> records { get; }
        string summary { get; }
    }
    public interface IQuellePageRequest
    {
        Guid session { get; }
        string format { get; }
        UInt64 page { get; }
    }
    public interface IQuellePageResult  // GET the HTML, TEXT, or MD representation of page
    {
        string result { get; }
        Dictionary<string, string> messages { get; }
    }
    public interface IQuelleHelp
    {
        string Help();
        string Help(string topic);
    }
}

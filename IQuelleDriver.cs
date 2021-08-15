using QuelleHMI.Definitions;
using QuelleHMI.Actions;
using System;
using System.Collections.Generic;

namespace QuelleHMI
{
    public static class SegmentElement  // slight compacting to store/encode 98-bits in a 64-bit ulong [this avoids overhead of classes and tuples]
    {
        // SSSS CCCC IIIIIIII
        // IIIIIIII      [UInt32] is the widx of the starting writ of the matched segment
        // IIIIIIII+CCCC [UInt32] is the widx of the ending writ of the matched segment
        // SSSS          [UInt16] is the segment index (cooresponding to the request) of the matched segment [this is NOT bit-wise, which allows UInt16.Max segments per query]
        // 
        public static UInt32 GetStart(UInt64 encoded)
        {
            var widx = (UInt32)(encoded & 0xFFFFFFFF);
            return widx;
        }
        public static UInt32 GetEnd(UInt64 encoded)
        {
            var last = (UInt32) (encoded & 0xFFFFFFFF);
            var cnt = (UInt32) ((encoded & 0xFFFF00000000) >> 32);  // cnt really represents cnt-1
            last += cnt;
            return last;
        }
        public static UInt16 GetSegmentIndex(UInt64 encoded)
        {
            var segmentIdx = (UInt16)((encoded & 0xFFFF000000000000) >> 48);
            return segmentIdx;
        }
        public static UInt64 Create(UInt32 start, UInt32 end, UInt16 segmentIndex)
        {
            UInt64 result = (UInt64)segmentIndex;
            result <<= 48;
            result |= start;

            if (end > start)
            {
                UInt64 cnt = end - start;   // cnt really represents cnt-1
                cnt <<= 32;
                result |= cnt;
            }
            return result;
        }
        public static UInt64 Create(UInt32 widx, UInt16 wcnt, UInt16 segmentIndex)
        {
            UInt64 result = (UInt64)segmentIndex;
            result <<= 48;
            result |= widx;

            if (wcnt > 1)
            {
                UInt64 cnt = (UInt64) wcnt-1;   // cnt really represents wcnt-1
                cnt <<= 32;
                result |= cnt;
            }
            return result;
        }
    }
    public interface IQuelleSearchRequest
    {
        IQuelleSearchClause[] clauses { get;  }
        IQuelleSearchControls controls { get; }
        Guid session { get; }
    }
    public interface IQuelleSearchResult
    {
//      HashSet<UInt64> segments { get; set; }  // formerly "matches": each segment match is a UInt64 encoded via SegmentElement
        HashSet<UInt16> verses { get; set; }  
        HashSet<UInt32> tokens { get; set; } 
        string summary { get; }
        Guid session { get; } // MD5/GUID
        Dictionary<UInt32, String> abstracts { get; }
        Dictionary<string, List<string>> messages { get; }

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
        Dictionary<string, List<string>> messages { get; }
    }
    public interface IQuelleHelp
    {
        string Help();
        string Help(string topic);
    }
}

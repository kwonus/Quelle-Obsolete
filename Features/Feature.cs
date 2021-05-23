using System;
using System.Collections.Generic;
using System.Text;

namespace QuelleHMI.Tokens
{
/*
    // These are redundantly defined in managed space (also in C++ domain):
    public abstract class Type
    {
        public const UInt16 typeNOT        = 0x8000;
        public const UInt16 typeWord       = 0x0800;
        public const UInt16 typeWordAV     = 0x1800;
        public const UInt16 typeWordAVX    = 0x2800;
        public const UInt16 typeWordAny    = 0x3800;
        public const UInt16 typeWordSame   = 0x4800;
        public const UInt16 typeWordDiff   = 0xC800; // not same

        public const UInt16 typeEnglish    = 0x0400;
        public const UInt16 typeHebrew     = 0x0200;
        public const UInt16 typeGreek      = 0x0100;
        public const UInt16 typeLemma      = 0x0080;
        public const UInt16 typePOSBits    = 0x0040;
        public const UInt16 typePOS        = 0x0020;
        public const UInt16 typePunct      = 0x0010;
        public const UInt16 typeBounds     = 0x0008;
        public const UInt16 typeReserved4  = 0x0004;
        public const UInt16 typeReserved2  = 0x0002;
        public const UInt16 typeReserved1  = 0x0001;

    }
*/

    public interface IQuelleFeature
    {
        bool searchable { get; }
        bool not { get; }
        string feature { get; }
        bool isMatch(UInt32 widx);
        byte type { get; }
        byte subtype { get; }
        List<UInt16> featureMatchVector { get; }
        UInt32 discretePOS { get; } // Discrete POS is always a singleton and requires 32 bits
    }
    public class Feature : IQuelleFeature
    {
        public bool searchable { get; set; }
        public bool not { get; set; }

        public string feature { get; set; }
        public List<UInt16> featureMatchVector { get; set; }
        public byte type { get; set; }
        public byte subtype { get; set;  }
        public UInt32 discretePOS { get; set; } // Discrete POS is always a singleton and requires 32 bits

        public Feature(string f)
        {
            this.featureMatchVector = null;

            if (f != null)
            {
                this.feature = f.Trim();
                this.not = this.feature.StartsWith("!");
                if (this.not)
                    this.feature = this.feature.Length > 1 ? this.feature.Substring(1) : "";
                    
                this.searchable = (this.feature.Length >= 1);
            }
            else
            {
                this.not = false;
                this.feature = "";
                this.searchable = false;
            }
        }
        public bool isMatch(UInt32 widx)
        {
            return false;
        }
    }
}

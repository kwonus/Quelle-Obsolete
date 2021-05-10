using System;
using System.Collections.Generic;
using System.Text;

namespace QuelleHMI.Tokens
{
    public interface IQuelleFeature
    {
        string feature { get; }
        bool isMatch(UInt32 widx);
        UInt16[] featureMatchVector { get; }
    }
    public class Feature : IQuelleFeature
    {
        public string feature { get; set; }
        public UInt16[] featureMatchVector { get; set; }

        public Feature(string f)
        {
            this.feature = f;
            this.featureMatchVector = null;
        }
        public bool isMatch(UInt32 widx)
        {
            return false;
        }
    }
}

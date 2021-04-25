using System;
using System.Collections.Generic;
using System.Text;

namespace QuelleHMI.Tokens
{
    public interface IQuelleFeature
    {
        string feature { get; }
        char featureType { get; }
        UInt16[] featureMatchVector { get; }
    }
}

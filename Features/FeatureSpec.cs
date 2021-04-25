using System;
using System.Collections.Generic;
using System.Text;

namespace QuelleHMI.Tokens
{
    public interface IQuelleFeatureSpec
    {
        string matchSpec { get; }
        IQuelleFeatureMatch[] matchAny { get; }
    }
}

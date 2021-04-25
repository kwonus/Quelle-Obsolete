using System;
using System.Collections.Generic;
using System.Text;

namespace QuelleHMI.Tokens
{public interface IQuelleFeatureMatch
    {
        string condition { get; }
        IQuelleFeature[] features { get; }
    }
}

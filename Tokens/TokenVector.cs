using System;
using System.Collections.Generic;
using System.Text;

namespace QuelleHMI.Tokens
{
    public interface IQuelleTokenVector
    {
        string specification { get; }
        IQuelleTokenMatch[] matchAll { get; }
    }
}

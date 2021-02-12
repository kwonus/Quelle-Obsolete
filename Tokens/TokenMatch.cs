using System;
using System.Collections.Generic;
using System.Text;

namespace QuelleHMI.Tokens
{
    public class TokenMatch
    {
        public string condition { get; }
        public TokenFeature[] anyFeature { get; }
    }
}

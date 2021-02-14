using System;
using System.Collections.Generic;
using System.Text;

namespace QuelleHMI.Tokens
{
    public interface IQuelleTokenMatch
    {
        string condition { get; }
        IQuelleTokenFeature[] anyFeature { get; }
    }
    public class TokenMatch: IQuelleTokenMatch
    {
        public string condition { get; }
        public IQuelleTokenFeature[] anyFeature { get; }
    }
}

using System;
using System.Collections.Generic;
using System.Text;

namespace QuelleHMI.Tokens
{
    public interface IQuelleTokenFeature
    {
        string feature { get; }
    }
    public class TokenFeature: IQuelleTokenFeature
    {
        public string feature { get; }
    }
}

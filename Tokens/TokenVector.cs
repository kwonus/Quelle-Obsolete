using System;
using System.Collections.Generic;
using System.Text;

namespace QuelleHMI.Tokens
{
    public interface IQuelleTokenVector
    {
        public string specification { get; }
        IQuelleTokenMatch[] matchAll { get; }
    }
    public class TokenVector: IQuelleTokenVector
    {
        public string specification { get; }
        public IQuelleTokenMatch[] matchAll { get; }
    }
}

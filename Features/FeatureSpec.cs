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
    public class FeatureSpec: IQuelleFeatureSpec
    {
        public string matchSpec { get; set; }
        public IQuelleFeatureMatch[] matchAny
        {
            get
            {
                var any = this.matchSpec.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
                var array = new IQuelleFeatureMatch[any.Length];
                int i = 0;
                foreach (var text in any)
                    array[i++] = new FeatureMatch(text);

                return null;
            }
        }

        public FeatureSpec(string spec)
        {
            this.matchSpec = spec;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Text;

namespace QuelleHMI.Tokens
{
    public interface IQuelleFeatureMatch
    {
        string condition { get; }
        IQuelleFeature[] features { get; }
    }
    public class FeatureMatch : IQuelleFeatureMatch
    {
        public string condition { get; set; }
        public IQuelleFeature[] features
        {
            get
            {
                var any = this.condition.Split(new char[] { '&' }, StringSplitOptions.RemoveEmptyEntries);
                var array = new IQuelleFeature[any.Length];
                int i = 0;
                foreach (var text in any)
                    array[i++] = new Feature(text);

                return array;
            }
        }

        public FeatureMatch(string cond)
        {
            this.condition = cond;
        }
    }
}

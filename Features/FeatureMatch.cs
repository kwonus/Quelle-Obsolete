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
        private IQuelleFeature[] _features;
        public IQuelleFeature[] features
        {
            get
            {
                if (_features == null)
                {
                    var any = this.condition.Split(new char[] { '&' }, StringSplitOptions.RemoveEmptyEntries);
                    this._features = new IQuelleFeature[any.Length];
                    int i = 0;
                    foreach (var text in any)
                        this._features[i++] = new Feature(text);
                }
                return this._features;
            }
            set
            {
                this._features = value;
            }
        }

        public FeatureMatch(string cond)
        {
            this.condition = cond;
            this._features = null;
        }
    }
}

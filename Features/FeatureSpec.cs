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
        private IQuelleFeatureMatch[] _matchAny;
        public IQuelleFeatureMatch[] matchAny
        {
            get
            {
                if (this._matchAny == null)
                {
                    var any = this.matchSpec.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
                    this._matchAny = new IQuelleFeatureMatch[any.Length];
                    int i = 0;
                    foreach (var text in any)
                        this._matchAny[i++] = new FeatureMatch(text);
                }
                return this._matchAny;
            }
            set
            {
                this._matchAny = value;
            }
        }

        public FeatureSpec(string spec)
        {
            this.matchSpec = spec;
            this._matchAny = null;
        }
    }
}

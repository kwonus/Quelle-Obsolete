using QuelleHMI.Tokens;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace QuelleHMI.Fragments
{
	public interface IQuelleSearchFragment
    {
		byte adjacency { get; }
		byte bracketed { get; }
		IQuelleFeatureSpec[] specifications { get; }  // spec is "All Of" features in the specification
		string text { get; }
		UInt64 bit { get; }
	}
    public class SearchFragment: Fragment, IQuelleSearchFragment
	{
		public UInt16 span { get; protected set; }  // applies primarilly to the first mentioned fragment in a bracketed expression

		protected bool quoted;

		public SearchFragment(Actions.Action segment, string fragment, bool quoted, byte adjacency, byte group, List<SearchFragment> bracketed)
		: base(segment, fragment, adjacency, group)
		{
			this.quoted = quoted;
			this.span = 0;
			this.text = fragment.Trim();

			if (bracketed != null)
			{
				bracketed.Add(this);
				bracketed[0].span = (UInt16)bracketed.Count;
			}
		}
		private IQuelleFeatureSpec[] _specs;
		public IQuelleFeatureSpec[] specifications
		{
            get
            {
				if (this._specs == null)
				{
					var specs = this.text.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
					this._specs = new IQuelleFeatureSpec[specs.Length];
					int i = 0;
					foreach (var text in specs)
					{
						this._specs[i++] = new FeatureSpec(text);
					}
				}
				return this._specs;
            }
			set
            {
				this._specs = value;
            }
        }
	}
}

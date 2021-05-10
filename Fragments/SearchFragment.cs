using QuelleHMI.Tokens;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace QuelleHMI.Fragments
{
	public interface IQuelleSearchFragment
    {
		byte AdjacencyOrAnchorage { get; }
		UInt16 UnorderedSubgroupIndex { get; }
		IQuelleFeatureSpec[] spec { get; }  // spec is "All Of" features in the specification
		string text { get; }
	}
    public class SearchFragment: Fragment, IQuelleSearchFragment
	{
		protected bool quoted;
		protected UInt32 absolute;
		protected bool elipses;
		protected bool bracketStart;
		protected bool bracketEnd;
		protected SearchFragment previous;

		protected UInt32 getStartBracket()   // 0 if false, 1-based index of start-bracket otherwise
		{
			return this.bracketStart ? this.sequence : this.previous != null ? this.previous.getStartBracket() : 0;
		}
		protected UInt32 getStartBracket(out SearchFragment start)   // 0 if false, 1-based index of start-bracket otherwise
		{
			if (this.bracketStart)
			{
				start = this;
				return this.sequence;
			}
			if (this.previous != null)
			{
				return this.previous.getStartBracket(out start);
			}
			start = null;
			return 0;
		}
		protected bool isBracketed
		{
			get
			{
				return bracketStart || (this.previous != null && this.previous.isBracketed && !this.bracketEnd);
			}
		}

		public byte AdjacencyOrAnchorage
		{
			get
			{
				if (!this.quoted)
                {
					return 0;
                }
				switch (this.absolute)
				{
					case 0: return 0;
					case 1: return 0xFF;
					default: return 1;
				}
			}
		}
		public UInt16 UnorderedSubgroupIndex
		{
			get
			{
				if (!this.quoted)
					return 1;

				if (this.absolute > 0)
					return 0;

				else if (this.previous == null) // unquoted fragment or pole position (but pole position would have absolute == 1)
					return 0;

				UInt32 zero = !this.elipses ? 0u : 1u;

				if (this.bracketStart)
                {
					for (var prev = this.previous; prev != null; prev = prev.previous)
                    {
						var prevIdx = prev.UnorderedSubgroupIndex;
						if (prevIdx > 0)
							return ++prevIdx;
					}
					return 1;
                }
				else if (!this.previous.bracketEnd)
                {
					return this.previous.UnorderedSubgroupIndex;
				}
				return 0;
			}
		}

		public SearchFragment(Actions.Action segment, string fragment, bool quoted, uint sequence, SearchFragment prev = null)
		: base(segment, fragment, sequence)
		{
			this.previous = prev;
			this.quoted = quoted;
			this.bracketStart = false;
			this.bracketEnd = false;

			if (this.segment == null)
			{
				segment.Notify("error", "Major design/implementation error: aborting!");
				return;
			}
			string token = fragment != null ? fragment.Trim() : "";
			if (token.StartsWith("..."))
			{
				this.elipses = true;
				token = token.Substring(3);
			}
			if (token.StartsWith("[") && token.EndsWith("]"))
			{
				if (!this.isBracketed)
				{
					segment.Notify("error", "Nested open square braces encounted:");
					segment.Notify("error", "Fragment processing is aborted.");
					return;
				}
				this.bracketStart = this.bracketEnd = true;
				segment.Notify("warning", "A single token with square brackets has no effect on ordering.");
				token = token.Substring(1, token.Length - 2).Trim();
			}
			else if (token.StartsWith("["))
			{
				if (!this.isBracketed)
				{
					segment.Notify("error", "Nested open square braces encounted:");
					segment.Notify("error", "Fragment processing is aborted.");
					return;
				}
				this.bracketStart = true;
				token = token.Substring(1).Trim();
			}
			else if (token.EndsWith("]"))
			{
				if (!this.isBracketed)
				{
					segment.Notify("error", "Cannot close a square brace before supplying an open square brace:");
					segment.Notify("error", "Fragment processing is aborted.");
					return;
				}
				this.bracketEnd = true;
				token = token.Substring(0, token.Length - 1).Trim();
			}
			this.text = token.Trim();
		}
		public IQuelleFeatureSpec[] spec
        {
            get
            {
				return null;
            }
        }
	}
}

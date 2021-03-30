using QuelleHMI.Tokens;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace QuelleHMI.Fragments
{
	public interface IQuelleSearchFragment
    {
		UInt32[] positionAspects { get; }
		IQuelleTokenVector[] anyOf { get; }
		string text { get; }
	}
    public class SearchFragment: Fragment, IQuelleSearchFragment
	{
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

		public UInt32[] positionAspects
		{
			get
			{
				UInt32[] aspects = null;

				if (this.absolute > 0)
					return new UInt32[] { this.absolute };

				else if (this.previous == null) // unquoted fragment or pole position (but pole position would have absolute == 1)
					return null;

				UInt32 zero = !this.elipses ? 0u : 1u;

				if (this.previous.bracketEnd)
				{
					UInt32 bracketed = this.previous.getStartBracket();
					UInt32 len = this.sequence - bracketed;
					aspects = new uint[len + 1];
					aspects[0] = zero;
					for (var i = 1; i <= len; i++)
						aspects[i] = bracketed++;
				}
				else if (this.isBracketed)
				{
					SearchFragment start;
					UInt32 bracketed = this.previous.getStartBracket(out start);

					if (start != null && start.previous != null)
					{
						if (start.previous.bracketEnd)
						{
							UInt32 previuosBracketed = this.previous.getStartBracket();
							UInt32 len = bracketed - previuosBracketed;
							aspects = new uint[len + 1];
							aspects[0] = zero;
							for (var i = 1; i <= len; i++)
								aspects[i] = previuosBracketed++;
						}
					}
					else
					{
						aspects = new uint[] { zero, bracketed - 1 };
					}
				}
				else
				{
					aspects = new uint[] { zero, this.sequence - 1 };
				}
				return aspects;
			}
		}

		public SearchFragment(Actions.Action segment, string fragment, uint sequence, SearchFragment prev = null)
		: base(segment, fragment, sequence)
		{
			this.previous = prev;
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
			if (token.StartsWith('[') && token.EndsWith(']'))
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
			else if (token.StartsWith('['))
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
			else if (token.EndsWith(']'))
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
		/*
		public string identity
		{
			get
			{
				BitArray s;
				if (this.segment != null)
				{
					s = new BitArray((int)this.segment.sequence);
					s.Set(0, true);
				}
				else s = new BitArray(0, false);

				var f = new BitArray((int)this.sequence);
				f.Set(0, true);

				var id = new AVXSearchBitsSerializable(s, f);
				return id.segments + ":" + id.fragments;
			}
		}
		*/
		public IQuelleTokenVector[] anyOf
        {
            get
            {
				return null;
            }
        }
	}
}

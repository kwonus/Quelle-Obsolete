using QuelleHMI.Tokens;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace QuelleHMI.Fragments
{
    public class HMISearchFragment: HMIFragment
    {
		public HMISearchFragment(HMIClause segment, string fragment, uint sequence, HMIFragment prev = null)
		: base(segment, fragment, sequence, prev)
		{
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
		public TokenVector[] anyOf
        {
            get
            {
				return null;
            }
        }
	}
}

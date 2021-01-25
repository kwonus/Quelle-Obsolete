using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace QuelleHMI
{
	public class HMIFragment
	{
		private HMIPhrase segment;
		private HMIFragment previous
		{ // used to determine position of previous token
			get => (this.sequence > 1) && this.segment.fragments.ContainsKey(this.sequence-1) ? this.segment.fragments[this.sequence-1] : null;
		}
		private HMIFragment next
		{ // used to determine position of previous token
			get => (this.sequence >= 1) && this.segment.fragments.ContainsKey(this.sequence+1) ? this.segment.fragments[this.sequence+1] : null;
		}
		public UInt32 order { get; private set; }
		public Boolean? ordered;
		public Boolean unorderCancelled = false;

		public Boolean? elipses { get; private set; }
		public UInt32 sequence { get; private set; }	// Sequence number of fragment
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
		public string fragment { get; private set; }
		public HMIToken token { get; private set; }

		private Boolean isOrdereNext()
        {
			if (this.ordered == null)
				return (this.previous != null) ? this.previous.isOrdereNext() : true;
			return this.ordered.Value || this.unorderCancelled;
        }
		public Boolean isOrderedThis()
		{
			if (this.ordered == null)
				return (this.previous != null) ? this.previous.isOrdereNext() : true;
			return this.ordered.Value;
		}
		private Boolean hasElipses()
		{
			if (this.elipses == null)
				return (this.previous != null) ? this.previous.hasElipses() : false;
			return !this.elipses.Value;
		}
		public HMIFragment(HMIPhrase segment, UInt32 order, UInt32 fragmentSeq, string fragment)
		{
			this.fragment = fragment;
			this.segment = segment;
			this.order = order;
			this.sequence = fragmentSeq;

			if (this.segment == null)
            {
				segment.Notify("error", "Major design/implementation error: aborting!");
				return;
            }
			this.ordered = order > 0 ? (Boolean?) null : false;

			string token = fragment != null ? fragment.Trim() : "";
			if (token.StartsWith("..."))
			{
				this.elipses = true;
				token = token.Substring(3);
			}
			if (token.StartsWith('[') && token.EndsWith(']'))
			{
				if (!this.isOrderedThis())
				{
					segment.Notify("error", "Nested open square braces encounted:");
					segment.Notify("error", "Fragment processing is aborted.");
					return;
				}
				segment.Notify("warning", "A single token with square brackets has no effect on ordering.");
				this.ordered = true;
				token = token.Substring(1, token.Length-2).Trim();
			}
			else if (token.StartsWith('['))
			{
				if (!this.isOrderedThis())
				{
					segment.Notify("error", "Nested open square braces encounted:");
					segment.Notify("error", "Fragment processing is aborted.");
					return;
				}
				this.ordered = false;
				token = token.Substring(1).Trim();
			}
			else if (token.EndsWith(']'))
			{
				if (!this.isOrderedThis())
				{
					segment.Notify("error", "Cannot close a square brace before supplying an open square brace:");
					segment.Notify("error", "Fragment processing is aborted.");
					return;
				}
				if (this.ordered != null && this.ordered.HasValue == false)
				{
					this.ordered = true;
					segment.Notify("warning", "There is no meaning to enclosing a single token in square braces (square braces are being ignored).");
				}
				this.unorderCancelled = true;
				token = token.Substring(0, token.Length - 1).Trim();
			}
			this.token = new HMIToken(token);

		}
	}
}
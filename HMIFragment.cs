using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace QuelleHMI
{
	public abstract class HMIFragment
	{
		protected HMIClause segment;

		protected UInt32 absolute;
		protected bool elipses;
		protected bool bracketStart;
		protected bool bracketEnd;
		protected HMIFragment previous;


		public string text { get; protected set; }
		protected UInt32 sequence;  // Sequence number of fragment

		protected UInt32 getStartBracket()   // 0 if false, 1-based index of start-bracket otherwise
        {
			return this.bracketStart ? this.sequence : this.previous != null ? this.previous.getStartBracket() : 0;
		}
		protected UInt32 getStartBracket(out HMIFragment start)   // 0 if false, 1-based index of start-bracket otherwise
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

				else if (this.previous == null)	// unquoted fragment or pole position (but pole position would have absolute == 1)
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
					HMIFragment start;
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

		protected HMIFragment(HMIClause segment, string fragment, uint sequence, HMIFragment prev = null)
		{
			this.previous = prev;
			this.text = fragment != null ? fragment.Trim() : "";
			this.segment = segment;
			this.bracketStart = false;
			this.bracketEnd = false;
			this.sequence = sequence;
		}
	}
}
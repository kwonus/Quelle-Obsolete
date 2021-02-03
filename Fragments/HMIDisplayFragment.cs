using System;
using System.Collections.Generic;
using System.Text;

namespace QuelleHMI.Fragments
{
    public class HMIDisplayFragment: HMIFragment
	{
		public HMIDisplayFragment(HMIClause segment, UInt32 order, UInt32 fragmentSeq, string fragment)
						: base(segment, fragment, fragmentSeq)
		{
			if (this.segment == null)
			{
				segment.Notify("error", "Major design/implementation error: aborting!");
				return;
			}
			this.ordered = true;
		}
	}
}

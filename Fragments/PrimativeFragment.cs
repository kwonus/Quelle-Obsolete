using System;
using System.Collections.Generic;
using System.Text;

namespace QuelleHMI.Fragments
{
    public class PrimativeFragment: Fragment
    {
		public PrimativeFragment(HMIClause segment, string fragment, uint sequence)
			: base(segment, fragment, sequence)
		{
			if (this.segment == null)
			{
				segment.Notify("error", "Major design/implementation error: aborting!");
				return;
			}
		}
	}
}

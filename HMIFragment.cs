using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace QuelleHMI
{
	public abstract class HMIFragment
	{
		protected HMIClause segment;

		public UInt32 order { get; protected set; }
		public Boolean ordered { get; protected set; }
		public Boolean unorderCancelled { get; protected set; } = false;

		public string token { get; protected set; }
		public UInt32 sequence { get; protected set; }  // Sequence number of fragment

		protected HMIFragment(HMIClause segment, string fragment, uint sequence)
		{
			this.token = fragment != null ? fragment.Trim() : "";
			this.segment = segment;
			this.order = sequence;
			this.sequence = sequence;
		}
	}
}
﻿using QuelleHMI.Tokens;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace QuelleHMI
{
	public abstract class Fragment
	{
		public readonly static char[] whitespace = new char[] { ' ', '\t', '\n' };
		protected Actions.Action segment;

		public string text { get; protected set; }
		public FeatureSpec[] specifications { get; protected set; }
		public byte adjacency { get; protected set; }
		public byte bracketed { get; protected set; }
		public UInt64 bit { get; protected set; }
		protected Fragment(Actions.Action segment, string fragment, byte adjacency, byte group)
		{
			this.text = fragment != null ? fragment.Trim() : "";
			this.segment = segment;
			this.adjacency = adjacency;
			this.bracketed = group;
			this.bit = segment.GetNextBit();

			var specs = this.text.Split(Fragment.whitespace, StringSplitOptions.RemoveEmptyEntries);
			this.specifications = new FeatureSpec[specs.Length];
			int i = 0;
			foreach (var spec in specs)
            {
				this.specifications[i++] = new FeatureSpec(spec.Trim());
			}

		}
	}
}
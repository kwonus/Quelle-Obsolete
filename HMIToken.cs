using System;
using System.Collections.Generic;
using System.Text;

namespace ClarityHMI
{
	public class HMIToken
	{
		public string token { get; private set; }
		public Boolean singleton { get; private set;  }

		public HMIToken(string token)
		{
			this.token = token != null ? token.Trim() : "";
			this.singleton = !(token.StartsWith("(") && token.EndsWith(")") || token.Contains("*") || token.Contains("?"));
		}
	}
}

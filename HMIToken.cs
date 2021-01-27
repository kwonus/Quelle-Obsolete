using System;
using System.Collections.Generic;
using System.Text;

namespace QuelleHMI
{
	public class HMIToken
	{
		public string token { get; private set; }
		public Boolean singleton { get; private set;  }

		//	For SEARCH FRAGMENTS
		public HMIToken(string token)
		{
			this.token = token != null ? token.Trim() : "";
			this.singleton = !(token.StartsWith("(") && token.EndsWith(")") || token.Contains("*") || token.Contains("?"));
		}
		//	ALL OTHER FRAGMENTS
		public HMIToken(string token, bool singleton)
		{
			this.token = token != null ? token.Trim() : "";
			this.singleton =singleton;
		}
	}
}

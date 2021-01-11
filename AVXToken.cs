using System;
using System.Collections.Generic;
using System.Text;

namespace ClarityAVX
{
	public class AVXToken
	{
		public string token { get; private set; }
		public Boolean singleton { get; private set;  }

		public AVXToken(string token)
		{
			this.token = token != null ? token.Trim() : "";
			this.singleton = !(token.StartsWith("(") && token.EndsWith(")") || token.Contains("*") || token.Contains("?"));
		}
	}
}

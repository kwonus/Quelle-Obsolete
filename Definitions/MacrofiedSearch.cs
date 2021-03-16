using QuelleHMI.Actions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuelleHMI.Definitions
{
	public struct MacrofiedSearch
	{
		public string text;
		public char polarity;
		public bool quoted;

		public MacrofiedSearch Create(IQuelleSearchClause segment)
		{
			var factory = new MacrofiedSearch();
			factory.text = HMIStatement.SquenchText(segment.segment);
			factory.quoted = segment.segment.StartsWith('"') && segment.segment.EndsWith('"');
			factory.polarity = segment.polarity;
			return factory;
		}
	}
}

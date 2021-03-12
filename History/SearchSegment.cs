using QuelleHMI.Verbs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuelleHMI.Catalog
{
	public struct SearchSegment
	{
		public string text;
		public char polarity;
		public bool quoted;

		public SearchSegment Create(IQuelleSearchClause segment)
		{
			var factory = new SearchSegment();
			factory.text = HMIStatement.SquenchText(segment.segment);
			factory.quoted = segment.segment.StartsWith('"') && segment.segment.EndsWith('"');
			factory.polarity = segment.polarity;
			return factory;
		}
	}
}

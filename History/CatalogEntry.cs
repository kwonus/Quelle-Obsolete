using QuelleHMI.Catalog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuelleHMI.History
{
    public struct CatalogEntry
    {
		public Guid md5;
		public Dictionary<string, string> control;
		public List<SearchSegment> search;
		public HashSet<UInt64> usages;

		public static CatalogEntry Create(HMIStatement statement)
		{
			var factory = new CatalogEntry();
			factory.md5 = Guid.Empty;
			factory.search = new List<SearchSegment>();
			factory.control = new Dictionary<string, string>();
			factory.usages = new HashSet<UInt64>();

			return factory;
		}
	}
}

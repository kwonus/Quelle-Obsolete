using QuelleHMI.Definitions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuelleHMI.Definitions
{
    public struct CatalogEntry
    {
		public Guid md5;
		public Dictionary<string, string> control;
		public List<MacrofiedSearch> search;
		public HashSet<UInt64> usages;

		public static CatalogEntry Create(HMIStatement statement)
		{
			var factory = new CatalogEntry();
			factory.md5 = Guid.Empty;
			factory.search = new List<MacrofiedSearch>();
			factory.control = new Dictionary<string, string>();
			factory.usages = new HashSet<UInt64>();

			return factory;
		}
	}
}

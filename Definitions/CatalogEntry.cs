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
		public HashSet<UInt64> usages;
	}
}

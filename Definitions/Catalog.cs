using QuelleHMI;
using QuelleHMI.Definitions;
using System;
using System.Collections.Generic;

namespace Quelle.Definitions
{
	public struct Catalog
	{
		public Dictionary<string, Guid> labels;
		public Dictionary<Guid, CatalogEntry> entries;

		public Catalog(Dictionary<string, Guid> labels, Dictionary<Guid, CatalogEntry> entries)
		{
			this.labels = labels;
			this.entries = entries;
		}

		public Guid Add(HMIStatement statement)
		{
			return Guid.Empty;
		}
		public void Save(String md5, Guid entry)
		{

			this.labels.Add(md5, entry);
		}
		public static Catalog Create()
		{
			return new Catalog(new Dictionary<string, Guid>(), new Dictionary<Guid, CatalogEntry>());
		}	
	}

}

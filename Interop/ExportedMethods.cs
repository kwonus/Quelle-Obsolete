using QuelleHMI.History;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace QuelleHMI.Serializable
{
    public abstract class ExportedMethods
    {
		/*
		 * 		public Guid md5;
		 *		public Dictionary<string, string> control;
		 *		public List<SearchSegment> search;
		 *		public HashSet<UInt64> usages;
		 */ 
		public static CatalogEntry Parse(string text)
		{
			return new CatalogEntry();
		}
		public static IQuelleSearchRequest PrepareSearch(Guid command, bool firstApplyControlActions)
		{
			return null;
		}
//		public void ApplyControlActions(Guid md5)
		[UnmanagedCallersOnly]
		public static void ApplyControlActions(UInt64 hash_1, UInt64 hash_2)
		{

		}
		public static string ExecuteExplicitAction(Guid command)
		{

			return "";
		}
		public static CatalogEntry Review(UInt64 entry, bool macrosOnly)
		{

			return new CatalogEntry();
		}
		public static Dictionary<UInt64, CatalogEntry> ReviewAll(UInt64 maxEntries, bool macrosOnly)
		{

			return null;
		}
		public static string Help(string topic)
		{

			return "";
		}

	}
}

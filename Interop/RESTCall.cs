using QuelleHMI.Controls;
using QuelleHMI.Verbs;
using System;
using System.Collections.Generic;
using System.Linq;

namespace QuelleHMI
{
    public class CloudSearchRequest : IQuelleSearchRequest
    {
        public IQuelleSearchClause[] clauses { get; private set; }
        public IQuelleSearchControls controls { get; private set; }
        public UInt64 count { get; private set; }

        public CloudSearchRequest(HMIStatement statement, CTLSearch searchControls)
        {
            this.controls = searchControls;

            if (statement == null)
            {
                return;
            }
            var clauses = new List<Verbs.Search>();
            var searches = (from key in statement.segmentation.Keys orderby key select statement.segmentation[key]);
            foreach (var clause in searches)
            {
                clauses.Add((Verbs.Search)clause);
            }
            this.clauses = clauses.ToArray();
        }
    }
}
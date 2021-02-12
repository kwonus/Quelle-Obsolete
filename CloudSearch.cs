using QuelleHMI.Controls;
using System;
using System.Collections.Generic;
using System.Linq;

namespace QuelleHMI
{
    public class CloudSearch: IQuelleCloudSearchRequest
    {
        public Verbs.Search[] clauses { get; private set; }
        public CTLSearch controls { get; private set; }
        public uint count { get; private set; }

        public CloudSearch(HMIStatement statement, CTLSearch searchControls)
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

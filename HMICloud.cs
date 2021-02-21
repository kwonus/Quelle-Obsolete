using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;

namespace QuelleHMI
{
    public class HMICloud
    {
        private Uri host;    // includes :port if non-standadrd
        
        public HMICloud(string host)
        {
            this.host = new Uri(host);
        }
        public IQuelleSearchResult Post(CloudSearchRequest payload)
        {
            var rest = new HttpClient();
            return null;
        }
    }
}

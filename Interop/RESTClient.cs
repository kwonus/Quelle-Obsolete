using System;
using System.IO;
using System.Threading.Tasks;

namespace QuelleHMI
{
    // REQUESTS
    //
    public interface ISearchProviderAsync
    {
        ValueTask<IQuelleStatusResult> StatusAsync(QStatusRequest request);
        ValueTask<IQuelleSearchResult> SearchAsync(QSearchRequest request);
        ValueTask<IQuelleFetchResult> FetchAsync(QFetchRequest request);
        ValueTask<IQuellePageResult> PageAsync(QPageRequest request);
        ValueTask<string> TestAsync(string request);
    }
    public interface ISearchProvider
    {
        IQuelleStatusResult Status(QStatusRequest request);
        IQuelleSearchResult Search(QSearchRequest request);
        IQuelleFetchResult Fetch(QFetchRequest request);
        IQuellePageResult Page(QPageRequest request);
        string Test(string request);
    }

    public class SearchProviderClient
    {
        private String baseUrl;
        public QuelleSearchProvider api;
        public class QuelleSearchProvider: ISearchProvider
        {
            private SearchProviderClient outer;
            public QuelleSearchProvider(SearchProviderClient outer)  {
                this.outer = outer;
            }
            public IQuelleStatusResult Status(QStatusRequest request) {
                return outer.Status(request);
            }
            public IQuelleSearchResult Search(QSearchRequest request)  {
                return outer.Search(request);
            }
            public IQuelleFetchResult Fetch(QFetchRequest request) {
                return outer.Fetch(request);
            }
            public IQuellePageResult Page(QPageRequest request) {
                return outer.Page(request);
            }
            public string Test(string request) {
                return outer.Test(request);
            }
        }
        public SearchProviderClient()
        {
            this.api = new QuelleSearchProvider(this);
        }
        internal IQuelleSearchResult Search(QSearchRequest req)
        {
            return null;
        }
        public IQuelleFetchResult Fetch(QFetchRequest req)
        {
            return null;
        }
        public IQuellePageResult Page(QPageRequest req)
        {
            return null;
        }
        public IQuelleStatusResult Status(QStatusRequest req)
        {
            return null;
        }
        public string Test(string req)
        {
            return null;
        }
    }
}
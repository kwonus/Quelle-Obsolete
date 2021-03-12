using MessagePack;
using System;
using System.IO;
using System.Threading.Tasks;

namespace QuelleHMI
{
    // REQUESTS
    //
    public interface ISearchProviderAsync
    {
        ValueTask<IQuelleStatusResult> StatusAsync();
        ValueTask<IQuelleSearchResult> SearchAsync(QSearchRequest request);
        ValueTask<IQuelleFetchResult> FetchAsync(QFetchRequest request);
        ValueTask<IQuellePageResult> PageAsync(QPageRequest request);
        ValueTask<string> TestAsync(string request);
    }
    public interface ISearchProvider
    {
        IQuelleStatusResult Status();
        IQuelleSearchResult Search(QSearchRequest request);
        IQuelleFetchResult Fetch(QFetchRequest request);
        IQuellePageResult Page(QPageRequest request);
        string Test(string request);
    }

    public class SearchProviderClient
    {
        private String baseUrl;
        public QuelleSearchProvider api;
        public ISearchProvider API { get => api; }

        public class QuelleSearchProvider: ISearchProvider
        {
            private SearchProviderClient outer;
            public QuelleSearchProvider(SearchProviderClient outer)  {
                this.outer = outer;
            }
            public IQuelleStatusResult Status() {
                return outer.Status();
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
        public SearchProviderClient(string host)
        {
            this.baseUrl = host != null ? host : "http://127.0.0.1:7878/";
            if (!this.baseUrl.EndsWith('/'))
                this.baseUrl += '/';
            this.api = new QuelleSearchProvider(this);
        }
        internal const string mimetype = "application/msgpack";
        internal IQuelleSearchResult Search(QSearchRequest req)
        {
            var cloud = new QWebClient(this.baseUrl);
            var payload = MessagePackSerializer.Serialize(req);

            var packedRespospone = cloud.Post("/search", payload, mimetype);
            //var response = MessagePackSerializer.Deserialize<IQuelleSearchResult>(packedRespospone.data);
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
        public IQuelleStatusResult Status()
        {
            var cloud = new QWebClient(this.baseUrl);

            var result = cloud.Get("/");
            Console.WriteLine("Result from get:");
            Console.WriteLine(result);

            return null;
        }
        public string Test(string req)
        {
            return null;
        }
    }
}
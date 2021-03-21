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
        ValueTask<IQuelleSearchResult> SearchAsync(QRequestSearch request);
        ValueTask<IQuelleFetchResult> FetchAsync(IQuelleFetchRequest request);
        ValueTask<IQuellePageResult> PageAsync(QRequestPage request);
        ValueTask<string> TestAsync(string request);
    }
    public interface ISearchProvider
    {
        IQuelleStatusResult Status();
        IQuelleSearchResult Search(QRequestSearch request);
        IQuelleFetchResult Fetch(QRequestFetch request);
        IQuellePageResult Page(QRequestPage request);
        string Test(string request);
    }
    public interface ISearchProviderVanilla
    {
        IQuelleStatusResult Status();
        IQuelleSearchResult Search(IQuelleSearchRequest request);
        IQuelleFetchResult Fetch(IQuelleFetchRequest request);
        IQuellePageResult Page(IQuellePageRequest request);
        string Test(string request);
    }

    public class SearchProviderClient
    {
        private String baseUrl;
        public ISearchProvider api { get; private set; }
        public ISearchProviderVanilla vanilla { get; private set; }

        public class QuelleSearchProvider: ISearchProvider
        {
            private SearchProviderClient outer;
            public QuelleSearchProvider(SearchProviderClient outer)  {
                this.outer = outer;
            }
            public IQuelleStatusResult Status() {
                return outer.Status();
            }
            public IQuelleSearchResult Search(QRequestSearch request)  {
                return outer.Search(request);
                //outer.Test("foo");
                //return null;
            }
            public IQuelleFetchResult Fetch(QRequestFetch request) {
                return outer.Fetch(request);
            }
            public IQuellePageResult Page(QRequestPage request) {       // GET HTML, TEXT, or MD page
                return outer.Page(request);
            }
            public string Test(string request) {
                return outer.Test(request);
            }
        }
        public class QuelleSearchProviderVanilla : ISearchProviderVanilla
        {
            private SearchProviderClient outer;
            public QuelleSearchProviderVanilla(SearchProviderClient outer)
            {
                this.outer = outer;
            }
            public IQuelleStatusResult Status()
            {
                return outer.Status();
            }
            public IQuelleSearchResult Search(IQuelleSearchRequest request)
            {
                var qrequest = new QRequestSearch(request);
                return outer.Search(qrequest);
            }
            public IQuelleFetchResult Fetch(IQuelleFetchRequest request)
            {
                var qrequest = new QRequestFetch(request);
                return outer.Fetch(qrequest);
            }
            public IQuellePageResult Page(IQuellePageRequest request)
            {       // GET HTML, TEXT, or MD page
                var qrequest = new QRequestPage(request);
                return outer.Page(qrequest);
            }
            public string Test(string request)
            {
                return outer.Test(request);
            }
        }
        public SearchProviderClient(string host)
        {
            this.baseUrl = host != null ? host : "http://127.0.0.1:7878/";
            if (!this.baseUrl.EndsWith('/'))
                this.baseUrl += '/';
            this.api = new QuelleSearchProvider(this);
            this.vanilla = new QuelleSearchProviderVanilla(this);
        }
        internal const string mimetype = "application/msgpack";
        internal IQuelleSearchResult Search(QRequestSearch req)
        {
            var brief = new QRequestSearchBrief(req);

            var cloud = new QWebClient(this.baseUrl);
            if (cloud != null)
            {
                try
                {
                    var payload = MessagePackSerializer.Serialize(brief);
                    var packedRespospone = cloud.Post("/search", payload, mimetype);
                    var response = MessagePackSerializer.Deserialize<IQuelleSearchResult>(packedRespospone.data);
                    return response;
                }
                catch (Exception ex)
                {
                    var bad = new QSearchResult();
                    bad.errors = new string[] { "Unable to pack message (befor calling search provider)" };
                    bad.success = false;
                    return bad;
                }
            }
            return null;
        }
        public IQuelleFetchResult Fetch(QRequestFetch req)
        {
            return null;
        }
        public IQuellePageResult Page(QRequestPage req)
        {
            return null;
        }
        public IQuelleStatusResult Status()
        {
            var cloud = new QWebClient(this.baseUrl);

            var result = cloud.Get("/status");
            Console.WriteLine("Result from get /status:");
            Console.WriteLine(result);

            return null;
        }
        public string Test(string req)
        {
            var cloud = new QWebClient(this.baseUrl);

            var result = cloud.Get("/");
            Console.WriteLine("Result from get /:");
            Console.WriteLine(result);

            return result;
        }
    }
}
using Utf8Json;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace QuelleHMI
{
    // REQUESTS
    //
    public interface ISearchProviderAsync
    {
        //ValueTask<IQuelleStatusResult> StatusAsync();
        ValueTask<IQuelleSearchResult> SearchAsync(QRequestSearch request);
        ValueTask<IQuellePageResult> PageAsync(QRequestPage request);
        ValueTask<string> TestAsync(string request);
    }
    public interface ISearchProvider
    {
        //IQuelleStatusResult Status();
        IQuelleSearchResult Search(QRequestSearch request);
        IQuellePageResult Page(QRequestPage request);
        string Test(string request);
    }
    public class AbstractQuelleSearchResult: IQuelleSearchResult    // for C++/CLI support
    {
        //                b                c     v [compact bit array]        
        public virtual Dictionary<byte, Dictionary<byte, UInt32>> matches { get; }
        public virtual string summary { get; }
        public Guid session { get; } // MD5/GUID
        public Dictionary<UInt32, String> abstracts { get; }
        public UInt64 cursor { get; }
        public virtual UInt64 count { get; }
        public UInt64 remainder { get; }
        public Dictionary<string, List<string>> messages { get; set; }
        public void AddWarning(string message)
        {
            if (this.messages == null)
                this.messages = new Dictionary<string, List<string>>();
            var list = this.messages.ContainsKey("warnings") ? this.messages["warnings"] : null;
            if (list == null)
            {
                list = new List<string>();
                this.messages.Add("warnings", list);
            }
            list.Add(message);
        }
        public void AddError(string message)
        {
            if (this.messages == null)
                this.messages = new Dictionary<string, List<string>>();
            var list = this.messages.ContainsKey("errors") ? this.messages["errors"] : null;
            if (list == null)
            {
                list = new List<string>();
                this.messages.Add("errors", list);
            }
            list.Add(message);
        }
    }
    public class AbstractQuellePageResult: IQuellePageResult    // for C++/CLI support
    {
        public string result { get; }
        public Dictionary<string, List<string>> messages { get; }
    }
    public abstract class AbstractQuelleSearchProvider    // for C++/CLI support
    {
        //IQuelleStatusResult Status();
        public abstract IQuelleSearchResult Search(QRequestSearch request);
        public abstract IQuellePageResult Page(QRequestPage request);
        public abstract string Test(string request);
    }
    public class InstantiatedQuelleSearchProvider: ISearchProvider    // for C++/CLI support
    {
        private AbstractQuelleSearchProvider instance;
        public InstantiatedQuelleSearchProvider(AbstractQuelleSearchProvider provider)
        {
            this.instance = provider;
        }
        //IQuelleStatusResult Status();
        public IQuelleSearchResult Search(QRequestSearch request)
        {
            if (instance != null)
            {
                var result = instance.Search(request);  // if exception is thrown here, recompile the C++ code
                return result;
            }
            else
            {
                Console.Out.WriteLine("Error: " + "Could not locate a Quelle search provider/instance");
                return null;
            }
        }
        public IQuellePageResult Page(QRequestPage request)
        {
            return instance != null ? instance.Page(request) : null;
        }
        public string Test(string request)
        {
            return instance != null ? instance.Test(request) : null;
        }
    }
    public interface ISearchProviderVanilla
    {
        //IQuelleStatusResult Status();
        IQuelleSearchResult Search(IQuelleSearchRequest request);
        IQuellePageResult Page(IQuellePageRequest request);
        string Test(string request);
    }

    public class SearchProviderClient: ISearchProvider
    {
        private String baseUrl;
        public ISearchProvider api { get; private set; }
        public ISearchProviderVanilla vanilla { get; private set; }

        public class QuelleSearchProvider: ISearchProvider
        {
            private ISearchProvider outer;
            public QuelleSearchProvider(ISearchProvider outer)  {
                this.outer = outer;
            }
            //public IQuelleStatusResult Status() {
            //    return outer.Status();
            //}
            public IQuelleSearchResult Search(QRequestSearch request)  {
                return this.outer.Search(request);
                //outer.Test("foo");
                //return null;
            }
            public IQuellePageResult Page(QRequestPage request) {       // GET HTML, TEXT, or MD page
                return this.outer.Page(request);
            }
            public string Test(string request) {
                return this.outer.Test(request);
            }
        }
        public class QuelleSearchProviderVanilla : ISearchProviderVanilla
        {
            private SearchProviderClient outer;
            public QuelleSearchProviderVanilla(SearchProviderClient outer)
            {
                this.outer = outer;
            }
            //public IQuelleStatusResult Status()
            //{
            //    return outer.Status();
            //}
            public IQuelleSearchResult Search(IQuelleSearchRequest request)
            {
                var qrequest = new QRequestSearch(request);
                return outer.Search(qrequest);
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
        public SearchProviderClient(ISearchProvider inprocProvider)
        {
            this.baseUrl = "";
            if (!this.baseUrl.EndsWith("/"))
                this.baseUrl += "/";
            this.api = new QuelleSearchProvider(inprocProvider);
            this.vanilla = new QuelleSearchProviderVanilla(this);
        }
        public SearchProviderClient(string host)
        {
            this.baseUrl = host != null ? host : "http://127.0.0.1:7878/";
            if (!this.baseUrl.EndsWith("/"))
                this.baseUrl += "/";
            this.api = new QuelleSearchProvider(this);
            this.vanilla = new QuelleSearchProviderVanilla(this);
        }
        internal const string mimetype = "application/json";
        public IQuelleSearchResult Search(QRequestSearch req)
        {
            var cloud = new QWebClient(this.baseUrl);
            if (cloud != null)
            {
                try
                {
                    var payload = JsonSerializer.Serialize(req);
                    var packedResponse = cloud.Post("/search", payload, mimetype);
                    var str = System.Text.Encoding.Default.GetString(packedResponse.data).Trim();
                    var response = JsonSerializer.Deserialize<QSearchResult>(packedResponse.data);
                    return response;
                }
                catch (Exception ex)
                {
                    var bad = new QSearchResult();
                    bad.AddError("Unable to pack message (before calling search provider)");
                    return bad;
                }
            }
            return null;
        }
        public IQuellePageResult Page(QRequestPage req)
        {
            return null;
        }
        public string Status()
        {
            var cloud = new QWebClient(this.baseUrl);

            var result = cloud.Get("/status");
            Console.WriteLine("Result from get /status:");
            Console.WriteLine(result);

            return result;
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
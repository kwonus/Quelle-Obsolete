using Grpc.Net.Client;
using QuelleHMI.Controls;
using QuelleHMI.Verbs;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace QuelleHMI
{
    // REQUESTS
    //
    [ServiceContract]
    public interface ISearchProvider
    {
        ValueTask<PBStatusResult> StatusAsync(PBStatusRequest request);
        ValueTask<PBSearchResult> SearchAsync(PBSearchRequest request);
        ValueTask<PBFetchResult> FetchAsync(PBFetchRequest request);
        ValueTask<PBPageResult> PageAsync(PBPageRequest request);
    }
    class SearchProviderClient
    {
        private GrpcChannel channel;
        ISearchProvider searchprovider;

        public SearchProviderClient()
        {
            (int c, int r)[] foo = new (int c, int r)[3];

            this.channel = GrpcChannel.ForAddress("http://localhost:10042");

 //         this.searchprovider = this.channel.CreateGrpcService<ISearchProvider>();
        }
        public IQuelleSearchResult Search(IQuelleSearchRequest request)
        {
            return null;
        }
        public IQuelleFetchResult Fetch(IQuelleFetchRequest request)
        {
            return null;
        }
        public IQuellePageResult Page(IQuellePageRequest request)
        {
            return null;
        }
    }
}

using Grpc.Net.Client;
using ProtoBuf.Grpc.Client;
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
    public class SearchProviderClient
    {
        private GrpcChannel channel;
        ISearchProvider searchprovider;

        public SearchProviderClient()
        {
            this.channel = GrpcChannel.ForAddress("http://localhost:50051");

            //this.searchprovider = this.channel.CreateGrpcService<ISearchProvider>();
        }
        public IQuelleSearchResult Search(IQuelleSearchRequest req)
        {
            PBSearchRequest pbrequest = new PBSearchRequest(req);
            var request = this.searchprovider.SearchAsync(pbrequest);
            request.AsTask().Wait();
            if (request.IsCompletedSuccessfully)
                return request.Result;
            return null;
        }
        public IQuelleFetchResult Fetch(IQuelleFetchRequest req)
        {
            PBFetchRequest pbrequest = new PBFetchRequest(req);
            var request = this.searchprovider.FetchAsync(pbrequest);
            request.AsTask().Wait();
            if (request.IsCompletedSuccessfully)
                return request.Result;
            return null;
        }
        public IQuellePageResult Page(IQuellePageRequest req)
        {
            PBPageRequest pbrequest = new PBPageRequest(req);
            var request = this.searchprovider.PageAsync(pbrequest);
            request.AsTask().Wait();
            if (request.IsCompletedSuccessfully)
                return request.Result;
            return null;
        }
        public IQuelleStatusResult Status(IQuelleStatusRequest req)
        {
            PBStatusRequest pbrequest = new PBStatusRequest(req);
            var request = this.searchprovider.StatusAsync(pbrequest);
            request.AsTask().Wait();
            if (request.IsCompletedSuccessfully)
                return request.Result;
            return null;
        }
    }
}

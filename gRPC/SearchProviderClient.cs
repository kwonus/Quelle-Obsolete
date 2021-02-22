using Grpc.Net.Client;
using ProtoBuf;
using ProtoBuf.Grpc.Client;
using QuelleHMI.Controls;
using QuelleHMI.Verbs;
using System;
using System.Collections.Generic;
using System.IO;
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
        [OperationContract]
        ValueTask<PBStatusResult> StatusAsync(PBStatusRequest request);
        [OperationContract]
        ValueTask<PBSearchResult> SearchAsync(PBSearchRequest request);
        [OperationContract]
        ValueTask<PBFetchResult> FetchAsync(PBFetchRequest request);
        [OperationContract]
        ValueTask<PBPageResult> PageAsync(PBPageRequest request);
    }
    public class SearchProviderClient
    {
        public SearchProviderClient()
        {
//          this.channel = GrpcChannel.ForAddress("http://[::1]:50051");
//          this.searchprovider = this.channel.CreateGrpcService<ISearchProvider>();
        }
        public IQuelleSearchResult Search(IQuelleSearchRequest req)
        {
            AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);
            using (var channel = GrpcChannel.ForAddress("http://[::1]:50051"))
            {
                var searchprovider = channel.CreateGrpcService<ISearchProvider>();
                var pbrequest = new PBSearchRequest(req);
                byte[] msgOut;

                using (var stream = new MemoryStream())
                {
                    Serializer.Serialize(stream, pbrequest);
                    msgOut = stream.GetBuffer();
                }

                var request = searchprovider.SearchAsync(pbrequest);
                request.AsTask().Wait();
                if (request.IsCompletedSuccessfully)
                    return request.Result;
            }
            return null;
        }
        public IQuelleFetchResult Fetch(IQuelleFetchRequest req)
        {/*
            PBFetchRequest pbrequest = new PBFetchRequest(req);
            var request = searchprovider.FetchAsync(pbrequest);
            request.AsTask().Wait();
            if (request.IsCompletedSuccessfully)
                return request.Result;*/
            return null;
        }
        public IQuellePageResult Page(IQuellePageRequest req)
        {/*
            PBPageRequest pbrequest = new PBPageRequest(req);
            var request = this.searchprovider.PageAsync(pbrequest);
            request.AsTask().Wait();
            if (request.IsCompletedSuccessfully)
                return request.Result;*/
            return null;
        }
        public IQuelleStatusResult Status(IQuelleStatusRequest req)
        {/*
            PBStatusRequest pbrequest = new PBStatusRequest(req);
            var request = this.searchprovider.StatusAsync(pbrequest);
            request.AsTask().Wait();
            if (request.IsCompletedSuccessfully)
                return request.Result;*/
            return null;
        }
    }
}

using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Modix.Services.Cat
{
    public interface IHttpClient
    {
        Task<HttpResponseMessage> GetAsync(string uri, CancellationToken cancellationToken = default);
        void AddHeader(string id, string value);
    }

    public class HttpClient : IHttpClient
    {
        private static readonly System.Net.Http.HttpClient Client = new System.Net.Http.HttpClient();

        public Task<HttpResponseMessage> GetAsync(string uri, CancellationToken cancellationToken)
            => Client.GetAsync(uri, cancellationToken);

        public void AddHeader(string id, string value)
            => Client.DefaultRequestHeaders.Add(id, value);
    }
}
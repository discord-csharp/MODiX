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
        private readonly System.Net.Http.HttpClient _client = new System.Net.Http.HttpClient();

        public Task<HttpResponseMessage> GetAsync(string uri, CancellationToken cancellationToken)
            => _client.GetAsync(uri, cancellationToken);

        public void AddHeader(string id, string value)
            => _client.DefaultRequestHeaders.Add(id, value);
    }
}
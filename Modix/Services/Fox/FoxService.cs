namespace Modix.Services.Fox
{
    using System;
    using System.Net;
    using System.Net.Http;
    using System.Threading.Tasks;

    public interface IFoxService
    {
        /// <summary>
        /// Gets a random fox image URL
        /// </summary>
        /// <returns>Fox image URL</returns>
        Task<string> GetFoxPicture();
    }

    public class FoxService : IFoxService
    {
        private const string FoxApiUrl = "https://giraffeduck.com/api/fox/direct";
        private readonly HttpClient _client;

        public FoxService()
        {
            var handler = new HttpClientHandler
            {
                AllowAutoRedirect = false
            };

            _client = new HttpClient(handler)
            {
                Timeout = TimeSpan.FromSeconds(5)
            };
        }

        public async Task<string> GetFoxPicture()
        {
            var response = await _client.GetAsync(FoxApiUrl);

            if (response.StatusCode == HttpStatusCode.Redirect)
            {
                return response.Headers.Location.AbsoluteUri;
            }
            else
            {
                throw new Exception("Invalid data received from API.");
            }
        }
    }
}

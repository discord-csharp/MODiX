namespace Modix.Services.Fox
{
    using System;
    using System.Net;
    using System.Net.Http;
    using System.Threading.Tasks;

    public class FoxService
    {
        private const string FoxApiUrl = "https://giraffeduck.com/api/fox/direct";

        public async Task<string> GetFoxPicture()
        {
            var handler = new HttpClientHandler
            {
                AllowAutoRedirect = false
            };

            var client = new HttpClient(handler)
            {
                Timeout = TimeSpan.FromSeconds(5)
            };

            using (client)
            {
                var response = await client.GetAsync(FoxApiUrl);

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
}

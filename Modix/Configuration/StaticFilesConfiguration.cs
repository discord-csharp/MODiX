using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace Modix.Configuration
{
    public class StaticFilesConfiguration : IConfigureOptions<StaticFileOptions>
    {
        private readonly IAntiforgery antiforgery;

        public StaticFilesConfiguration(IAntiforgery antiforgery)
        {
            this.antiforgery = antiforgery;
        }

        public void Configure(StaticFileOptions options)
        {
            //Set up our antiforgery stuff when the user hits the page
            options.OnPrepareResponse =
                fileResponse =>
                {
                    if (fileResponse.File.Name == "index.html")
                    {
                        var tokens = antiforgery.GetAndStoreTokens(fileResponse.Context);

                        fileResponse.Context.Response.Cookies.Append(
                            "XSRF-TOKEN", tokens.RequestToken, new CookieOptions() { HttpOnly = false });
                    }
                };
        }
    }
}

using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace Modix.Configuration
{
    public class StaticFilesConfiguration : IConfigureOptions<StaticFileOptions>
    {
        private readonly IAntiforgery antiforgery;
        private readonly IWebHostEnvironment _env;

        public StaticFilesConfiguration(IAntiforgery antiforgery, IWebHostEnvironment env)
        {
            this.antiforgery = antiforgery;
            _env = env;
        }

        public void Configure(StaticFileOptions options)
        {
            var cachePeriod = _env.IsDevelopment() ? "600" : "604800";

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

                    fileResponse.Context.Response.Headers.Append("Cache-Control", $"public, max-age={cachePeriod}");
                };
        }
    }
}

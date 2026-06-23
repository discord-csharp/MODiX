using MudBlazor;
using MudBlazor.Services;

namespace Modix.Web;

public static class Setup
{
    public static WebApplication ConfigureBlazorApplication(this WebApplication app)
    {
        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Error");
            app.UseHsts();
        }

        app.UseHttpsRedirection();

        app.UseStaticFiles();

        app.UseRouting();

        app.MapBlazorHub();
        app.MapFallbackToPage("/_Host");

        return app;
    }

    public static IServiceCollection ConfigureBlazorServices(this IServiceCollection services)
    {
        services.AddMudServices();

        services.AddRazorPages();
        services.AddServerSideBlazor();

        return services;
    }
}

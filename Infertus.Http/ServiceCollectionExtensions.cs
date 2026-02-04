using Microsoft.Extensions.DependencyInjection;

namespace Infertus.Http;

public static class ServiceCollectionExtensions2
{
    public static IServiceCollection AddInfertusHttpService(this IServiceCollection services, string referer, string userAgent)
    {
        services.AddHttpClient<IHttpService, HttpService>(client =>
        {
            client.DefaultRequestHeaders.Referrer = new Uri(referer);
            client.DefaultRequestHeaders.UserAgent.ParseAdd(userAgent);
            client.DefaultRequestHeaders.Accept.ParseAdd("application/json");
        });
        return services;
    }
}

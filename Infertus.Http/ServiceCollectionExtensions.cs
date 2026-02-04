using Microsoft.Extensions.DependencyInjection;
using System.Net.Http.Headers;

namespace Infertus.Http;

public static class ServiceCollectionExtensions2
{
    public static IServiceCollection AddInfertusHttpService(this IServiceCollection services, HttpClientConfig config)
    {
        if (config.BearerToken != null && config.Base64Authorization != null)
            throw new ArgumentException(
                $"Provided {nameof(HttpClientConfig)} has both, Bearer and Basic authorization methods set up!");

        services.AddHttpClient<IHttpService, HttpService>(client =>
        {
            if (config.BaseAddress != null)
                client.BaseAddress = new Uri(config.BaseAddress);

            if (config.Referer != null)
                client.DefaultRequestHeaders.Referrer = new Uri(config.Referer);

            if (config.UserAgent != null)
                client.DefaultRequestHeaders.UserAgent.ParseAdd(config.UserAgent);

            if (config.Accept != null)
                client.DefaultRequestHeaders.Accept.ParseAdd(config.Accept);
            else
                client.DefaultRequestHeaders.Accept.ParseAdd("application/json");

            if (config.BearerToken != null)
                client.DefaultRequestHeaders.Authorization = 
                    new AuthenticationHeaderValue("Bearer", config.BearerToken);

            if (config.Base64Authorization != null)
                client.DefaultRequestHeaders.Authorization = 
                    new AuthenticationHeaderValue("Basic", config.Base64Authorization);
        });

        return services;
    }
}

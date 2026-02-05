using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Headers;

namespace Infertus.Http;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfertusHttpService(
        this IServiceCollection services, HttpClientConfig config)
    {
        if (config == null)
            throw new ArgumentNullException(nameof(config));

        if (config.BearerToken != null && config.Base64Authorization != null)
            throw new ArgumentException(
                "HttpClientConfig cannot have both Bearer and Basic authentication configured.");

        services.AddHttpClient<IHttpService, HttpService>()
            .ConfigurePrimaryHttpMessageHandler(() =>
            {
                var handler = new HttpClientHandler();

                if (config.Cookies != null)
                {
                    handler.UseCookies = true;
                    handler.CookieContainer = config.Cookies;
                }

                if (config.EnableCompression)
                {
                    handler.AutomaticDecompression =
                        DecompressionMethods.GZip |
                        DecompressionMethods.Deflate |
                        DecompressionMethods.Brotli;
                }

                if (config.UseProxy && config.Proxy != null)
                {
                    handler.UseProxy = true;
                    handler.Proxy = config.Proxy;
                }

                if (config.ServerCertificateCustomValidation != null)
                {
                    handler.ServerCertificateCustomValidationCallback =
                        config.ServerCertificateCustomValidation;
                }

                return handler;
            })
            .ConfigureHttpClient(client =>
            {
                if (!string.IsNullOrWhiteSpace(config.BaseAddress))
                    client.BaseAddress = new Uri(config.BaseAddress);

                if (config.Timeout.HasValue)
                    client.Timeout = config.Timeout.Value;

                if (!string.IsNullOrWhiteSpace(config.Accept))
                    client.DefaultRequestHeaders.Accept.ParseAdd(config.Accept);
                else
                    client.DefaultRequestHeaders.Accept.ParseAdd("application/json");

                if (!string.IsNullOrWhiteSpace(config.UserAgent))
                    client.DefaultRequestHeaders.UserAgent.ParseAdd(config.UserAgent);

                if (!string.IsNullOrWhiteSpace(config.Referer))
                    client.DefaultRequestHeaders.Referrer = new Uri(config.Referer);

                if (!string.IsNullOrWhiteSpace(config.BearerToken))
                {
                    client.DefaultRequestHeaders.Authorization =
                        new AuthenticationHeaderValue("Bearer", config.BearerToken.Trim());
                }
                else if (!string.IsNullOrWhiteSpace(config.Base64Authorization))
                {
                    client.DefaultRequestHeaders.Authorization =
                        new AuthenticationHeaderValue("Basic", config.Base64Authorization);
                }

                // Custom headers
                foreach (var header in config.DefaultHeaders)
                {
                    client.DefaultRequestHeaders.TryAddWithoutValidation(
                        header.Key, header.Value);
                }
            });

        return services;
    }
}

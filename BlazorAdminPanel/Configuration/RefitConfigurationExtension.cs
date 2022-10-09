using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http.Logging;
using Refit;
using System;
using System.Text.Json;

namespace BlazorAdminPanel.Configuration
{
    public static class RefitConfigurationExtension
    {
        private static readonly RefitSettings _settings;

        static RefitConfigurationExtension()
        {
            _settings = new RefitSettings
            {
                ContentSerializer = new SystemTextJsonContentSerializer(new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    WriteIndented = true
                })
            };
        }

        public static IHttpClientBuilder AddRefitClient<TClient>(this IServiceCollection services, Uri serviceUri)
            where TClient : class
        {
            return services.AddRefitClient<TClient>(_settings)
                .ConfigureHttpClient(client =>
                {
                    client.BaseAddress = serviceUri;
                })
                .AddHttpMessageHandler<LoggingHttpMessageHandler>();
        }
    }
}

using Annium.Net.Http;
using Annium.Net.Http.Internal;
using Microsoft.Extensions.DependencyInjection;

namespace Annium.Core.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static void AddHttpRequestFactory(this IServiceCollection services)
        {
            services.AddSingleton<IHttpRequestFactory, HttpRequestFactory>();
            services.AddSingleton<IHttpContentSerializer, HttpContentSerializer>();
        }
    }
}
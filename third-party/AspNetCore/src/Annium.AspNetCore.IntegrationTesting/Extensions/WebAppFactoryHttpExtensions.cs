using System.Collections.Concurrent;
using Annium.Core.DependencyInjection;
using Annium.Net.Http;

namespace Annium.AspNetCore.IntegrationTesting
{
    public static class WebAppFactoryHttpExtensions
    {
        private static readonly ConcurrentDictionary<IWebApplicationFactory, IHttpRequest> Cache = new();

        public static IHttpRequest GetHttpRequest(
            this IWebApplicationFactory appFactory
        ) =>
            Cache.GetOrAdd(appFactory, factory =>
            {
                var httpClient = factory.CreateClient();
                var httpRequestFactory = factory.ServiceProvider.Resolve<IHttpRequestFactory>();

                var request = httpRequestFactory.New().UseClient(httpClient);

                return request;
            }).Clone();
    }
}
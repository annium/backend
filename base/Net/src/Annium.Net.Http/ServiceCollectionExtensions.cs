using Annium.Net.Http;
using Annium.Net.Http.Internal;

namespace Annium.Core.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceContainer AddHttpRequestFactory(this IServiceContainer container)
        {
            container.Add<IHttpRequestFactory, HttpRequestFactory>().Singleton();
            container.Add<IHttpContentSerializer, HttpContentSerializer>().Singleton();

            return container;
        }
    }
}
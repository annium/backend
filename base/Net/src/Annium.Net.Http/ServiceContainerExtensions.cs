using Annium.Net.Http;
using Annium.Net.Http.Internal;

// ReSharper disable once CheckNamespace
namespace Annium.Core.DependencyInjection;

public static class ServiceContainerExtensions
{
    public static IHttpFactoryConfigurationBuilder AddHttpRequestFactory(this IServiceContainer container)
    {
        return new HttpFactoryConfigurationBuilder(container, Constants.DefaultKey);
    }

    public static IHttpFactoryConfigurationBuilder AddHttpRequestFactory(this IServiceContainer container, string key)
    {
        return new HttpFactoryConfigurationBuilder(container, key);
    }
}
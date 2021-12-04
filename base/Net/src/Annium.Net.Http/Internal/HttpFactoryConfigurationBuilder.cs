using System.Linq;
using Annium.Core.DependencyInjection;
using Annium.Logging.Abstractions;
using Annium.Serialization.Abstractions;

namespace Annium.Net.Http.Internal;

internal class HttpFactoryConfigurationBuilder : IHttpFactoryConfigurationBuilder
{
    private readonly IServiceContainer _container;
    private readonly string _key;

    public HttpFactoryConfigurationBuilder(IServiceContainer container, string key)
    {
        _container = container;
        _key = key;

        _container.Add<IHttpContentSerializer>(sp =>
        {
            var serializers = sp.Resolve<IIndex<SerializerKey, ISerializer<string>>>()
                .Where(x => x.Key.Key == _key)
                .ToIndex(x => x.Key.Type, x => x.Value);

            return new HttpContentSerializer(serializers);
        }).AsKeyed<IHttpContentSerializer, string>(_key).Singleton();

        _container.Add<IHttpRequestFactory>(sp =>
        {
            var serializer = sp.Resolve<IIndex<string, IHttpContentSerializer>>()[_key];
            var logger = sp.Resolve<ILogger<IHttpRequest>>();

            return new HttpRequestFactory(serializer, logger);
        }).AsKeyed<IHttpRequestFactory, string>(_key).Singleton();
    }

    public IHttpFactoryConfigurationBuilder SetDefault()
    {
        // default serializer+factory
        _container.Add(sp => sp.Resolve<IIndex<string, IHttpContentSerializer>>()[_key]).As<IHttpContentSerializer>().Singleton();
        _container.Add(sp => sp.Resolve<IIndex<string, IHttpRequestFactory>>()[_key]).As<IHttpRequestFactory>().Singleton();

        return this;
    }
}
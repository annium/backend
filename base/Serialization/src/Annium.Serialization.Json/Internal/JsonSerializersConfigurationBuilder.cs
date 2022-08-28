using System;
using System.Text.Json;
using Annium.Core.DependencyInjection;
using Annium.Core.Runtime.Types;
using Annium.Serialization.Abstractions;

namespace Annium.Serialization.Json.Internal;

internal class JsonSerializersConfigurationBuilder : IJsonSerializersConfigurationBuilder
{
    private readonly IServiceContainer _container;
    private readonly SerializerKey _fullKey;

    public JsonSerializersConfigurationBuilder(IServiceContainer container, string key)
    {
        _container = container;
        _fullKey = SerializerKey.Create(key, Constants.MediaType);

        Func<IServiceProvider, T> OptionsResolvingFactory<T>(string shortKey, Func<IServiceProvider, OptionsContainer, T> factory)
        {
            return sp =>
            {
                var index = sp.Resolve<IIndex<string, OptionsContainer>>();
                if (!index.TryGetValue(shortKey, out var opts))
                    opts = sp.Resolve<OptionsContainer>();
                return factory(sp, opts);
            };
        }

        _container.Add<ISerializer<byte[]>>(OptionsResolvingFactory(key,
                (_, opts) => new ByteArraySerializer(opts)
            ))
            .AsKeyed<ISerializer<byte[]>, SerializerKey>(_fullKey).Singleton();
        _container.Add<ISerializer<ReadOnlyMemory<byte>>>(OptionsResolvingFactory(key,
                (_, opts) => new ReadOnlyMemoryByteSerializer(opts)
            ))
            .AsKeyed<ISerializer<ReadOnlyMemory<byte>>, SerializerKey>(_fullKey).Singleton();
        _container.Add<ISerializer<string>>(OptionsResolvingFactory(key,
                (_, opts) => new StringSerializer(opts)
            ))
            .AsKeyed<ISerializer<string>, SerializerKey>(_fullKey).Singleton();

        // default configuration
        _container.Add(sp =>
        {
            var opts = new JsonSerializerOptions();
            opts.ConfigureDefault(sp.Resolve<ITypeManager>());

            return new OptionsContainer(opts);
        }).AsSelf().Singleton();
    }

    public IJsonSerializersConfigurationBuilder Configure(Action<JsonSerializerOptions> configure)
        => Configure((_, opts) => configure(opts));

    public IJsonSerializersConfigurationBuilder Configure(Action<IServiceProvider, JsonSerializerOptions> configure)
    {
        _container.Add(sp =>
        {
            var opts = new JsonSerializerOptions();
            opts.ConfigureDefault(sp.Resolve<ITypeManager>());
            configure(sp, opts);

            return new OptionsContainer(opts);
        }).AsKeyed<OptionsContainer, string>(_fullKey.Key).Singleton();

        return this;
    }

    public IJsonSerializersConfigurationBuilder SetDefault()
    {
        // default serializer for key+type
        _container.Add(sp => sp.Resolve<IIndex<SerializerKey, ISerializer<byte[]>>>()[_fullKey]).As<ISerializer<byte[]>>().Singleton();
        _container.Add(sp => sp.Resolve<IIndex<SerializerKey, ISerializer<ReadOnlyMemory<byte>>>>()[_fullKey]).As<ISerializer<ReadOnlyMemory<byte>>>().Singleton();
        _container.Add(sp => sp.Resolve<IIndex<SerializerKey, ISerializer<string>>>()[_fullKey]).As<ISerializer<string>>().Singleton();

        return this;
    }
}
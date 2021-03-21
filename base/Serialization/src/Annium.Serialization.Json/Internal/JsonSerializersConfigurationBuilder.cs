using System;
using System.Text.Json;
using Annium.Core.DependencyInjection;
using Annium.Core.Runtime.Types;
using Annium.Serialization.Abstractions;

namespace Annium.Serialization.Json.Internal
{
    internal class JsonSerializersConfigurationBuilder : IJsonSerializersConfigurationBuilder
    {
        private readonly IServiceContainer _container;
        private readonly string _key;

        public JsonSerializersConfigurationBuilder(IServiceContainer container, string key)
        {
            _container = container;
            _key = key;
            Configure((_, _) => { });
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
            }).AsSelf().Singleton();

            _container.Add<ByteArraySerializer>().AsKeyed<ISerializer<byte[]>, string>(_key).Singleton();
            _container.Add<ReadOnlyMemoryByteSerializer>().AsKeyed<ISerializer<ReadOnlyMemory<byte>>, string>(_key).Singleton();
            _container.Add<StringSerializer>().AsKeyed<ISerializer<string>, string>(_key).Singleton();

            return this;
        }

        public IJsonSerializersConfigurationBuilder SetDefault()
        {
            _container.Add(sp => sp.Resolve<IIndex<string, ISerializer<byte[]>>>()[_key]).As<ISerializer<byte[]>>().Singleton();
            _container.Add(sp => sp.Resolve<IIndex<string, ISerializer<ReadOnlyMemory<byte>>>>()[_key]).As<ISerializer<ReadOnlyMemory<byte>>>().Singleton();
            _container.Add(sp => sp.Resolve<IIndex<string, ISerializer<string>>>()[_key]).As<ISerializer<string>>().Singleton();

            return this;
        }
    }
}
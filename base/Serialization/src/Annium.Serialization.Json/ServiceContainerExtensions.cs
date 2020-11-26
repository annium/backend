using System;
using System.Net.Mime;
using System.Text.Json;
using Annium.Core.Runtime.Types;
using Annium.Serialization.Abstractions;
using Annium.Serialization.Json.Internal;

namespace Annium.Core.DependencyInjection
{
    public static class ServiceContainerExtensions
    {
        private const string DefaultKey = MediaTypeNames.Application.Json;

        public static IServiceContainer AddJsonSerializers(
            this IServiceContainer container
        ) => container.AddJsonSerializers(DefaultKey, (_, _) => { });

        public static IServiceContainer AddJsonSerializers(
            this IServiceContainer container,
            Action<JsonSerializerOptions> configure
        ) => container.AddJsonSerializers(DefaultKey, (_, opts) => configure(opts));

        public static IServiceContainer AddJsonSerializers(
            this IServiceContainer container,
            Action<IServiceProvider, JsonSerializerOptions> configure
        ) => container.AddJsonSerializers(DefaultKey, configure);

        public static IServiceContainer AddJsonSerializers(
            this IServiceContainer container,
            string key,
            Action<JsonSerializerOptions> configure
        ) => container.AddJsonSerializers(key, (_, opts) => configure(opts));

        public static IServiceContainer AddJsonSerializers(
            this IServiceContainer container,
            string key,
            Action<IServiceProvider, JsonSerializerOptions> configure
        )
        {
            container.Add(sp =>
            {
                var opts = new JsonSerializerOptions();
                opts.ConfigureDefault(sp.Resolve<ITypeManager>());
                configure(sp, opts);

                return new OptionsContainer(opts);
            }).AsSelf().Singleton();

            container.Add<ByteArraySerializer>().AsKeyed<ISerializer<byte[]>, string>(key).Singleton();
            container.Add<StringSerializer>().AsKeyed<ISerializer<string>, string>(key).Singleton();

            return container;
        }
    }
}
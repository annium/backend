using System;
using System.Net.Mime;
using System.Text.Json;
using Annium.Serialization.Abstractions;
using Annium.Serialization.Json;
using Annium.Serialization.Json.Internal;

namespace Annium.Core.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceContainer AddJsonSerializers(
            this IServiceContainer container,
            Action<IServiceProvider, JsonSerializerOptions> configure
        )
        {
            container.Add(sp =>
            {
                var opts = new JsonSerializerOptions();
                configure(sp, opts);

                return new OptionsContainer(opts);
            }).AsSelf().Singleton();

            return container.AddSerializers();
        }

        public static IServiceContainer AddJsonSerializers(
            this IServiceContainer container,
            Action<JsonSerializerOptions> configure
        )
        {
            container.Add(_ =>
            {
                var opts = new JsonSerializerOptions();
                configure(opts);

                return new OptionsContainer(opts);
            }).AsSelf().Singleton();

            return container.AddSerializers();
        }

        private static IServiceContainer AddSerializers(
            this IServiceContainer container
        )
        {
            container.Add<ByteArraySerializer>()
                .AsKeyed<ISerializer<byte[]>, string>(Constants.Key)
                .AsKeyed<ISerializer<byte[]>, string>(MediaTypeNames.Application.Json)
                .Singleton();
            container.Add<StringSerializer>()
                .AsKeyed<ISerializer<string>, string>(Constants.Key)
                .AsKeyed<ISerializer<string>, string>(MediaTypeNames.Application.Json)
                .Singleton();

            return container;
        }
    }
}
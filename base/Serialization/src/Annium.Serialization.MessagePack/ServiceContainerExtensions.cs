using System;
using Annium.Serialization.Abstractions;
using Annium.Serialization.MessagePack.Internal;

namespace Annium.Core.DependencyInjection
{
    public static class ServiceContainerExtensions
    {
        public static IServiceContainer AddMessagePackSerializer(this IServiceContainer container) =>
            container.AddMessagePackSerializerInternal(Constants.DefaultKey);

        public static IServiceContainer AddMessagePackSerializer(this IServiceContainer container, string key) =>
            container.AddMessagePackSerializerInternal(key);

        private static IServiceContainer AddMessagePackSerializerInternal(this IServiceContainer container, string key)
        {
            var fullKey = SerializerKey.Create(key, Serialization.MessagePack.Constants.MediaType);
            container.Add<MessagePackSerializer>().AsKeyed<ISerializer<ReadOnlyMemory<byte>>, SerializerKey>(fullKey).Singleton();

            return container;
        }
    }
}
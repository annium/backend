using System;
using Annium.Serialization.Abstractions;
using Annium.Serialization.MessagePack;
using Annium.Serialization.MessagePack.Internal;

namespace Annium.Core.DependencyInjection
{
    public static class ServiceContainerExtensions
    {
        public static IServiceContainer AddMessagePackSerializer(this IServiceContainer container)
        {
            container.Add<MessagePackSerializer>().AsKeyed<ISerializer<ReadOnlyMemory<byte>>, string>(Constants.Key).Singleton();

            return container;
        }
    }
}
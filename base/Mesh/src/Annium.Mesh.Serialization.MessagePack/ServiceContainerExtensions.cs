using Annium.Core.DependencyInjection.Container;
using Annium.Core.DependencyInjection.Extensions;
using Annium.Data.Operations.Serialization.MessagePack;
using Annium.Mesh.Serialization.Abstractions;
using Annium.Serialization.Abstractions;
using Annium.Serialization.MessagePack;
using MessagePack;
using MessagePack.Resolvers;
using Constants = Annium.Mesh.Serialization.MessagePack.Internal.Constants;
using Serializer = Annium.Mesh.Serialization.MessagePack.Internal.Serializer;

namespace Annium.Mesh.Serialization.MessagePack;

public static class ServiceContainerExtensions
{
    public static IServiceContainer AddMeshMessagePackSerialization(
        this IServiceContainer container,
        ConfigureSerializer? configure = null
    )
    {
        container.Add<ISerializer, Serializer>().Singleton();
        container
            .AddSerializers(Constants.SerializerKey)
            .WithMessagePack(
                configure
                    ?? (
                        _ =>
                        {
                            var resolver = CompositeResolver.Create(
                                Resolver.Instance,
                                MessagePackSerializerOptions.Standard.Resolver
                            );
                            var opts = new MessagePackSerializerOptions(resolver).WithCompression(
                                MessagePackCompression.Lz4BlockArray
                            );

                            return opts;
                        }
                    )
            );

        return container;
    }
}

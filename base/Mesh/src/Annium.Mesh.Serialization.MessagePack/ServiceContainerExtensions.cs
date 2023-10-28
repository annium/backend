using Annium.Data.Operations.Serialization.MessagePack;
using Annium.Mesh.Serialization.Abstractions;
using Annium.Mesh.Serialization.MessagePack.Internal;
using MessagePack;
using MessagePack.Resolvers;

// ReSharper disable once CheckNamespace
namespace Annium.Core.DependencyInjection;

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

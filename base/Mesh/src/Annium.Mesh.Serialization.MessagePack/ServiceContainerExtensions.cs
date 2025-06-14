using Annium.Core.DependencyInjection;
using Annium.Data.Operations.Serialization.MessagePack;
using Annium.Mesh.Serialization.Abstractions;
using Annium.Serialization.Abstractions;
using Annium.Serialization.MessagePack;
using MessagePack;
using MessagePack.Resolvers;
using Constants = Annium.Mesh.Serialization.MessagePack.Internal.Constants;
using Serializer = Annium.Mesh.Serialization.MessagePack.Internal.Serializer;

namespace Annium.Mesh.Serialization.MessagePack;

/// <summary>
/// Provides extension methods for configuring MessagePack serialization for mesh communication.
/// </summary>
public static class ServiceContainerExtensions
{
    /// <summary>
    /// Adds mesh MessagePack serialization to the service container with optional custom configuration.
    /// Uses LZ4 block array compression by default for optimal performance.
    /// </summary>
    /// <param name="container">The service container to configure.</param>
    /// <param name="configure">Optional delegate to configure the MessagePack serializer options. If null, uses default configuration with LZ4 compression.</param>
    /// <returns>The configured service container.</returns>
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

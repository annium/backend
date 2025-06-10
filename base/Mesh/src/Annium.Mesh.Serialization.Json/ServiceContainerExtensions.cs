using Annium.Core.DependencyInjection.Container;
using Annium.Core.DependencyInjection.Extensions;
using Annium.Data.Operations.Serialization.Json;
using Annium.Mesh.Serialization.Abstractions;
using Annium.Mesh.Serialization.Json.Internal;
using Annium.Serialization.Abstractions;
using Annium.Serialization.Json;
using Constants = Annium.Mesh.Serialization.Json.Internal.Constants;
using Serializer = Annium.Mesh.Serialization.Json.Internal.Serializer;

namespace Annium.Mesh.Serialization.Json;

public static class ServiceContainerExtensions
{
    public static IServiceContainer AddMeshJsonSerialization(
        this IServiceContainer container,
        ConfigureSerializer configure
    )
    {
        return AddSerializers<Serializer>(container, configure);
    }

    public static IServiceContainer AddMeshJsonSerialization(this IServiceContainer container)
    {
        return AddSerializers<Serializer>(container);
    }

    public static IServiceContainer AddMeshJsonDebugSerialization(
        this IServiceContainer container,
        ConfigureSerializer configure
    )
    {
        return AddSerializers<DebugSerializer>(container, configure);
    }

    public static IServiceContainer AddMeshJsonDebugSerialization(this IServiceContainer container)
    {
        return AddSerializers<DebugSerializer>(container);
    }

    private static IServiceContainer AddSerializers<TSerializer>(
        IServiceContainer container,
        ConfigureSerializer configure
    )
        where TSerializer : ISerializer
    {
        container.Add<ISerializer, TSerializer>().Singleton();
        container
            .AddSerializers(Constants.SerializerKey)
            .WithJson(
                (sp, opts) =>
                {
                    opts.ConfigureForOperations();
                    configure(sp, opts);
                }
            );

        return container;
    }

    private static IServiceContainer AddSerializers<TSerializer>(IServiceContainer container)
        where TSerializer : ISerializer
    {
        container.Add<ISerializer, TSerializer>().Singleton();
        container.AddSerializers(Constants.SerializerKey).WithJson(opts => opts.ConfigureForOperations());

        return container;
    }
}

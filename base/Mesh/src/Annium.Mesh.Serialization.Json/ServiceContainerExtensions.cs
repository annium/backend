using Annium.Core.DependencyInjection;
using Annium.Data.Operations.Serialization.Json;
using Annium.Mesh.Serialization.Abstractions;
using Annium.Mesh.Serialization.Json.Internal;
using Annium.Serialization.Abstractions;
using Annium.Serialization.Json;
using Constants = Annium.Mesh.Serialization.Json.Internal.Constants;
using Serializer = Annium.Mesh.Serialization.Json.Internal.Serializer;

namespace Annium.Mesh.Serialization.Json;

/// <summary>
/// Provides extension methods for configuring JSON serialization for mesh communication.
/// </summary>
public static class ServiceContainerExtensions
{
    /// <summary>
    /// Adds mesh JSON serialization with custom configuration to the service container.
    /// </summary>
    /// <param name="container">The service container to configure.</param>
    /// <param name="configure">A delegate to configure the JSON serializer options.</param>
    /// <returns>The configured service container.</returns>
    public static IServiceContainer AddMeshJsonSerialization(
        this IServiceContainer container,
        ConfigureSerializer configure
    )
    {
        return AddSerializers<Serializer>(container, configure);
    }

    /// <summary>
    /// Adds mesh JSON serialization with default configuration to the service container.
    /// </summary>
    /// <param name="container">The service container to configure.</param>
    /// <returns>The configured service container.</returns>
    public static IServiceContainer AddMeshJsonSerialization(this IServiceContainer container)
    {
        return AddSerializers<Serializer>(container);
    }

    /// <summary>
    /// Adds mesh JSON debug serialization with custom configuration to the service container.
    /// Debug serialization includes additional logging for troubleshooting.
    /// </summary>
    /// <param name="container">The service container to configure.</param>
    /// <param name="configure">A delegate to configure the JSON serializer options.</param>
    /// <returns>The configured service container.</returns>
    public static IServiceContainer AddMeshJsonDebugSerialization(
        this IServiceContainer container,
        ConfigureSerializer configure
    )
    {
        return AddSerializers<DebugSerializer>(container, configure);
    }

    /// <summary>
    /// Adds mesh JSON debug serialization with default configuration to the service container.
    /// Debug serialization includes additional logging for troubleshooting.
    /// </summary>
    /// <param name="container">The service container to configure.</param>
    /// <returns>The configured service container.</returns>
    public static IServiceContainer AddMeshJsonDebugSerialization(this IServiceContainer container)
    {
        return AddSerializers<DebugSerializer>(container);
    }

    /// <summary>
    /// Configures the specified serializer type with custom JSON serialization options.
    /// </summary>
    /// <typeparam name="TSerializer">The type of serializer to register.</typeparam>
    /// <param name="container">The service container to configure.</param>
    /// <param name="configure">A delegate to configure the JSON serializer options.</param>
    /// <returns>The configured service container.</returns>
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

    /// <summary>
    /// Configures the specified serializer type with default JSON serialization options.
    /// </summary>
    /// <typeparam name="TSerializer">The type of serializer to register.</typeparam>
    /// <param name="container">The service container to configure.</param>
    /// <returns>The configured service container.</returns>
    private static IServiceContainer AddSerializers<TSerializer>(IServiceContainer container)
        where TSerializer : ISerializer
    {
        container.Add<ISerializer, TSerializer>().Singleton();
        container.AddSerializers(Constants.SerializerKey).WithJson(opts => opts.ConfigureForOperations());

        return container;
    }
}

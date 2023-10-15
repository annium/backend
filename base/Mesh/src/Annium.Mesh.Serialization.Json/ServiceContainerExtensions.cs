using Annium.Mesh.Serialization.Abstractions;
using Annium.Mesh.Serialization.Json.Internal;

// ReSharper disable once CheckNamespace
namespace Annium.Core.DependencyInjection;

public static class ServiceContainerExtensions
{
    public static IServiceContainer AddMeshJsonSerialization(this IServiceContainer container)
    {
        container.Add<ISerializer, Serializer>().Singleton();

        return container;
    }
    public static IServiceContainer AddMeshJsonDebugSerialization(this IServiceContainer container)
    {
        container.Add<ISerializer, DebugSerializer>().Singleton();

        return container;
    }
}
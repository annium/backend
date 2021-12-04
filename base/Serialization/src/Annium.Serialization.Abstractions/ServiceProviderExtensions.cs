using System;
using Annium.Core.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

namespace Annium.Serialization.Abstractions;

public static class ServiceProviderExtensions
{
    public static ISerializer<T> ResolveSerializer<T>(this IServiceProvider provider, string key, string type)
        => provider.GetRequiredService<IIndex<SerializerKey, ISerializer<T>>>()[SerializerKey.Create(key, type)];

    public static ISerializer<TSource, TDestination> ResolveSerializer<TSource, TDestination>(this IServiceProvider provider, string key, string type)
        => provider.GetRequiredService<IIndex<SerializerKey, ISerializer<TSource, TDestination>>>()[SerializerKey.Create(key, type)];
}
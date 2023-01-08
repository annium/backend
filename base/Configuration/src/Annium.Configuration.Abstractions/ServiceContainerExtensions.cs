using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Annium.Configuration.Abstractions;
using Annium.Configuration.Abstractions.Internal;
using Annium.Core.Reflection;

namespace Annium.Core.DependencyInjection;

public static class ServiceContainerExtensions
{
    public static IServiceContainer AddConfiguration<T>(
        this IServiceContainer container,
        T configuration
    )
        where T : class, new()
    {
        container.Add(configuration).AsSelf().Singleton();

        Register(container, typeof(T));

        return container;
    }

    public static IServiceContainer AddConfiguration<T>(
        this IServiceContainer container,
        Action<IConfigurationContainer> configure
    )
        where T : class, new()
    {
        container.AddConfigurationBuilder();

        var cfgContainer = new ConfigurationContainer();
        configure(cfgContainer);

        container.Add(sp =>
        {
            var builder = sp.Resolve<IConfigurationBuilder>();
            builder.Add(cfgContainer.Get());

            return builder.Build<T>();
        }).AsSelf().Singleton();

        Register(container, typeof(T));

        return container;
    }

    public static async Task<IServiceContainer> AddConfiguration<T>(
        this IServiceContainer container,
        Func<IConfigurationContainer, Task> configure
    )
        where T : class, new()
    {
        container.AddConfigurationBuilder();

        var cfgContainer = new ConfigurationContainer();
        await configure(cfgContainer);

        container.Add(sp =>
        {
            var builder = sp.Resolve<IConfigurationBuilder>();
            builder.Add(cfgContainer.Get());

            return builder.Build<T>();
        }).AsSelf().Singleton();

        Register(container, typeof(T));

        return container;
    }

    private static void AddConfigurationBuilder(
        this IServiceContainer container
    )
    {
        container.Add<IConfigurationBuilder, ConfigurationBuilder>().AsFactory<IConfigurationBuilder>().Transient();
    }

    private static void Register(
        IServiceContainer container,
        Type type
    )
    {
        foreach (var property in GetRegisteredProperties(type))
            Register(container, type, property);
    }

    private static void Register(
        IServiceContainer container,
        Type type,
        PropertyInfo property
    )
    {
        var propertyType = property.PropertyType;
        container.Add(propertyType, sp => property.GetValue(sp.Resolve(type))!).AsSelf().Singleton();

        foreach (var prop in GetRegisteredProperties(propertyType))
            Register(container, propertyType, prop);
    }

    private static IReadOnlyCollection<PropertyInfo> GetRegisteredProperties(Type type) => type
        .GetProperties()
        .Where(x =>
            x.CanRead && !x.PropertyType.IsEnum && !x.PropertyType.IsValueType && !x.PropertyType.IsPrimitive && !x.PropertyType.IsDerivedFrom(typeof(IEnumerable<>))
        )
        .ToArray();
}
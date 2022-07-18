using System;
using System.Linq;
using System.Reflection;
using Annium.Core.Runtime.Types;
using Annium.linq2db.Extensions.Configuration;
using Annium.linq2db.Extensions.Internal.Configuration.Extensions;
using LinqToDB.Mapping;

namespace Annium.linq2db.Extensions.Internal.Configuration;

internal class MappingBuilder : IMappingBuilder
{
    private readonly Assembly _configurationsAssembly;
    private readonly MappingSchema _schema;
    private readonly Lazy<(Type configurationType, Type entityType)[]> _configurations;

    public MappingBuilder(
        Assembly configurationsAssembly,
        MappingSchema schema
    )
    {
        _configurationsAssembly = configurationsAssembly;
        _schema = schema;
        _configurations = new Lazy<(Type configurationType, Type entityType)[]>(CollectConfigurations);
    }

    public IMappingBuilder ApplyConfigurations()
    {
        var fluentMappingBuilder = _schema.GetFluentMappingBuilder();

        var entityMappingBuilderFactory = typeof(FluentMappingBuilder).GetMethod(nameof(FluentMappingBuilder.Entity))!;

        foreach (var (configurationType, entityType) in _configurations.Value)
        {
            var entityMappingBuilder = entityMappingBuilderFactory.MakeGenericMethod(entityType)
                .Invoke(fluentMappingBuilder, new object?[] { null })!;
            var configureMethod = typeof(IEntityConfiguration<>).MakeGenericType(entityType)
                .GetMethod(nameof(IEntityConfiguration<object>.Configure))!;
            var configuration = Activator.CreateInstance(configurationType)!;
            configureMethod.Invoke(configuration, new[] { entityMappingBuilder });
        }

        _schema.IncludeAssociationKeysAsColumns();

        return this;
    }

    private (Type configurationType, Type entityType)[] CollectConfigurations()
    {
        var configurationType = typeof(IEntityConfiguration<>);

        var allTypes = TypeManager.GetInstance(_configurationsAssembly).Types;
        var concreteClasses = allTypes.Where(x => x.IsClass && !x.IsAbstract && !x.IsGenericType).ToArray();

        var configurationTypes = concreteClasses
            .Select(x => (
                x,
                i: x.GetInterfaces().SingleOrDefault(y =>
                    y.IsGenericType && y.GetGenericTypeDefinition() == configurationType
                )
            ))
            .Where(p => p.i != null)
            .Select(p => (p.x, p.i!.GenericTypeArguments.Single()))
            .ToArray();

        return configurationTypes;
    }
}
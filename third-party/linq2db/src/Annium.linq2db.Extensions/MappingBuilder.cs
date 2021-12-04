using System;
using System.Linq;
using System.Reflection;
using Annium.Core.Runtime.Types;
using Annium.linq2db.Extensions.Models;
using LinqToDB.Mapping;

namespace Annium.linq2db.Extensions;

public class MappingBuilder
{
    public FluentMappingBuilder Map { get; }
    private readonly MetadataBuilder _metadataBuilder;
    private readonly Assembly _configurationsAssembly;
    private readonly Lazy<(Type configurationType, Type entityType)[]> _configurations;

    public MappingBuilder(
        Assembly configurationsAssembly,
        MappingSchema schema
    )
    {
        Map = schema.GetFluentMappingBuilder();
        _metadataBuilder = new MetadataBuilder();
        _configurationsAssembly = configurationsAssembly;
        _configurations = new Lazy<(Type configurationType, Type entityType)[]>(CollectConfigurations);
    }

    public MappingBuilder ApplyConfigurations()
    {
        var entityMappingBuilderFactory = typeof(FluentMappingBuilder).GetMethod(nameof(FluentMappingBuilder.Entity))!;

        foreach (var (configurationType, entityType) in _configurations.Value)
        {
            var entityMappingBuilder = entityMappingBuilderFactory.MakeGenericMethod(entityType)
                .Invoke(Map, new object?[] {null})!;
            var configureMethod = typeof(IEntityConfiguration<>).MakeGenericType(entityType)
                .GetMethod(nameof(IEntityConfiguration<object>.Configure))!;
            var configuration = Activator.CreateInstance(configurationType)!;
            configureMethod.Invoke(configuration, new[] {entityMappingBuilder});
        }

        this.IncludeAssociationKeysAsColumns();

        return this;
    }

    public Database GetMetadata(MetadataBuilderFlags flags = MetadataBuilderFlags.None) =>
        _metadataBuilder.Build(Map.MappingSchema, flags);

    public MappingBuilder Configure(Action<Database> configure, MetadataBuilderFlags flags = MetadataBuilderFlags.None)
    {
        configure(GetMetadata(flags));

        return this;
    }

    private (Type configurationType, Type entityType)[] CollectConfigurations()
    {
        var configurationType = typeof(IEntityConfiguration<>);

        var allTypes = TypeManager.GetInstance(_configurationsAssembly, false).Types;
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
using System;
using System.Linq;
using System.Reflection;
using Annium.Core.Runtime.Types;
using Annium.Extensions.Primitives;
using Annium.linq2db.Extensions.Models;
using LinqToDB.Mapping;

namespace Annium.linq2db.Extensions
{
    public class MappingBuilder
    {
        private readonly MetadataBuilder _metadataBuilder;
        private readonly Assembly _configurationsAssembly;
        private readonly FluentMappingBuilder _mappingBuilder;
        private readonly Lazy<(Type configurationType, Type entityType)[]> _configurations;

        public MappingBuilder(
            Assembly configurationsAssembly,
            FluentMappingBuilder mappingBuilder
        )
        {
            _metadataBuilder = new MetadataBuilder();
            _mappingBuilder = mappingBuilder;
            _configurationsAssembly = configurationsAssembly;
            _configurations = new Lazy<(Type configurationType, Type entityType)[]>(CollectConfigurations);
        }

        public MappingBuilder ApplyConfigurations()
        {
            var entityMappingBuilderFactory = typeof(FluentMappingBuilder).GetMethod(nameof(FluentMappingBuilder.Entity))!;

            foreach (var (configurationType, entityType) in _configurations.Value)
            {
                var entityMappingBuilder = entityMappingBuilderFactory.MakeGenericMethod(entityType)
                    .Invoke(_mappingBuilder, new object?[] { null })!;
                var configureMethod = typeof(IEntityConfiguration<>).MakeGenericType(entityType)
                    .GetMethod(nameof(IEntityConfiguration<object>.Configure))!;
                var configuration = Activator.CreateInstance(configurationType)!;
                configureMethod.Invoke(configuration, new[] { entityMappingBuilder });
            }

            return this;
        }

        public Database GetMetadata() => _metadataBuilder.Build(_mappingBuilder.MappingSchema);

        public MappingBuilder SnakeCaseColumns() => Configure(db =>
        {
            foreach (var table in db.Tables)
            foreach (var column in table.Columns)
                column.Column.Name = column.Member.Name.SnakeCase();
        });

        public MappingBuilder Configure(Action<Database> configure)
        {
            configure(GetMetadata());

            return this;
        }

        private (Type configurationType, Type entityType)[] CollectConfigurations() => TypeManager.GetInstance(_configurationsAssembly).Types
            .Where(x => x.IsClass && !x.IsAbstract && !x.IsGenericType)
            .Select(x => (x, i: x.GetInterfaces().SingleOrDefault(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IEntityConfiguration<>))))
            .Where(p => p.i != null)
            .Select(p => (p.x, p.i.GenericTypeArguments.Single()))
            .ToArray();
    }
}
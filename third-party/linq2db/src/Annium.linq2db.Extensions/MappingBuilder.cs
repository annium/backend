using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Annium.Core.Runtime.Types;
using Annium.Extensions.Primitives;
using LinqToDB.Mapping;

namespace Annium.linq2db.Extensions
{
    public class MappingBuilder
    {
        private readonly Assembly _configurationsAssembly;
        private readonly FluentMappingBuilder _mappingBuilder;
        private readonly Lazy<(Type configurationType, Type entityType)[]> _configurations;

        public MappingBuilder(
            Assembly configurationsAssembly,
            FluentMappingBuilder mappingBuilder
        )
        {
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

        public MappingBuilder SnakeCaseColumns()
        {
            var entityTypes = _configurations.Value.Select(x => x.entityType).ToArray();

            var entityMappingBuilderFactory = typeof(FluentMappingBuilder).GetMethod(nameof(FluentMappingBuilder.Entity))!;

            foreach (var entityType in entityTypes)
            {
                var entityMappingBuilder = entityMappingBuilderFactory.MakeGenericMethod(entityType)
                    .Invoke(_mappingBuilder, new object?[] { null })!;
                var getPropertyBuilder = typeof(EntityMappingBuilder<>).MakeGenericType(entityType)
                    .GetMethod(nameof(EntityMappingBuilder<object>.Property))!;
                var hasColumnName = typeof(PropertyMappingBuilder<>).MakeGenericType(entityType)
                    .GetMethod(nameof(PropertyMappingBuilder<object>.HasColumnName))!;
                var entityParameter = Expression.Parameter(entityType);

                foreach (var property in entityType.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
                {
                    var propertyBuilder = getPropertyBuilder
                        .Invoke(
                            entityMappingBuilder,
                            new[]
                            {
                                Expression.Lambda(
                                    Expression.Convert(Expression.Property(entityParameter, property), typeof(object)),
                                    entityParameter
                                )
                            }
                        )!;
                    hasColumnName.Invoke(propertyBuilder, new[] { property.Name.SnakeCase() });
                }
            }

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
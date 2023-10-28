using System;
using System.Linq.Expressions;
using System.Net.Mime;
using Annium.Core.DependencyInjection;
using Annium.Serialization.Abstractions;
using LinqToDB;
using LinqToDB.Mapping;

// ReSharper disable once CheckNamespace
namespace Annium.linq2db.Extensions;

public static class MappingSchemaExtensions
{
    public static MappingSchema UseSnakeCaseColumns(this MappingSchema schema) =>
        schema.Configure(
            db =>
            {
                foreach (var table in db.Tables.Values)
                    foreach (var column in table.Columns.Values)
                        column.Attribute.Name = column.Member.Name.SnakeCase();
            },
            MetadataFlags.IncludeMembersNotMarkedAsColumns
        );

    public static MappingSchema UseJsonSupport(this MappingSchema schema, IServiceProvider sp)
    {
        var serializers = sp.Resolve<IIndex<SerializerKey, ISerializer<string>>>();
        var serializer = serializers[SerializerKey.CreateDefault(MediaTypeNames.Application.Json)];
        var serialize = typeof(ISerializer<string>).GetMethod(
            nameof(ISerializer<string>.Serialize),
            new[] { typeof(Type), typeof(object) }
        )!;
        var deserialize = typeof(ISerializer<string>).GetMethod(
            nameof(ISerializer<string>.Deserialize),
            new[] { typeof(Type), typeof(string) }
        )!;

        return schema.Configure(
            db =>
            {
                var entityMappingBuilderFactory = typeof(FluentMappingBuilder).GetMethod(
                    nameof(FluentMappingBuilder.Entity)
                )!;
                var mappingBuilder = new FluentMappingBuilder(schema);

                foreach (var table in db.Tables.Values)
                {
                    var entityMappingBuilder = entityMappingBuilderFactory
                        .MakeGenericMethod(table.Type)
                        .Invoke(mappingBuilder, new object?[] { null })!;
                    var getPropertyMappingBuilder = entityMappingBuilder
                        .GetType()
                        .GetMethod(nameof(EntityMappingBuilder<object>.Property))!;

                    foreach (var column in table.Columns.Values)
                    {
                        if (column.Attribute.DataType is not (DataType.Json or DataType.BinaryJson))
                            continue;

                        var typeParameter = Expression.Parameter(table.Type);
                        var propertyMappingBuilder = getPropertyMappingBuilder
                            .MakeGenericMethod(column.Type)
                            .Invoke(
                                entityMappingBuilder,
                                new object[]
                                {
                                    Expression.Lambda(
                                        Expression.PropertyOrField(typeParameter, column.Member.Name),
                                        typeParameter
                                    )
                                }
                            )!;
                        var hasConversionFunc = propertyMappingBuilder
                            .GetType()
                            .GetMethod(nameof(PropertyMappingBuilder<object, object>.HasConversionFunc))!
                            .MakeGenericMethod(typeof(string));
                        var instance = Expression.Constant(serializer);
                        var serializeValue = Expression.Parameter(column.Type);
                        var serializeFn = Expression
                            .Lambda(
                                Expression.Call(
                                    instance,
                                    serialize,
                                    new Expression[] { Expression.Constant(column.Type), serializeValue }
                                ),
                                serializeValue
                            )
                            .Compile();
                        var deserializeType = Expression.Constant(column.Type);
                        var deserializeValue = Expression.Parameter(typeof(string));
                        var deserializeFn = Expression
                            .Lambda(
                                Expression.Convert(
                                    Expression.Call(instance, deserialize, deserializeType, deserializeValue),
                                    column.Type
                                ),
                                deserializeValue
                            )
                            .Compile();
                        hasConversionFunc.Invoke(
                            propertyMappingBuilder,
                            new object[] { serializeFn, deserializeFn, false }
                        );
                    }
                }

                mappingBuilder.Build();
            },
            MetadataFlags.IncludeMembersNotMarkedAsColumns
        );
    }
}

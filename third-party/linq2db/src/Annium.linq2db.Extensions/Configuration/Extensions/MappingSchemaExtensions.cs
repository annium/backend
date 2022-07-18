using System;
using System.Linq.Expressions;
using System.Net.Mime;
using Annium.Core.DependencyInjection;
using Annium.Core.Primitives;
using Annium.Serialization.Abstractions;
using LinqToDB;
using LinqToDB.Mapping;

namespace Annium.linq2db.Extensions.Configuration.Extensions;

public static class MappingSchemaExtensions
{
    public static MappingSchema UseSnakeCaseColumns(this MappingSchema schema) => schema.Configure(db =>
    {
        foreach (var table in db.Tables)
        foreach (var column in table.Columns)
            column.Attribute.Name = column.Member.Name.SnakeCase();
    }, MetadataFlags.IncludeMembersNotMarkedAsColumns);

    public static MappingSchema UseJsonSupport(this MappingSchema schema, IServiceProvider sp) => schema.Configure(db =>
    {
        var serializers = sp.Resolve<IIndex<SerializerKey, ISerializer<string>>>();
        var serializer = Expression.Constant(serializers[SerializerKey.CreateDefault(MediaTypeNames.Application.Json)]);
        var serialize = typeof(ISerializer<string>).GetMethod(nameof(ISerializer<string>.Serialize), new[] { typeof(object) })!;
        var deserialize = typeof(ISerializer<string>).GetMethod(nameof(ISerializer<string>.Deserialize), new[] { typeof(Type), typeof(string) })!;

        foreach (var table in db.Tables)
        foreach (var column in table.Columns)
        {
            if (column.Attribute.DataType is not DataType.BinaryJson)
                continue;

            var serializeValue = Expression.Parameter(column.Type);
            var serializeFn = Expression.Lambda(
                Expression.Call(serializer, serialize, serializeValue),
                serializeValue
            );
            schema.SetConvertExpression(column.Type, typeof(string), serializeFn);

            var deserializeType = Expression.Constant(column.Type);
            var deserializeValue = Expression.Parameter(typeof(string));
            var deserializeFn = Expression.Lambda(
                Expression.Convert(
                    Expression.Call(serializer, deserialize, deserializeType, deserializeValue),
                    column.Type
                ),
                deserializeValue
            );
            schema.SetConvertExpression(typeof(string), column.Type, deserializeFn);
        }
    }, MetadataFlags.IncludeMembersNotMarkedAsColumns);
}
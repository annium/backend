using System;
using System.Linq.Expressions;
using System.Net.Mime;
using Annium.Core.DependencyInjection;
using Annium.Core.Primitives;
using Annium.Serialization.Abstractions;
using LinqToDB;

namespace Annium.linq2db.Extensions.Configuration.Extensions;

public static class MappingBuilderExtensions
{
    public static IMappingBuilder UseSnakeCaseColumns(this IMappingBuilder builder) => builder.Configure(db =>
    {
        foreach (var table in db.Tables)
        foreach (var column in table.Columns)
            column.Attribute.Name = column.Member.Name.SnakeCase();
    }, MetadataBuilderFlags.IncludeMembersNotMarkedAsColumns);

    public static IMappingBuilder UseJsonSupport(this IMappingBuilder builder, IServiceProvider sp) => builder.Configure(db =>
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
            builder.Schema.SetConvertExpression(column.Type, typeof(string), serializeFn);

            var deserializeType = Expression.Constant(column.Type);
            var deserializeValue = Expression.Parameter(typeof(string));
            var deserializeFn = Expression.Lambda(
                Expression.Convert(
                    Expression.Call(serializer, deserialize, deserializeType, deserializeValue),
                    column.Type
                ),
                deserializeValue
            );
            builder.Schema.SetConvertExpression(typeof(string), column.Type, deserializeFn);
        }
    }, MetadataBuilderFlags.IncludeMembersNotMarkedAsColumns);
}
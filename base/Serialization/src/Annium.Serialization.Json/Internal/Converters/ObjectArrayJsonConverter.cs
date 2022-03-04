using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Annium.Serialization.Json.Internal.Converters;

internal class ObjectArrayJsonConverter<T> : JsonConverter<T>
{
    private static readonly IReadOnlyCollection<MemberInfo> Members;

    static ObjectArrayJsonConverter()
    {
        var members = typeof(T).GetMembers()
            .Where(x => x switch
            {
                PropertyInfo p => p.CanRead && p.CanWrite,
                FieldInfo f    => !f.IsInitOnly,
                _              => false
            })
            .Select(x => (member: x, order: x.GetCustomAttribute<JsonPropertyOrderAttribute>()))
            .ToArray();

        Members = members.All(x => x.order is null)
            ? members
                .OrderBy(x => x.member.Name)
                .Select(x => x.member)
                .ToArray()
            : members
                .Where(x => x.order is not null)
                .OrderBy(x => x.order!.Order)
                .Select(x => x.member)
                .ToArray();
    }

    public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartArray)
            throw new JsonException("Expected array start");

        var depth = reader.CurrentDepth;
        var value = Activator.CreateInstance(typeof(T))!;

        foreach (var member in Members)
        {
            reader.Read();
            if (member is PropertyInfo p)
                p.SetValue(value, JsonSerializer.Deserialize(ref reader, p.PropertyType, options));
            if (member is FieldInfo f)
                f.SetValue(value, JsonSerializer.Deserialize(ref reader, f.FieldType, options));
        }

        reader.Read();

        if (reader.CurrentDepth != depth)
            throw new JsonException($"Final depth {reader.CurrentDepth} != initial depth {depth}");

        if (reader.TokenType != JsonTokenType.EndArray)
            throw new JsonException("Expected array end");

        return (T)value;
    }

    public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
    {
        writer.WriteStartArray();

        foreach (var member in Members)
        {
            if (member is PropertyInfo p)
                JsonSerializer.Serialize(writer, p.GetValue(value), p.PropertyType, options);
            if (member is FieldInfo f)
                JsonSerializer.Serialize(writer, f.GetValue(value), f.FieldType, options);
        }

        writer.WriteEndArray();
    }
}
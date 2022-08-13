using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using Annium.Core.Primitives;
using Annium.Core.Reflection;
using Annium.Serialization.Abstractions;
using Annium.Serialization.Abstractions.Attributes;

namespace Annium.Serialization.Json.Internal.Converters;

internal class ConstructorJsonConverterFactory : JsonConverterFactory
{
    private readonly ConcurrentDictionary<Type, ConstructorJsonConverterConfiguration?> _configurations = new();

    public override bool CanConvert(Type type) => _configurations.GetOrAdd(type, GetConfiguration) is not null;

    public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        var configuration = _configurations[typeToConvert] ?? throw new ArgumentException($"Type {typeToConvert.FriendlyName()} configuration is missing");

        return (JsonConverter) Activator.CreateInstance(
            typeof(ConstructorJsonConverter<>).MakeGenericType(typeToConvert),
            configuration.Constructor,
            configuration.Parameters,
            configuration.Properties
        )!;
    }

    private ConstructorJsonConverterConfiguration? GetConfiguration(Type type)
    {
        // must be class or struct
        if (!type.IsClass && !type.IsValueType)
            return null;

        // must be not abstract and constructable
        if (type.IsAbstract || type.IsGenericType && !type.IsConstructedGenericType)
            return null;

        // must be object-like
        if (typeof(IEnumerable).IsAssignableFrom(type))
            return null;

        // must not be Tuple
        if (typeof(ITuple).IsAssignableFrom(type))
            return null;

        // must not be nullable struct
        if (type.IsNullableValueType())
            return null;

        // select non-default constructors
        var constructors = type
            .GetConstructors(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
            .Where(x =>
            {
                var constructorParams = x.GetParameters();

                // default constructor is not applicable
                if (constructorParams.Length == 0)
                    return false;

                // clone constructor is not applicable
                if (constructorParams.Length == 1 && constructorParams[0].ParameterType == type)
                    return false;

                return true;
            })
            .ToArray();

        // if many non-default constructors - try find single constructor with DeserializationConstructorAttribute
        if (constructors.Length != 1)
            constructors = constructors
                .Where(x => x.GetCustomAttribute<DeserializationConstructorAttribute>() != null)
                .ToArray();

        // if ambiguous match - won't convert
        if (constructors.Length != 1)
            return null;

        var constructor = constructors[0];
        var parameters = constructor.GetParameters()
            .Select(x =>
            {
                var property = type.GetProperty(x.Name!.PascalCase());
                var name = property?.GetCustomAttribute<JsonPropertyNameAttribute>()?.Name ?? x.Name ?? string.Empty;

                return new ConstructorJsonConverterConfiguration.ParameterItem(x.ParameterType, name);
            })
            .ToList();
        var properties = type.GetAllProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
            .Where(x => x.CanWrite)
            .ToArray();

        return new ConstructorJsonConverterConfiguration(constructor, parameters, properties);
    }
}
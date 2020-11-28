using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using Annium.Core.Reflection;
using Annium.Core.Primitives;
using Annium.Serialization.Abstractions;

namespace Annium.Serialization.Json.Internal.Converters
{
    internal class ConstructorJsonConverterFactory : JsonConverterFactory
    {
        private readonly IDictionary<Type, ConstructorJsonConverterConfiguration> _configurations =
            new Dictionary<Type, ConstructorJsonConverterConfiguration>();

        public override bool CanConvert(Type type)
        {
            if (_configurations.ContainsKey(type))
                return true;

            // must be class or struct
            if (!type.IsClass && !type.IsValueType)
                return false;

            // must be not abstract and constructable
            if (type.IsAbstract || type.IsGenericType && !type.IsConstructedGenericType)
                return false;

            // must be object-like
            if (typeof(IEnumerable).IsAssignableFrom(type))
                return false;

            // must not be nullable struct
            if (type.IsNullableValueType())
                return false;

            // select non-default constructors
            var constructors = type
                .GetConstructors(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .Where(x =>
                {
                    var parameters = x.GetParameters();

                    // default constructor is not applicable
                    if (parameters.Length == 0)
                        return false;

                    // clone constructor is not applicable
                    if (parameters.Length == 1 && parameters[0].ParameterType == type)
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
                return false;

            _configurations[type] = ParseConfiguration(constructors[0]);

            return true;
        }

        public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
        {
            var configuration = _configurations[typeToConvert];

            return (JsonConverter) Activator.CreateInstance(
                typeof(ConstructorJsonConverter<>).MakeGenericType(typeToConvert),
                configuration
            )!;
        }

        private ConstructorJsonConverterConfiguration ParseConfiguration(ConstructorInfo constructor)
        {
            var type = constructor.DeclaringType!;
            var parameters = constructor.GetParameters()
                .Select(x =>
                {
                    var property = type.GetProperty(x.Name!.PascalCase());
                    var name = property?.GetCustomAttribute<JsonPropertyNameAttribute>()?.Name ?? x.Name ?? string.Empty;

                    return new ConstructorJsonConverterConfiguration.ParameterItem(name, x.ParameterType);
                })
                .ToList();

            return new ConstructorJsonConverterConfiguration(constructor, parameters);
        }
    }
}
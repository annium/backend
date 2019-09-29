using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Annium.Core.Mapper;
using Annium.Extensions.Primitives;

namespace Annium.Extensions.Arguments
{
    internal class ConfigurationBuilder : IConfigurationBuilder
    {
        private readonly IArgumentProcessor argumentProcessor;

        private readonly IConfigurationProcessor configurationProcessor;

        public ConfigurationBuilder(
            IArgumentProcessor argumentProcessor,
            IConfigurationProcessor configurationProcessor
        )
        {
            this.argumentProcessor = argumentProcessor;
            this.configurationProcessor = configurationProcessor;
        }

        public T Build<T>(string[] args)
        where T : new()
        {
            var raw = argumentProcessor.Compose(args);

            var value = new T();

            SetPositions(value, raw.Positions.ToArray());
            SetFlags(value, raw.Flags.ToArray());
            SetOptions(value, raw.Options, raw.MultiOptions);
            SetRaw(value, raw.Raw);

            return value;
        }

        private void SetPositions<T>(T value, string[] positions)
        {
            var properties = configurationProcessor.GetPropertiesWithAttribute<PositionAttribute>(typeof(T))
                .OrderBy(e => e.attribute.Position);

            var i = 1;
            foreach (var(property, attribute) in properties)
            {
                if (attribute.Position != i)
                    throw new Exception($"Position argument expected to have position '{i}', but got position '{attribute.Position}");

                if (i > positions.Length)
                    if (attribute.IsRequired)
                        throw new Exception($"Required position argument '{property.Name}' has no value");
                    else
                        break;

                property.SetValue(value, GetValue(property, property.PropertyType, positions[i - 1]));
                i++;
            }
        }

        private void SetFlags<T>(T value, string[] flags)
        {
            var properties = configurationProcessor.GetPropertiesWithAttribute<OptionAttribute>(typeof(T))
                .Where(e => e.property.PropertyType == typeof(bool))
                .ToArray();

            foreach (var(property, attribute) in properties)
            {
                var key = FindOptionName(flags, property.Name, attribute.Alias);
                property.SetValue(value, key != null);
            }
        }

        private void SetOptions<T>(
            T value,
            IReadOnlyDictionary<string, string> plainOptions,
            IReadOnlyDictionary<string, IEnumerable<string>> multiOptions
        )
        {
            var properties = configurationProcessor.GetPropertiesWithAttribute<OptionAttribute>(typeof(T));
            var plainProperties = properties
                .Where(e => e.property.PropertyType != typeof(bool) && !e.property.PropertyType.IsArray)
                .ToArray();

            // for base properties - set values from options
            foreach (var(property, attribute) in plainProperties)
            {
                var key = FindOptionName(plainOptions.Keys, property.Name, attribute.Alias);
                if (key == null)
                    if (attribute.IsRequired)
                        throw new Exception($"Required option argument '{property.Name}' has no value");
                    else
                        continue;

                property.SetValue(value, GetValue(property, property.PropertyType, plainOptions[key]));
            }

            var arrayProperties = configurationProcessor.GetPropertiesWithAttribute<OptionAttribute>(typeof(T))
                .Where(e => e.property.PropertyType.IsArray)
                .ToArray();

            // for array properties - set values from multioptions with fallback to options
            foreach (var(property, attribute) in arrayProperties)
            {
                string? key = null;
                string[] raw = Array.Empty<string>();
                if ((key = FindOptionName(multiOptions.Keys, property.Name, attribute.Alias)) != null)
                    raw = multiOptions[key].ToArray();
                else if ((key = FindOptionName(plainOptions.Keys, property.Name, attribute.Alias)) != null)
                    raw = new [] { plainOptions[key] };
                else if (attribute.IsRequired)
                    throw new Exception($"Required multi option argument '{property.Name}' has no value");
                else
                    continue;

                var type = property.PropertyType.GetElementType() !;
                var array = (IList) Array.CreateInstance(type, raw.Count());
                for (var i = 0; i < array.Count; i++)
                    array[i] = GetValue(property, type, raw[i]);

                property.SetValue(value, array);
            }
        }

        private void SetRaw<T>(T value, string raw)
        {
            var(property, _) = configurationProcessor.GetPropertiesWithAttribute<RawAttribute>(typeof(T))
                .FirstOrDefault();

            if (property != null)
                property.SetValue(value, GetValue(property, property.PropertyType, raw));
        }

        private string? FindOptionName(IEnumerable<string> names, string name, string? alias)
        {
            if (alias == null)
                return names.Contains(name) ? name : null;

            alias = alias.PascalCase();

            if (names.Contains(alias))
                return alias;

            if (names.Contains(name))
                return name;

            return null;
        }

        private object GetValue(PropertyInfo property, Type type, string value)
        {
            var values = property.GetCustomAttribute<ValuesAttribute>()?.Values;
            if (values != null && !values.Contains(value))
                throw new Exception($"Given value '{value}' isn't in allowed values: {string.Join(", ",values)}");

            return Mapper.Map(value, type);
        }
    }
}
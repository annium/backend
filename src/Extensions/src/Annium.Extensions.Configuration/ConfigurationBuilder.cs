using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Annium.Extensions.Conversion;

namespace Annium.Extensions.Configuration
{
    public class ConfigurationBuilder : IConfigurationBuilder
    {
        internal const string Separator = "<$>";

        private bool isAlreadyBuilt = false;

        private IDictionary<string, string> config = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        private Stack<string> context = new Stack<string>();

        private string path => string.Join(Separator, context.Reverse());

        public IConfigurationBuilder Add(IReadOnlyDictionary<string, string> config)
        {
            foreach (var (key, value) in config)
                this.config[key] = value;

            return this;
        }

        public T Build<T>()
        where T : class, new()
        {
            if (isAlreadyBuilt)
                throw new InvalidOperationException($"Configuration is already built");
            isAlreadyBuilt = true;

            return (T) Process(typeof(T));
        }

        private object Process(Type type)
        {
            if (type.IsGenericType &&
                type.GetGenericTypeDefinition() == typeof(Dictionary<,>))
                return ProcessDictionary(type);
            if (type.IsGenericType &&
                type.GetGenericTypeDefinition() == typeof(List<>))
                return ProcessList(type);
            if (type.IsArray)
                return ProcessArray(type);
            if (Converter.CanConvert(typeof(string), type))
                return ProcessValue(type);
            return ProcessObject(type);
        }

        private object ProcessDictionary(Type type)
        {
            var keyType = type.GetGenericArguments() [0];
            var valueType = type.GetGenericArguments() [1];
            var path = this.path;
            var items = config.Where(e => e.Key.StartsWith(path, StringComparison.OrdinalIgnoreCase) && e.Key.Length > path.Length).ToArray();
            var result = (IDictionary) Activator.CreateInstance(type);

            foreach (var (key, value) in items)
            {
                var name = key.Substring(path.Length + Separator.Length).Split(Separator) [0];
                context.Push(name);
                result[Converter.Convert(name, keyType)] = Process(valueType);
                context.Pop();
            }

            return result;
        }

        private object ProcessList(Type type)
        {
            var elementType = type.GetGenericArguments() [0];
            var result = (IList) Activator.CreateInstance(type);

            var process = true;
            for (var index = 0; process; index++)
            {
                context.Push(index.ToString());

                var path = this.path;
                if (config.Keys.Any(e => e.StartsWith(path, StringComparison.OrdinalIgnoreCase)))
                    result.Add(Process(elementType));
                else
                    process = false;

                context.Pop();
            }

            return result;
        }

        private object ProcessArray(Type type)
        {
            var raw = (IList) ProcessList(type);

            var elementType = type.GetElementType();
            var result = (IList) Array.CreateInstance(elementType, raw.Count);

            for (var index = 0; index < raw.Count; index++)
                result[index] = raw[index];

            return result;
        }

        private object ProcessObject(Type type)
        {
            var result = Activator.CreateInstance(type);

            foreach (var property in type.GetProperties().Where(e => e.CanWrite))
            {
                context.Push(property.Name);

                var path = this.path;
                if (config.Keys.Any(e => e.StartsWith(path, StringComparison.OrdinalIgnoreCase)))
                    property.SetValue(result, Process(property.PropertyType));

                context.Pop();
            }

            return result;
        }

        private object ProcessValue(Type type)
        {
            return Converter.Convert(config[path], type);
        }
    }
}
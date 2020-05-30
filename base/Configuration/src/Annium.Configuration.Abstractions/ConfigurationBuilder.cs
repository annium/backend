using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Annium.Core.Mapper;
using Annium.Core.Runtime.Types;
using Annium.Extensions.Primitives;

namespace Annium.Configuration.Abstractions
{
    public class ConfigurationBuilder : IConfigurationBuilder
    {
        private readonly IDictionary<string[], string> config = new Dictionary<string[], string>(new KeyComparer());

        private readonly Stack<string> context = new Stack<string>();

        protected string[] Path => context.Reverse().ToArray();

        public IConfigurationBuilder Add(IReadOnlyDictionary<string[], string> config)
        {
            foreach (var (key, value) in config)
                this.config[key] = value;

            return this;
        }

        public T Build<T>() where T : class, new()
        {
            return (T) Process(typeof(T));
        }

        private object Process(Type type)
        {
            if (Mapper.HasMap(string.Empty, type))
                return ProcessValue(type);
            if (type.IsGenericType &&
                type.GetGenericTypeDefinition() == typeof(Dictionary<,>))
                return ProcessDictionary(type);
            if (type.IsGenericType &&
                type.GetGenericTypeDefinition() == typeof(List<>))
                return ProcessList(type);
            if (type.IsArray)
                return ProcessArray(type);
            return ProcessObject(type);
        }

        private object ProcessDictionary(Type type)
        {
            var keyType = type.GetGenericArguments()[0];
            var valueType = type.GetGenericArguments()[1];

            // var items = config.Where(e => e.Key.StartsWith(path, StringComparison.OrdinalIgnoreCase) && e.Key.Length > path.Length).ToArray();
            var items = GetDescendants();
            var result = (IDictionary) Activator.CreateInstance(type)!;

            foreach (var name in items)
            {
                // var name = key.Substring(path.Length + separator.Length).Split(separator) [0];
                context.Push(name);
                result[Mapper.Map(name, keyType)] = Process(valueType);
                context.Pop();
            }

            return result;
        }

        private object ProcessList(Type type)
        {
            var elementType = type.GetGenericArguments()[0];
            var result = (IList) Activator.CreateInstance(type)!;

            var items = GetDescendants();

            foreach (var index in items)
            {
                context.Push(index);
                result.Add(Process(elementType));
                context.Pop();
            }

            return result;
        }

        private object ProcessArray(Type type)
        {
            var elementType = type.GetElementType()!;
            var raw = (IList) ProcessList(typeof(List<>).MakeGenericType(elementType));

            var result = (IList) Array.CreateInstance(elementType, raw.Count);

            for (var index = 0; index < raw.Count; index++)
                result[index] = raw[index];

            return result;
        }

        private object ProcessObject(Type type)
        {
            if (type.IsAbstract || type.IsInterface)
            {
                var resolutionKeyProperty = TypeManager.Instance.GetResolutionKeyProperty(type);
                if (resolutionKeyProperty is null)
                    throw new ArgumentException($"Can't resolve abstract type {type}");

                context.Push(resolutionKeyProperty.Name);
                var hasKey = config.TryGetValue(Path, out var key);
                context.Pop();
                if (!hasKey)
                    return null!;

                var resolution = TypeManager.Instance.ResolveByKey(key!, type);
                if (resolution is null)
                    throw new ArgumentException($"Can't resolve abstract type {type} with key {key}");
                type = resolution;
            }

            var result = Activator.CreateInstance(type)!;
            var properties = type.GetProperties().Where(e => e.CanWrite).ToArray();
            foreach (var property in properties)
            {
                context.Push(property.Name);
                if (KeyExists())
                    property.SetValue(result, Process(property.PropertyType));
                context.Pop();
            }

            return result;
        }

        private object ProcessValue(Type type)
        {
            if (config.TryGetValue(Path, out var value))
                return Mapper.Map(value, type);

            throw new ArgumentException($"Key {string.Join('.', Path)} not found in configuration.");
        }

        private string[] GetDescendants()
        {
            var path = Normalize(Path);
            if (path.Length == 0)
                return config.Keys.Select(k => k.First()).Distinct().ToArray();

            return config.Keys
                .Where(k => k.Length > path.Length)
                .Where(k => Normalize(k.Take(path.Length)).SequenceEqual(path))
                .Select(k => k.Skip(path.Length).First())
                .Distinct()
                .ToArray();
        }

        private bool KeyExists()
        {
            var path = Normalize(Path);
            if (path.Length == 0)
                return config.Keys.Count() > 0;

            return config.Keys
                .Where(k => k.Length >= path.Length)
                .Select(k => k.Take(path.Length))
                .Where(k => Normalize(k).SequenceEqual(path))
                .Count() > 0;
        }

        private string[] Normalize(IEnumerable<string> seq) => seq.Select(e => e.CamelCase()).ToArray();

        private class KeyComparer : IEqualityComparer<string[]>
        {
            public bool Equals(string[] x, string[] y)
            {
                // if same reference or both null, then equality is true
                if (ReferenceEquals(x, y))
                    return true;

                // if any is null, or length doesn't match - false
                if (x == null || y == null || x.Length != y.Length)
                    return false;

                // check, that all elements are equal case independently
                for (int i = 0; i < x.Length; i++)
                    if (x[i].CamelCase() != y[i].CamelCase())
                        return false;

                // if no mismatches, equal
                return true;
            }

            public int GetHashCode(string[] obj)
            {
                if (obj == null)
                    return 0;

                unchecked
                {
                    int hash = 17;

                    // get hash code for all items in array
                    foreach (var item in obj)
                    {
                        hash = hash * 23 + (item == null ? 0 : item.CamelCase().GetHashCode());
                    }

                    return hash;
                }
            }
        }
    }
}
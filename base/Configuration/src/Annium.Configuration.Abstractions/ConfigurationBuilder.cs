using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Annium.Core.Mapper;
using Annium.Core.Runtime.Types;
using Annium.Extensions.Primitives;

namespace Annium.Configuration.Abstractions
{
    internal class ConfigurationBuilder : IConfigurationBuilder
    {
        private readonly ITypeManager _typeManager;
        private readonly IMapper _mapper;
        private string[] Path => _context.Reverse().ToArray();
        private readonly IDictionary<string[], string> _config = new Dictionary<string[], string>(new KeyComparer());
        private readonly Stack<string> _context = new Stack<string>();

        public ConfigurationBuilder(
            ITypeManager typeManager,
            IMapper mapper
        )
        {
            _typeManager = typeManager;
            _mapper = mapper;
        }

        public IConfigurationBuilder Add(IReadOnlyDictionary<string[], string> config)
        {
            foreach (var (key, value) in config)
                _config[key] = value;

            return this;
        }

        public T Build<T>() where T : class, new()
        {
            return (T) Process(typeof(T));
        }

        private object Process(Type type)
        {
            if (_mapper.HasMap(string.Empty, type))
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
                _context.Push(name);
                result[_mapper.Map(name, keyType)] = Process(valueType);
                _context.Pop();
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
                _context.Push(index);
                result.Add(Process(elementType));
                _context.Pop();
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
                var resolutionKeyProperty = _typeManager.GetResolutionKeyProperty(type);
                if (resolutionKeyProperty is null)
                    throw new ArgumentException($"Can't resolve abstract type {type}");

                _context.Push(resolutionKeyProperty.Name);
                var hasKey = _config.TryGetValue(Path, out var key);
                _context.Pop();
                if (!hasKey)
                    return null!;

                var resolution = _typeManager.ResolveByKey(key!, type);
                if (resolution is null)
                    throw new ArgumentException($"Can't resolve abstract type {type} with key {key}");
                type = resolution;
            }

            var result = Activator.CreateInstance(type)!;
            var properties = type.GetProperties().Where(e => e.CanWrite).ToArray();
            foreach (var property in properties)
            {
                _context.Push(property.Name);
                if (KeyExists())
                    property.SetValue(result, Process(property.PropertyType));
                _context.Pop();
            }

            return result;
        }

        private object ProcessValue(Type type)
        {
            if (_config.TryGetValue(Path, out var value))
                return _mapper.Map(value, type);

            throw new ArgumentException($"Key {string.Join('.', Path)} not found in configuration.");
        }

        private string[] GetDescendants()
        {
            var path = Normalize(Path);
            if (path.Length == 0)
                return _config.Keys.Select(k => k.First()).Distinct().ToArray();

            return _config.Keys
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
                return _config.Keys.Count > 0;

            return _config.Keys
                .Where(k => k.Length >= path.Length)
                .Select(k => k.Take(path.Length))
                .Any(k => Normalize(k).SequenceEqual(path));
        }

        private string[] Normalize(IEnumerable<string> seq) => seq.Select(e => e.CamelCase()).ToArray();

        private class KeyComparer : IEqualityComparer<string[]>
        {
            public bool Equals(string[]? x, string[]? y)
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
                unchecked
                {
                    int hash = 17;

                    // get hash code for all items in array
                    foreach (var item in obj)
                    {
                        hash = hash * 23 + item.CamelCase().GetHashCode();
                    }

                    return hash;
                }
            }
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;

namespace Annium.Extensions.Mapper
{
    internal class TypeResolver
    {
        public static TypeResolver Instance = new TypeResolver();

        private readonly Lazy<IDictionary<Type, Type[]>> types;

        private readonly Lazy<IDictionary<Type, string[]>> signatures;

        public TypeResolver()
        {
            types = new Lazy<IDictionary<Type, Type[]>>(CollectTypes, true);
            signatures = new Lazy<IDictionary<Type, string[]>>(() => CollectSignatures(types.Value));
        }

        public Type Resolve(object instance, Type baseType)
        {
            var properties = instance.GetType().GetProperties().Select(p => p.Name.ToLowerInvariant()).OrderBy(p => p).ToArray();

            if (!types.Value.TryGetValue(baseType, out var descendants))
                throw new MappingException(instance.GetType(), baseType, "No descendants found");

            return descendants
                .OrderByDescending(type => signatures.Value[type].Intersect(properties).Count())
                .First();
        }

        private IDictionary<Type, Type[]> CollectTypes()
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();

            var types = assemblies
                .SelectMany(domainAssembly => domainAssembly.GetTypes())
                .ToArray();

            return types
                .ToDictionary(
                    t => t,
                    t => types.Where(s => s != t && t.IsAssignableFrom(s) && s.IsClass && !s.IsAbstract).ToArray()
                )
                .Where(p => p.Value.Length > 0)
                .ToDictionary(p => p.Key, p => p.Value);
        }

        private IDictionary<Type, string[]> CollectSignatures(IDictionary<Type, Type[]> types) => types.Values
            .SelectMany(v => v)
            .Distinct()
            .ToDictionary(
                t => t,
                t => t.GetProperties().Select(p => p.Name.ToLowerInvariant()).OrderBy(p => p).ToArray()
            )
            .Where(p => p.Value.Length > 0)
            .ToDictionary(p => p.Key, p => p.Value);
    }
}
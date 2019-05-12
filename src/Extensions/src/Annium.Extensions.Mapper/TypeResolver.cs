using System;
using System.Collections.Generic;
using System.Linq;

namespace Annium.Extensions.Mapper
{
    public class TypeResolver
    {
        public static TypeResolver Instance = new TypeResolver();

        private readonly Lazy<IDictionary<Type, Type[]>> types;

        private readonly Lazy<IDictionary<Type, string[]>> signatures;

        internal TypeResolver()
        {
            types = new Lazy<IDictionary<Type, Type[]>>(CollectTypes, true);
            signatures = new Lazy<IDictionary<Type, string[]>>(() => CollectSignatures(types.Value));
        }

        public bool CanResolve(Type baseType) => types.Value.ContainsKey(baseType);

        public Type ResolveBySignature(object instance, Type baseType) =>
            ResolveBySignature(
                instance.GetType(),
                instance.GetType().GetProperties().Select(p => p.Name.ToLowerInvariant()).OrderBy(p => p).ToArray(),
                baseType
            );

        public Type ResolveBySignature(string[] signature, Type baseType) =>
            ResolveBySignature(typeof(object), signature, baseType);

        private Type ResolveBySignature(Type src, string[] signature, Type baseType)
        {
            if (!types.Value.TryGetValue(baseType, out var descendants))
                throw new MappingException(src, baseType, "No descendants found");

            return descendants
                .OrderByDescending(type => signatures.Value[type].Intersect(signature).Count())
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
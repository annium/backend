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

        public Type ResolveBySignature(object instance, Type baseType, bool exact) =>
            ResolveBySignature(
                instance.GetType(),
                instance.GetType().GetProperties().Select(p => p.Name.ToLowerInvariant()).OrderBy(p => p).ToArray(),
                baseType,
                exact
            );

        public Type ResolveBySignature(string[] signature, Type baseType, bool exact) =>
            ResolveBySignature(typeof(object), signature, baseType, exact);

        private Type ResolveBySignature(Type src, string[] signature, Type baseType, bool exact)
        {
            if (!types.Value.TryGetValue(baseType, out var descendants))
                throw new MappingException(src, baseType, "No descendants found");

            var lookup = descendants
                .Select(type => (type: type, match: signatures.Value[type].Intersect(signature).Count()))
                .OrderByDescending(p => p.match)
                .ToList();

            if (lookup.Count == 0)
                return null;

            if (lookup.Count > 1)
            {
                var rivals = lookup.Where(p => p.match == lookup[0].match).Select(p => p.type.FullName).ToArray();
                if (rivals.Length > 1)
                    throw new MappingException(src, baseType, $"Ambiguous resolution between {string.Join(", ",rivals)}");
            }

            var resolution = lookup[0];
            if (exact)
                return resolution.match == signature.Length ? resolution.type : null;

            return resolution.type;
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
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Annium.Core.Runtime.Types
{
    public class TypeManager : ITypeManager
    {
        private static readonly ConcurrentDictionary<Assembly, ITypeManager> Instances = new ConcurrentDictionary<Assembly, ITypeManager>();
        public static readonly ITypeManager Instance = new TypeManager(Assembly.GetEntryAssembly()!);
        public static ITypeManager GetInstance(Assembly assembly) => Instances.GetOrAdd(assembly, a => new TypeManager(a));
        public IReadOnlyCollection<Type> Types => _types.Value;
        private readonly Lazy<HashSet<Type>> _types;
        private readonly Lazy<IReadOnlyDictionary<Type, HashSet<Type>>> _descendants;
        private readonly Lazy<IReadOnlyDictionary<Type, HashSet<string>>> _signatures;

        private TypeManager(
            Assembly assembly
        )
        {
            _types = new Lazy<HashSet<Type>>(new TypesCollector(assembly).CollectTypes, true);
            _descendants = new Lazy<IReadOnlyDictionary<Type, HashSet<Type>>>(CollectDescendants, true);
            _signatures = new Lazy<IReadOnlyDictionary<Type, HashSet<string>>>(CollectSignatures, true);
        }

        public Type? GetByName(string name) => _types.Value.FirstOrDefault(t => t.FullName == name);

        // returns whether given type is registered with some of subtypes
        public bool CanResolve(Type baseType) => _descendants.Value.ContainsKey(baseType);

        public Type[] GetImplementations(Type baseType)
        {
            // if baseType is not generic or baseType is generic type definition - it will be registered explicitly an all of it's dependants match
            if (!baseType.IsGenericType || baseType.IsGenericTypeDefinition)
                return GetDescendants(baseType);

            // if baseType is generic type - select descendants, assignable from it
            return GetDescendants(baseType)
                .Where(t => baseType.IsAssignableFrom(t))
                .ToArray();

            Type[] GetDescendants(Type type) =>
                _descendants.Value.TryGetValue(type, out var implementations)
                    ? implementations.ToArray()
                    : Type.EmptyTypes;
        }

        // resolve target type by base type and source instance
        // exact requires exact properties matching, otherwise - best matching type is selected
        public Type? ResolveBySignature(object instance, Type baseType, bool exact) =>
            ResolveBySignature(
                instance.GetType(),
                instance.GetType().GetProperties().Select(p => p.Name.ToLowerInvariant()).OrderBy(p => p).ToArray(),
                baseType,
                exact
            );

        // resolve target type by base type and source signature
        // exact requires exact properties matching, otherwise - best matching type is selected
        public Type? ResolveBySignature(string[] signature, Type baseType, bool exact) =>
            ResolveBySignature(
                typeof(object),
                signature.Select(p => p.ToLowerInvariant()).OrderBy(p => p).ToArray(),
                baseType,
                exact
            );

        // reolve type by key (for labeled types)
        public Type? ResolveByKey(string key, Type baseType)
        {
            var baseTypeDefinition = baseType.IsGenericType ? baseType.GetGenericTypeDefinition() : baseType;
            if (!_descendants.Value.TryGetValue(baseTypeDefinition, out var typeDescendants))
                throw new TypeResolutionException(typeof(object), baseTypeDefinition, "No descendants found");

            var resolutions = typeDescendants
                .Where(type => type.GetTypeInfo().GetCustomAttribute<ResolveKeyAttribute>()?.Key == key)
                .ToList();

            if (resolutions.Count > 1)
                throw new TypeResolutionException(typeof(object), baseTypeDefinition,
                    $"Ambiguous resolution between {string.Join(", ", resolutions.Select(r => r.FullName))}");

            return resolutions.FirstOrDefault();
        }

        // find type, derived
        private Type? ResolveBySignature(Type src, string[] signature, Type baseType, bool exact)
        {
            var baseTypeDefinition = baseType.IsGenericType ? baseType.GetGenericTypeDefinition() : baseType;
            if (!_descendants.Value.TryGetValue(baseTypeDefinition, out var typeDescendants))
                throw new TypeResolutionException(src, baseTypeDefinition, "No descendants found");

            var lookup = typeDescendants
                .Where(type => _signatures.Value.ContainsKey(type))
                .Select(type => (type, match: _signatures.Value[type].Intersect(signature).Count()))
                .OrderByDescending(p => p.match)
                .ToList();

            if (lookup.Count == 0)
                return null;

            if (lookup.Count > 1)
            {
                var rivals = lookup.Where(p => p.match == lookup[0].match).Select(p => p.type.FullName).ToArray();
                if (rivals.Length > 1)
                    throw new TypeResolutionException(src, baseTypeDefinition, $"Ambiguous resolution between {string.Join(", ", rivals)}");
            }

            var resolution = lookup[0];
            if (exact)
                return resolution.match == signature.Length ? resolution.type : null;

            return resolution.type;
        }

        private IReadOnlyDictionary<Type, HashSet<Type>> CollectDescendants() => new DescendantsCollector().CollectDescendants(_types.Value);

        // collect signatures from given types
        // each signature is array of lowercased property names
        private IReadOnlyDictionary<Type, HashSet<string>> CollectSignatures() => _descendants.Value.Values
            .SelectMany(v => v)
            .Distinct()
            .ToDictionary(
                t => t,
                t => t.GetProperties().Select(p => p.Name.ToLowerInvariant()).OrderBy(p => p).ToHashSet()
            )
            .Where(p => p.Value.Count > 0)
            .ToDictionary(p => p.Key, p => p.Value);
    }
}
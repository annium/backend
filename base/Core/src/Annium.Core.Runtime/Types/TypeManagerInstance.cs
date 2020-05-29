using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Annium.Core.Runtime.Types
{
    internal class TypeManagerInstance : ITypeManager
    {
        /// <summary>
        /// Contains collection of all types, collected for given Assembly
        /// </summary>
        public IReadOnlyCollection<Type> Types => _types.Value;

        private readonly Lazy<HashSet<Type>> _types;
        private readonly Lazy<IReadOnlyDictionary<Type, HashSet<Type>>> _descendants;
        private readonly Lazy<IReadOnlyDictionary<Type, HashSet<string>>> _signatures;

        public TypeManagerInstance(
            Assembly assembly
        )
        {
            _types = new Lazy<HashSet<Type>>(new TypesCollector(assembly).CollectTypes, true);
            _descendants = new Lazy<IReadOnlyDictionary<Type, HashSet<Type>>>(CollectDescendants, true);
            _signatures = new Lazy<IReadOnlyDictionary<Type, HashSet<string>>>(CollectSignatures, true);
        }

        /// <summary>
        /// Returns single type among discovered, that matches given fullName
        /// </summary>
        /// <param name="fullName"></param>
        /// <returns></returns>
        public Type? GetByName(string fullName) => _types.Value.SingleOrDefault(t => t.FullName == fullName);

        /// <summary>
        /// Returns whether given type is registered with one or more of subtypes.
        /// </summary>
        /// <param name="baseType"></param>
        public bool CanResolve(Type baseType) => _descendants.Value.ContainsKey(baseType);

        /// <summary>
        /// Returns all direct implementations of <see cref="baseType"/>.
        /// </summary>
        /// <param name="baseType"></param>
        /// <returns></returns>
        public Type[] GetImplementations(Type baseType)
        {
            // if baseType is not generic or baseType is generic type definition - it will be registered explicitly and all of it's dependants match
            if (!baseType.IsGenericType || baseType.IsGenericTypeDefinition)
                return GetDescendants(baseType);

            // if baseType is generic type - select descendants, assignable from it
            return GetDescendants(baseType)
                .Where(baseType.IsAssignableFrom)
                .ToArray();

            Type[] GetDescendants(Type type) =>
                _descendants.Value.TryGetValue(type, out var implementations)
                    ? implementations.ToArray()
                    : Type.EmptyTypes;
        }

        /// <summary>
        /// Resolve target type by base type and source instance.
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="baseType"></param>
        /// <param name="exact">Requires exact properties matching, otherwise - best matching type is selected.</param>
        /// <returns></returns>
        public Type? ResolveBySignature(object instance, Type baseType, bool exact) =>
            ResolveBySignature(
                instance.GetType(),
                instance.GetType().GetProperties().Select(p => p.Name.ToLowerInvariant()).OrderBy(p => p).ToArray(),
                baseType,
                exact
            );

        /// <summary>
        /// Resolve target type by base type and source signature
        /// </summary>
        /// <param name="signature"></param>
        /// <param name="baseType"></param>
        /// <param name="exact">Requires exact properties matching, otherwise - best matching type is selected.</param>
        /// <returns></returns>
        public Type? ResolveBySignature(string[] signature, Type baseType, bool exact) =>
            ResolveBySignature(
                typeof(object),
                signature.Select(p => p.ToLowerInvariant()).OrderBy(p => p).ToArray(),
                baseType,
                exact
            );

        /// <summary>
        /// Resolve type by key (for labeled types).
        /// </summary>
        /// <param name="key"></param>
        /// <param name="baseType"></param>
        /// <returns></returns>
        /// <exception cref="TypeResolutionException"></exception>
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

        /// <summary>
        /// Resolve type by signature.
        /// </summary>
        /// <param name="src"></param>
        /// <param name="signature"></param>
        /// <param name="baseType"></param>
        /// <param name="exact"></param>
        /// <returns></returns>
        /// <exception cref="TypeResolutionException"></exception>
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

        /// <summary>
        /// Collect signatures from given types. Each signature is array of lowercased property names.
        /// </summary>
        /// <returns></returns>
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
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
        private readonly Assembly _assembly;
        private readonly Lazy<Type[]> _types;
        private readonly Lazy<IDictionary<Type, Type[]>> _descendants;
        private readonly Lazy<IDictionary<Type, string[]>> _signatures;

        internal TypeManager(
            Assembly assembly
        )
        {
            _types = new Lazy<Type[]>(new TypesCollector(assembly).CollectTypes, true);
            _descendants = new Lazy<IDictionary<Type, Type[]>>(CollectDescendants, true);
            _signatures = new Lazy<IDictionary<Type, string[]>>(CollectSignatures, true);
        }

        public Type? GetByName(string name) => _types.Value.FirstOrDefault(t => t.FullName == name);

        // returns whether given type is registered with some of subtypes
        public bool CanResolve(Type baseType) => _descendants.Value.ContainsKey(baseType);

        public Type[] GetImplementations(Type baseType)
        {
            var types = this._types.Value;

            // handle generic type definition
            if (baseType.IsGenericTypeDefinition)
            {
                // generic interface
                if (baseType.IsInterface)
                    return types
                        .Where(t =>
                        {
                            if (t == baseType)
                                return false;

                            return t.GetInterfaces()
                                .Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == baseType);
                        })
                        .ToArray();

                return types
                    .Where(t =>
                    {
                        if (t == baseType)
                            return false;

                        while (t != null && t != typeof(object))
                        {
                            var checkedType = t.IsGenericType ? t.GetGenericTypeDefinition() : t;
                            if (checkedType == baseType)
                                return true;

                            t = t.BaseType!;
                        }

                        return false;
                    })
                    .ToArray();
            }

            return types
                .Where(t => t != baseType && baseType.IsAssignableFrom(t))
                .ToArray();
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

        // collect types with derived types from all loaded assemblies
        private IDictionary<Type, Type[]> CollectDescendants()
        {
            var types = this._types.Value;

            var result = new Dictionary<Type, Type[]>();
            foreach (var type in types)
            {
                // can have descendants if type is interface or class
                if (!type.IsInterface && !type.IsClass)
                    continue;

                Type[] descendants = Type.EmptyTypes;
                if (type.IsGenericTypeDefinition)
                {
                    if (type.IsInterface)
                        descendants = types
                            .Where(x => x != type && x.IsClass && !x.IsAbstract && x.GetInterfaces()
                                .Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == type)
                            )
                            .ToArray();

                    if (type.IsClass)
                    {
                        descendants = types.Where(x =>
                            {
                                if (x == type || !x.IsClass || x.IsAbstract)
                                    return false;

                                if (x.BaseType == null || x.BaseType == typeof(object))
                                    return false;

                                while (x != null)
                                {
                                    if (x.IsGenericType && x.GetGenericTypeDefinition() == type)
                                        return true;

                                    x = x.BaseType!;
                                }

                                return false;
                            })
                            .ToArray();
                    }
                }
                else
                {
                    descendants = types
                        .Where(x => x != type && x.IsClass && !x.IsAbstract && type.IsAssignableFrom(x))
                        .ToArray();
                }

                if (descendants.Length > 0)
                    result[type] = descendants;
            }

            return result;
        }

        // collect signatures from given types
        // each signature is array of lowercased property names
        private IDictionary<Type, string[]> CollectSignatures() => _descendants.Value.Values
            .SelectMany(v => v)
            .Distinct()
            .ToDictionary(
                t => t,
                t => t.GetProperties().Select(p => p.Name.ToLowerInvariant()).Distinct().OrderBy(p => p).ToArray()
            )
            .Where(p => p.Value.Length > 0)
            .ToDictionary(p => p.Key, p => p.Value);
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Annium.Core.Reflection
{
    public class TypeManager
    {
        public static readonly TypeManager Instance = new TypeManager();
        private readonly Lazy<Type[]> types;
        private readonly Lazy<IDictionary<Type, Type[]>> descendants;
        private readonly Lazy<IDictionary<Type, string[]>> signatures;

        internal TypeManager()
        {
            types = new Lazy<Type[]>(CollectTypes, true);
            descendants = new Lazy<IDictionary<Type, Type[]>>(CollectDescendants, true);
            signatures = new Lazy<IDictionary<Type, string[]>>(() => CollectSignatures(descendants.Value), true);
        }

        // returns whether given type is registered with some of subtypes
        public bool CanResolve(Type baseType) => descendants.Value.ContainsKey(baseType);

        public Type[] GetImplementations(Type baseType)
        {
            var types = this.types.Value;

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
        public Type ResolveByKey(string key, Type baseType)
        {
            if (!descendants.Value.TryGetValue(baseType, out var typeDescendants))
                throw new TypeResolutionException(typeof(object), baseType, "No descendants found");

            var resolutions = typeDescendants
                .Where(type => type.GetTypeInfo().GetCustomAttribute<ResolveKeyAttribute>()?.Key == key)
                .ToList();

            if (resolutions.Count > 1)
                throw new TypeResolutionException(typeof(object), baseType, $"Ambiguous resolution between {string.Join(", ", resolutions.Select(r => r.FullName))}");

            return resolutions.FirstOrDefault();
        }

        // find type, derived
        private Type? ResolveBySignature(Type src, string[] signature, Type baseType, bool exact)
        {
            if (!descendants.Value.TryGetValue(baseType, out var typeDescendants))
                throw new TypeResolutionException(src, baseType, "No descendants found");

            var lookup = typeDescendants
                .Where(type => signatures.Value.ContainsKey(type))
                .Select(type => (type: type, match: signatures.Value[type].Intersect(signature).Count()))
                .OrderByDescending(p => p.match)
                .ToList();

            if (lookup.Count == 0)
                return null;

            if (lookup.Count > 1)
            {
                var rivals = lookup.Where(p => p.match == lookup[0].match).Select(p => p.type.FullName).ToArray();
                if (rivals.Length > 1)
                    throw new TypeResolutionException(src, baseType, $"Ambiguous resolution between {string.Join(", ", rivals)}");
            }

            var resolution = lookup[0];
            if (exact)
                return resolution.match == signature.Length ? resolution.type : null;

            return resolution.type;
        }

        // collect types from all loaded assemblies
        private Type[] CollectTypes()
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();

            return assemblies
                .SelectMany(domainAssembly => domainAssembly.GetTypes())
                .ToArray();
        }

        // collect types with derived types from all loaded assemblies
        private IDictionary<Type, Type[]> CollectDescendants()
        {
            var types = this.types.Value;

            return types
                .ToDictionary(
                    t => t,
                    t => types.Where(s => s != t && t.IsAssignableFrom(s) && s.IsClass && !s.IsAbstract).ToArray()
                )
                .Where(p => p.Value.Length > 0)
                .ToDictionary(p => p.Key, p => p.Value);
        }

        // collect signatures from given types
        // each signature is array of lowercased property names
        private IDictionary<Type, string[]> CollectSignatures(IDictionary<Type, Type[]> types) => types.Values
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
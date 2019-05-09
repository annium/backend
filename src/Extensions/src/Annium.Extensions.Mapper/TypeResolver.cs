using System;
using System.Collections.Generic;
using System.Linq;

namespace Annium.Extensions.Mapper
{
    internal class TypeResolver
    {
        private readonly IDictionary<Type, Type[]> types;

        private readonly IDictionary<Type, string[]> signatures;

        public TypeResolver()
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();

            var types = assemblies
                .SelectMany(domainAssembly => domainAssembly.GetTypes())
                .ToArray();

            this.types = types
                .ToDictionary(
                    t => t,
                    t => types.Where(s => s != t && t.IsAssignableFrom(s) && s.IsClass && !s.IsAbstract).ToArray()
                )
                .Where(p => p.Value.Length > 0)
                .ToDictionary(p => p.Key, p => p.Value);

            this.signatures = this.types.Values
                .SelectMany(v => v)
                .Distinct()
                .ToDictionary(
                    t => t,
                    t => t.GetProperties().Select(p => p.Name.ToLowerInvariant()).OrderBy(p => p).ToArray()
                )
                .Where(p => p.Value.Length > 0)
                .ToDictionary(p => p.Key, p => p.Value);
        }

        public Type Resolve(object instance, Type baseType)
        {
            var properties = instance.GetType().GetProperties().Select(p => p.Name.ToLowerInvariant()).OrderBy(p => p).ToArray();

            if (!types.TryGetValue(baseType, out var descendants))
                throw new MappingException(instance.GetType(), baseType, "No descendants found");

            return descendants
                .OrderByDescending(type => signatures[type].Intersect(properties).Count())
                .First();
        }
    }
}
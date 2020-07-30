using System;
using System.Collections.Generic;
using System.Reflection;

namespace Annium.Core.Runtime.Types
{
    internal class TypesCollector
    {
        private readonly Assembly _assembly;
        private readonly IReadOnlyDictionary<string, Assembly> _assemblies;
        private readonly HashSet<Assembly> _processedAssemblies = new HashSet<Assembly>();

        public TypesCollector(
            Assembly assembly
        )
        {
            _assembly = assembly;

            var assemblies = new Dictionary<string, Assembly>();
            foreach (var domainAssembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (!assemblies.ContainsKey(domainAssembly.FullName))
                    assemblies[domainAssembly.FullName] = domainAssembly;
            }

            _assemblies = assemblies;
        }

        public HashSet<Type> CollectTypes()
        {
            var types = new HashSet<Type>();
            CollectTypes(_assembly.GetName(), type => types.Add(type));
            return types;
        }

        private void CollectTypes(AssemblyName name, Action<Type> add)
        {
            if (!_assemblies.TryGetValue(name.FullName, out var assembly))
                return;
            if (!_processedAssemblies.Add(assembly))
                return;
            foreach (var type in assembly.GetTypes())

                add(type);
            foreach (var assemblyName in assembly.GetReferencedAssemblies())
                CollectTypes(assemblyName, add);
        }
    }
}
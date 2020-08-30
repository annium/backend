using System;
using System.Collections.Generic;
using System.Reflection;

namespace Annium.Core.Runtime.Internal.Types
{
    internal class TypesCollector
    {
        public HashSet<Type> CollectTypes(Assembly assembly, bool tryLoadReferences)
        {
            // collect assemblies, already residing in AppDomain
            var assemblies = new Dictionary<string, Assembly>();
            foreach (var domainAssembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (!assemblies.ContainsKey(domainAssembly.FullName))
                    assemblies[domainAssembly.FullName] = domainAssembly;
            }

            // list of processed assemblies
            var processedAssemblies = new HashSet<Assembly>();

            // list of collected types
            var types = new HashSet<Type>();

            var resolveAssembly = tryLoadReferences ? (Func<AssemblyName, Assembly?>) LoadAssembly : GetAssembly;
            CollectTypes(
                assembly.GetName(),
                resolveAssembly,
                processedAssemblies.Add,
                type => types.Add(type)
            );

            return types;

            Assembly? GetAssembly(AssemblyName name)
            {
                if (assemblies.TryGetValue(name.FullName, out var asm))
                    return asm;

                return null;
            }

            Assembly? LoadAssembly(AssemblyName name)
            {
                if (assemblies.TryGetValue(name.FullName, out var asm))
                    return asm;

                return assemblies[name.FullName] = AppDomain.CurrentDomain.Load(name);
            }
        }

        private void CollectTypes(
            AssemblyName name,
            Func<AssemblyName, Assembly?> resolveAssembly,
            Func<Assembly, bool> registerAssembly,
            Action<Type> registerType
        )
        {
            var assembly = resolveAssembly(name);
            if (assembly is null)
                return;
            if (!registerAssembly(assembly))
                return;
            foreach (var type in assembly.GetTypes())
                registerType(type);
            foreach (var assemblyName in assembly.GetReferencedAssemblies())
                CollectTypes(assemblyName, resolveAssembly, registerAssembly, registerType);
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Annium.Core.Internal;
using Annium.Core.Primitives;

namespace Annium.Core.Runtime.Internal.Types
{
    internal class TypesCollector
    {
        private readonly Assembly _assembly;
        private readonly bool _tryLoadReferences;
        private readonly IReadOnlyCollection<string> _patterns;
        private readonly Dictionary<string, Assembly> _assemblies = new();


        public TypesCollector(
            Assembly assembly,
            bool tryLoadReferences,
            IReadOnlyCollection<string> patterns
        )
        {
            _assembly = assembly;
            _tryLoadReferences = tryLoadReferences;
            _patterns = patterns;
        }

        public HashSet<Type> CollectTypes()
        {
            this.Trace(() => "start");
            // collect assemblies, already residing in AppDomain
            foreach (var domainAssembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (domainAssembly.FullName != null! && !_assemblies.ContainsKey(domainAssembly.FullName) && IsMatch(domainAssembly.GetName()))
                    _assemblies[domainAssembly.FullName] = domainAssembly;
            }

            // list of processed assemblies
            var processedAssemblies = new HashSet<Assembly>();

            // list of collected types
            var types = new HashSet<Type>();

            var resolveAssembly = _tryLoadReferences ? (Func<AssemblyName, Assembly?>) LoadAssembly : GetAssembly;
            CollectTypes(
                _assembly.GetName(),
                resolveAssembly,
                processedAssemblies.Add,
                type => types.Add(type)
            );

            this.Trace(() => "done");
            return types;
        }

        private void CollectTypes(
            AssemblyName name,
            Func<AssemblyName, Assembly?> resolveAssembly,
            Func<Assembly, bool> registerAssembly,
            Action<Type> registerType
        )
        {
            this.Trace(() => $"from {name.Name}");
            var assembly = resolveAssembly(name);
            if (assembly is null)
            {
                this.Trace(() => $"assembly {name.Name} is null");
                return;
            }

            if (!registerAssembly(assembly))
            {
                this.Trace(() => $"assembly {name.Name} already registered");
                return;
            }

            var types = assembly.GetTypes();
            this.Trace(() => $"register {types.Length} type(s) from assembly {name.Name}");
            foreach (var type in assembly.GetTypes())
                registerType(type);

            foreach (var assemblyName in assembly.GetReferencedAssemblies().Where(IsMatch))
                CollectTypes(assemblyName, resolveAssembly, registerAssembly, registerType);
        }

        private bool IsMatch(AssemblyName assemblyName)
        {
            var name = assemblyName.Name;
            var match = _patterns.Any(name.IsLike);
            this.Trace(() => $"{name} -> {match}");

            return match;
        }

        private Assembly? GetAssembly(AssemblyName name)
        {
            if (_assemblies.TryGetValue(name.FullName, out var asm))
                return asm;

            return null;
        }

        private Assembly? LoadAssembly(AssemblyName name)
        {
            if (_assemblies.TryGetValue(name.FullName, out var asm))
                return asm;

            this.Trace(() => $"load {name}");
            return _assemblies[name.FullName] = AppDomain.CurrentDomain.Load(name);
        }
    }
}
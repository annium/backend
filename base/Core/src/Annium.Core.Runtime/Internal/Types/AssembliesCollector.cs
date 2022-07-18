using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Annium.Core.Internal;
using Annium.Core.Primitives.Reflection;
using Annium.Core.Runtime.Types;

namespace Annium.Core.Runtime.Internal.Types;

internal static class AssembliesCollector
{
    private static readonly TypeId AutoScannedTypeId = typeof(AutoScannedAttribute).GetId();

    public static IReadOnlyCollection<Assembly> Collect(
        Assembly assembly
    )
    {
        Log.Trace("start");

        // result parts
        var allAssemblies = new Dictionary<string, Assembly>();
        var processedAssemblies = new HashSet<Assembly>();
        var matchedAssemblies = new HashSet<Assembly>();

        // collect assemblies, already residing in AppDomain
        foreach (var domainAssembly in AppDomain.CurrentDomain.GetAssemblies())
            if (domainAssembly.FullName != null! && !allAssemblies.ContainsKey(domainAssembly.FullName))
            {
                Log.Trace($"{domainAssembly.FriendlyName()} - register with {domainAssembly.FullName}");
                allAssemblies[domainAssembly.FullName] = domainAssembly;
            }

        var resolveAssembly = LoadAssembly(allAssemblies);

        Collect(
            assembly.GetName(),
            resolveAssembly,
            processedAssemblies.Add,
            asm => matchedAssemblies.Add(asm)
        );

        Log.Trace("done");

        return matchedAssemblies;
    }

    private static void Collect(
        AssemblyName name,
        Func<AssemblyName, Assembly?> resolveAssembly,
        Func<Assembly, bool> registerAssembly,
        Action<Assembly> addMatchedAssembly
    )
    {
        var assembly = resolveAssembly(name);
        if (assembly is null)
        {
            Log.Trace($"{name.Name} - not resolved");
            return;
        }

        if (!registerAssembly(assembly))
            return;

        var autoScanned = assembly.GetCustomAttributes()
            .SingleOrDefault(x => x.GetType().GetId() == AutoScannedTypeId);
        if (autoScanned is null)
            Log.Trace($"{name.Name} - not marked as autoscanned");
        else
        {
            Log.Trace($"{name.Name} - matched");
            addMatchedAssembly(assembly);
            var dependencies = (Assembly[]) autoScanned.GetType()
                .GetProperty(nameof(AutoScannedAttribute.Dependencies))!
                .GetValue(autoScanned)!;
            foreach (var dependency in dependencies)
            {
                Log.Trace($"{name.Name} - add dependency {dependency.ShortName()}");
                addMatchedAssembly(dependency);
                Collect(dependency.GetName(), resolveAssembly, registerAssembly, addMatchedAssembly);
            }
        }

        foreach (var assemblyName in assembly.GetReferencedAssemblies())
            Collect(assemblyName, resolveAssembly, registerAssembly, addMatchedAssembly);
    }

    private static Func<AssemblyName, Assembly?> LoadAssembly(IDictionary<string, Assembly> assemblies) => name =>
    {
        if (assemblies.TryGetValue(name.FullName, out var asm))
            return asm;

        // Log.Trace($"load {name}");
        return assemblies[name.FullName] = AppDomain.CurrentDomain.Load(name);
    };
}
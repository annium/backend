using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Annium.Core.Runtime.Types;
using Annium.Logging;
using Annium.Reflection;

namespace Annium.Core.Runtime.Internal.Types;

internal class AssembliesCollector : ILogSubject
{
    private static readonly TypeId AutoScannedTypeId = typeof(AutoScannedAttribute).GetTypeId();

    public ILogger Logger { get; }

    public AssembliesCollector(ILogger logger)
    {
        Logger = logger;
    }

    public IReadOnlyCollection<Assembly> Collect(
        Assembly assembly
    )
    {
        this.Trace("start");

        // result parts
        var allAssemblies = new Dictionary<string, Assembly>();
        var processedAssemblies = new HashSet<Assembly>();
        var matchedAssemblies = new HashSet<Assembly>();

        // collect assemblies, already residing in AppDomain
        this.Trace("register AppDomain assemblies");
        foreach (var domainAssembly in AppDomain.CurrentDomain.GetAssemblies())
            if (domainAssembly.FullName != null! && !allAssemblies.ContainsKey(domainAssembly.FullName))
            {
                this.Trace($"{domainAssembly.FriendlyName()} - register with {domainAssembly.FullName}");
                allAssemblies[domainAssembly.FullName] = domainAssembly;
            }

        var resolveAssembly = LoadAssembly(allAssemblies);

        this.Trace($"collect {assembly} dependencies");
        Collect(
            assembly.GetName(),
            resolveAssembly,
            processedAssemblies.Add,
            asm => matchedAssemblies.Add(asm)
        );

        this.Trace("done");

        return matchedAssemblies;
    }

    private void Collect(
        AssemblyName name,
        Func<AssemblyName, Assembly?> resolveAssembly,
        Func<Assembly, bool> registerAssembly,
        Action<Assembly> addMatchedAssembly
    )
    {
        var assembly = resolveAssembly(name);
        if (assembly is null)
        {
            this.Trace($"{name.Name} - not resolved");
            return;
        }

        if (!registerAssembly(assembly))
            return;

        var autoScanned = assembly.GetCustomAttributes()
            .SingleOrDefault(x => x.GetType().GetTypeId() == AutoScannedTypeId);
        if (autoScanned is null)
            this.Trace($"{name.Name} - not marked as auto-scanned");
        else
        {
            this.Trace($"{name.Name} - matched");
            addMatchedAssembly(assembly);
            var dependencies = (Assembly[])autoScanned.GetType()
                .GetProperty(nameof(AutoScannedAttribute.Dependencies))!
                .GetValue(autoScanned)!;
            foreach (var dependency in dependencies)
            {
                this.Trace($"{name.Name} - add dependency {dependency.ShortName()}");
                addMatchedAssembly(dependency);
                Collect(dependency.GetName(), resolveAssembly, registerAssembly, addMatchedAssembly);
            }
        }

        foreach (var assemblyName in assembly.GetReferencedAssemblies())
            Collect(assemblyName, resolveAssembly, registerAssembly, addMatchedAssembly);
    }

    private Func<AssemblyName, Assembly?> LoadAssembly(IDictionary<string, Assembly> assemblies) => name =>
    {
        if (assemblies.TryGetValue(name.FullName, out var asm))
            return asm;

        this.Trace($"load {name}");
        return assemblies[name.FullName] = AppDomain.CurrentDomain.Load(name);
    };
}
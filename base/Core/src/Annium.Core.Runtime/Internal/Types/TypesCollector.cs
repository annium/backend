using System;
using System.Collections.Generic;
using System.Reflection;
using Annium.Debug;
using Annium.Reflection;

namespace Annium.Core.Runtime.Internal.Types;

internal class TypesCollector : ITraceSubject
{
    public ITracer Tracer { get; }

    public TypesCollector(ITracer tracer)
    {
        Tracer = tracer;
    }

    public IReadOnlyCollection<Type> Collect(IReadOnlyCollection<Assembly> assemblies)
    {
        this.Trace("start");

        // list of collected types
        var types = new HashSet<Type>();

        foreach (var assembly in assemblies)
        {
            var assemblyTypes = assembly.GetTypes();
            this.Trace($"register {assemblyTypes.Length} type(s) from assembly {assembly.ShortName()}");
            foreach (var type in assemblyTypes)
                types.Add(type);
        }

        this.Trace("done");

        return types;
    }
}
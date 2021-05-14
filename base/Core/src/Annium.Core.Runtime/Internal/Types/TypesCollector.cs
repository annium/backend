using System;
using System.Collections.Generic;
using System.Reflection;
using Annium.Core.Internal;
using Annium.Core.Primitives;

namespace Annium.Core.Runtime.Internal.Types
{
    internal static class TypesCollector
    {
        public static IReadOnlyCollection<Type> Collect(IReadOnlyCollection<Assembly> assemblies)
        {
            Log.Trace(() => "start");

            // list of collected types
            var types = new HashSet<Type>();

            foreach (var assembly in assemblies)
            {
                var assemblyTypes = assembly.GetTypes();
                Log.Trace(() => $"register {assemblyTypes.Length} type(s) from assembly {assembly.ShortName()}");
                foreach (var type in assemblyTypes)
                    types.Add(type);
            }

            Log.Trace(() => "done");

            return types;
        }
    }
}
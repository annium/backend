using System.Collections.Concurrent;
using System.Reflection;
using Annium.Core.Runtime.Internal.Types;

namespace Annium.Core.Runtime.Types
{
    public static class TypeManager
    {
        private static readonly ConcurrentDictionary<Assembly, ITypeManager> Instances =
            new ConcurrentDictionary<Assembly, ITypeManager>();

        public static ITypeManager GetInstance(Assembly assembly) =>
            Instances.GetOrAdd(assembly, a => new TypeManagerInstance(a));
    }
}
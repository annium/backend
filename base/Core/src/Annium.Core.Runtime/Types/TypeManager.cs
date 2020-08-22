using System;
using System.Collections.Concurrent;
using System.Reflection;
using Annium.Core.Runtime.Internal.Types;

namespace Annium.Core.Runtime.Types
{
    public static class TypeManager
    {
        private static readonly ConcurrentDictionary<ITrackingWeakReference<Assembly>, ITypeManager> Instances =
            new ConcurrentDictionary<ITrackingWeakReference<Assembly>, ITypeManager>();

        public static ITypeManager GetInstance(Assembly assembly)
        {
            var reference = TrackingWeakReference.Get(assembly);
            reference.Collected += () => Instances.TryRemove(reference, out _);

            return Instances.GetOrAdd(reference, x =>
            {
                if (!x.TryGetTarget(out var asm))
                    throw new ObjectDisposedException("Assembly reference is disposed");

                return new TypeManagerInstance(asm);
            });
        }
    }
}
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Loader;
using System.Threading.Tasks;

namespace Annium.Core.Runtime.Loader.Internal
{
    internal class AssemblyLoader : IAssemblyLoader
    {
        private readonly AssemblyLoadContext _context;

        public AssemblyLoader(
            IReadOnlyCollection<Func<AssemblyName, string?>> pathResolvers,
            IReadOnlyCollection<Func<AssemblyName, Task<byte[]>?>> byteArrayResolvers
        )
        {
            _context = new ResolvingLoadContext(pathResolvers, byteArrayResolvers);
        }

        public Assembly Load(string name)
        {
            var assembly = Load(new AssemblyName(name));

            return assembly;
        }

        private Assembly Load(AssemblyName name)
        {
            var assembly = _context.LoadFromAssemblyName(name);

            foreach (var referenceName in assembly.GetReferencedAssemblies())
                Load(referenceName);

            return assembly;
        }
    }
}
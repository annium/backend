using System.Reflection;
using System.Runtime.Loader;

namespace Annium.Core.Runtime.Loader
{
    public class DirectoryLoadContext : AssemblyLoadContext
    {
        private readonly AssemblyDependencyResolver _resolver;

        public DirectoryLoadContext(string directory)
        {
            _resolver = new AssemblyDependencyResolver(directory);
        }

        protected override Assembly? Load(AssemblyName assemblyName)
        {
            var assemblyPath = _resolver.ResolveAssemblyToPath(assemblyName);

            return assemblyPath is null ? null : LoadFromAssemblyPath(assemblyPath);
        }
    }
}
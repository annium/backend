using System.Reflection;
using System.Runtime.Loader;

namespace Annium.Core.Runtime.Loader
{
    public class PluginLoadContext : AssemblyLoadContext
    {
        private readonly AssemblyDependencyResolver _resolver;

        public PluginLoadContext(string assemblyPath)
        {
            _resolver = new AssemblyDependencyResolver(assemblyPath);
        }

        protected override Assembly? Load(AssemblyName assemblyName)
        {
            var assemblyPath = _resolver.ResolveAssemblyToPath(assemblyName);

            return assemblyPath is null ? null : LoadFromAssemblyPath(assemblyPath);
        }
    }
}
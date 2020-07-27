using System.Runtime.Loader;

namespace Annium.Core.Runtime.Loader.Internal
{
    internal class PluginLoadContextFactory : IPluginLoadContextFactory
    {
        public AssemblyLoadContext Create(string assemblyPath)
        {
            return new PluginLoadContext(assemblyPath);
        }
    }
}
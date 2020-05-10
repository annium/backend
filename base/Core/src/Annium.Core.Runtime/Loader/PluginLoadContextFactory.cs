using System.Runtime.Loader;

namespace Annium.Core.Runtime.Loader
{
    public class PluginLoadContextFactory
    {
        public AssemblyLoadContext Create(string assemblyPath)
        {
            return new PluginLoadContext(assemblyPath);
        }
    }
}
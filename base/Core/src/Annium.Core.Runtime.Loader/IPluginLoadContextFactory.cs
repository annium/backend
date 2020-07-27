using System.Runtime.Loader;

namespace Annium.Core.Runtime.Loader
{
    public interface IPluginLoadContextFactory
    {
        AssemblyLoadContext Create(string assemblyPath);
    }
}
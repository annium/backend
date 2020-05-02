using System.Runtime.Loader;

namespace Annium.Core.Runtime.Loader
{
    public class DirectoryLoadContextFactory
    {
        public AssemblyLoadContext Create(string assemblyPath)
        {
            return new DirectoryLoadContext(assemblyPath);
        }
    }
}
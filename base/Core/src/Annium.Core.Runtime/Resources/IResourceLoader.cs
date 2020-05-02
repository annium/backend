using System.Reflection;

namespace Annium.Core.Runtime.Resources
{
    public interface IResourceLoader
    {
        IResource[] Load(string prefix);

        IResource[] Load(string prefix, Assembly assembly);
    }
}
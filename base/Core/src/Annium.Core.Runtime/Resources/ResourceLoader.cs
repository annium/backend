using System.IO;
using System.Linq;
using System.Reflection;

namespace Annium.Core.Runtime.Resources
{
    internal class ResourceLoader : IResourceLoader
    {
        public IResource[] Load(string prefix) => Load(prefix, Assembly.GetCallingAssembly());

        public IResource[] Load(string prefix, Assembly assembly)
        {
            prefix = $"{assembly.GetName().Name}.{prefix}.";

            return assembly.GetManifestResourceNames()
                .Where(r => r.StartsWith(prefix))
                .Select(r =>
                {
                    var name = r.Substring(prefix.Length);
                    var rs = assembly.GetManifestResourceStream(r)!;
                    rs.Seek(0, SeekOrigin.Begin);

                    return new Resource(name, rs);
                })
                .ToArray();
        }
    }
}
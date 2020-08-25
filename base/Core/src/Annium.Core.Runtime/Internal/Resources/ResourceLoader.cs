using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Annium.Core.Runtime.Resources;

namespace Annium.Core.Runtime.Internal.Resources
{
    internal class ResourceLoader : IResourceLoader
    {
        public IReadOnlyCollection<IResource> Load(string prefix) => Load(prefix, Assembly.GetCallingAssembly());

        public IReadOnlyCollection<IResource> Load(string prefix, Assembly assembly)
        {
            prefix = $"{assembly.GetName().Name}.{prefix}.";

            var names = assembly.GetManifestResourceNames();

            return names
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
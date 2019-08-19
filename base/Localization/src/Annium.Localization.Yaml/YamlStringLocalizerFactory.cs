using System;
using System.Collections.Concurrent;
using System.IO;
using System.Reflection;
using Microsoft.Extensions.Localization;

namespace Annium.Localization.Yaml
{
    public class YamlStringLocalizerFactory : IStringLocalizerFactory
    {
        private ConcurrentDictionary<string, IStringLocalizer> localizers =
            new ConcurrentDictionary<string, IStringLocalizer>();

        public IStringLocalizer Create(Type resourceSource)
        {
            var assembly = resourceSource.GetTypeInfo().Assembly;
            var location = assembly.Location;
            var directory = Path.Combine(
                Path.GetDirectoryName(location),
                Path.Combine(resourceSource.Namespace.Substring(assembly.GetName().Name.Length).Split('.')),
                "locale"
            );

            return localizers.GetOrAdd(directory, root => new YamlStringLocalizer(root));
        }

        public IStringLocalizer Create(string baseName, string location)
        {
            throw new NotImplementedException();
        }
    }
}
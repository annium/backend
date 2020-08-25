using System.IO;
using Annium.Configuration.Yaml.Internal;

namespace Annium.Configuration.Abstractions
{
    public static class ConfigurationBuilderExtensions
    {
        public static IConfigurationBuilder AddYamlFile(
            this IConfigurationBuilder builder,
            string path,
            bool optional = false
        )
        {
            path = Path.GetFullPath(path);
            if (!File.Exists(path))
                if (optional)
                    return builder;
                else
                    throw new FileNotFoundException($"Yaml configuration file {path} not found and is not optional");

            var configuration = new YamlConfigurationProvider(path).Read();

            return builder.Add(configuration);
        }
    }
}
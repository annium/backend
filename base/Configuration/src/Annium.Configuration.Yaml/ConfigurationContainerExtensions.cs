using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Annium.Configuration.Yaml.Internal;

namespace Annium.Configuration.Abstractions;

public static class ConfigurationBuilderExtensions
{
    public static IConfigurationContainer AddYamlFile(
        this IConfigurationContainer container,
        string path,
        bool optional = false
    )
    {
        path = Path.GetFullPath(path);
        if (!File.Exists(path))
            if (optional)
                return container;
            else
                throw new FileNotFoundException($"Yaml configuration file {path} not found and is not optional");

        var raw = File.ReadAllText(path);
        var configuration = new YamlConfigurationProvider(raw).Read();

        container.Add(configuration);

        return container;
    }

    public static async Task<IConfigurationContainer> AddRemoteYaml(
        this IConfigurationContainer container,
        string uri,
        bool optional = false
    )
    {
        var client = new HttpClient();
        var message = new HttpRequestMessage(HttpMethod.Get, uri);
        var response = await client.SendAsync(message);
        if (!response.IsSuccessStatusCode)
            if (optional)
                return container;
            else
                throw new FileNotFoundException($"Yaml configuration not available at {uri} and is not optional");

        var raw = await response.Content.ReadAsStringAsync();
        var configuration = new YamlConfigurationProvider(raw).Read();

        container.Add(configuration);

        return container;
    }
}
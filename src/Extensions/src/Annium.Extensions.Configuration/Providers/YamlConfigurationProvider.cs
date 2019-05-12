using System.Collections.Generic;
using System.IO;
using YamlDotNet.RepresentationModel;

namespace Annium.Extensions.Configuration
{
    internal class YamlConfigurationProvider : ConfigurationProviderBase
    {
        private readonly string filePath;

        public YamlConfigurationProvider(string filePath)
        {
            this.filePath = filePath;
        }

        public override IReadOnlyDictionary<string[], string> Read()
        {
            Init();

            var stream = new YamlStream();
            using(var fs = File.OpenText(filePath))
            {
                stream.Load(fs);
            }

            Process((YamlMappingNode) stream.Documents[0].RootNode);

            return data;
        }

        private void Process(YamlMappingNode node)
        {
            foreach (var(key, value) in node.Children)
            {
                context.Push(((YamlScalarNode) key).Value);

                if (value is YamlMappingNode map)
                    Process(map);
                else if (value is YamlSequenceNode seq)
                    Process(seq);
                else
                    Process((YamlScalarNode) value);

                context.Pop();
            }
        }

        private void Process(YamlSequenceNode node)
        {
            var index = 0;
            foreach (var item in node)
            {
                context.Push(index.ToString());

                if (item is YamlMappingNode map)
                    Process(map);
                else if (item is YamlSequenceNode seq)
                    Process(seq);
                else
                    Process((YamlScalarNode) item);

                context.Pop();
                index++;
            }
        }

        private void Process(YamlScalarNode token)
        {
            data[path] = token.Value;
        }
    }

    public static class YamlConfigurationProviderExtensions
    {
        public static IConfigurationBuilder AddYamlFile(this IConfigurationBuilder builder, string path, bool optional = false)
        {
            path = Path.GetFullPath(path);
            if (!File.Exists(path))
                if (optional)
                    return builder;
                else
                    throw new FileNotFoundException($"Json configuration file {path} not found and is not optional");

            var configuration = new YamlConfigurationProvider(path).Read();

            return builder.Add(configuration);
        }
    }
}
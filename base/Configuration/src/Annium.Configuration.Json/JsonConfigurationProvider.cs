using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Annium.Configuration.Abstractions;

namespace Annium.Configuration.Json
{
    internal class JsonConfigurationProvider : ConfigurationProviderBase
    {
        private readonly string filePath;

        public JsonConfigurationProvider(string filePath)
        {
            this.filePath = filePath;
        }

        public override IReadOnlyDictionary<string[], string> Read()
        {
            Init();

            var element = JsonDocument.Parse(File.ReadAllBytes(filePath)).RootElement;

            Process(element);

            return data;
        }

        private void ProcessObject(JsonElement token)
        {
            foreach (var property in token.EnumerateObject())
            {
                context.Push(property.Name);
                Process(property.Value);
                context.Pop();
            }
        }

        private void ProcessArray(JsonElement token)
        {
            var index = 0;
            foreach (var item in token.EnumerateArray())
            {
                context.Push(index.ToString());
                Process(item);
                context.Pop();
                index++;
            }
        }

        private void ProcessLeaf(JsonElement token)
        {
            data[Path] = token.ToString();
        }

        private void Process(JsonElement element)
        {
            switch (element.ValueKind)
            {
                case JsonValueKind.Object:
                    ProcessObject(element);
                    break;
                case JsonValueKind.Array:
                    ProcessArray(element);
                    break;
                default:
                    ProcessLeaf(element);
                    break;
            }
        }
    }

    public static class JsonConfigurationProviderExtensions
    {
        public static IConfigurationBuilder AddJsonFile(this IConfigurationBuilder builder, string path, bool optional = false)
        {
            path = Path.GetFullPath(path);
            if (!File.Exists(path))
                if (optional)
                    return builder;
                else
                    throw new FileNotFoundException($"Json configuration file {path} not found and is not optional");

            var configuration = new JsonConfigurationProvider(path).Read();

            return builder.Add(configuration);
        }
    }
}
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Annium.Configuration.Abstractions;

namespace Annium.Configuration.Json.Internal
{
    internal class JsonConfigurationProvider : ConfigurationProviderBase
    {
        private readonly string _filePath;

        public JsonConfigurationProvider(string filePath)
        {
            _filePath = filePath;
        }

        public override IReadOnlyDictionary<string[], string> Read()
        {
            Init();

            var element = JsonDocument.Parse(File.ReadAllBytes(_filePath)).RootElement;

            Process(element);

            return Data;
        }

        private void ProcessObject(JsonElement token)
        {
            foreach (var property in token.EnumerateObject())
            {
                Context.Push(property.Name);
                Process(property.Value);
                Context.Pop();
            }
        }

        private void ProcessArray(JsonElement token)
        {
            var index = 0;
            foreach (var item in token.EnumerateArray())
            {
                Context.Push(index.ToString());
                Process(item);
                Context.Pop();
                index++;
            }
        }

        private void ProcessLeaf(JsonElement token)
        {
            Data[Path] = token.ToString();
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
}
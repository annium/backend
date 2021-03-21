using System;
using System.Text.Json;

namespace Annium.Serialization.Json
{
    public interface IJsonSerializersConfigurationBuilder
    {
        IJsonSerializersConfigurationBuilder Configure(Action<JsonSerializerOptions> configure);
        IJsonSerializersConfigurationBuilder Configure(Action<IServiceProvider, JsonSerializerOptions> configure);
        IJsonSerializersConfigurationBuilder SetDefault();
    }
}
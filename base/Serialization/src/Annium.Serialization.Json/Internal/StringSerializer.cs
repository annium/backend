using System;
using System.Text.Json;
using Annium.Logging.Abstractions;
using Annium.Serialization.Abstractions;

namespace Annium.Serialization.Json.Internal
{
    internal class StringSerializer : ISerializer<string>, ILogSubject
    {
        public ILogger Logger { get; }
        private readonly JsonSerializerOptions _options;

        public StringSerializer(
            ILogger<StringSerializer> logger,
            OptionsContainer options
        )
        {
            Logger = logger;
            _options = options.Value;
        }

        public T Deserialize<T>(string value)
        {
            try
            {
                return JsonSerializer.Deserialize<T>(value, _options)!;
            }
            catch (Exception e)
            {
                this.Log().Error("Failed to deserialize {value} as {type} with {error}", value, typeof(T), e);
                throw;
            }
        }

        public object Deserialize(Type type, string value)
        {
            try
            {
                return JsonSerializer.Deserialize(value, type, _options)!;
            }
            catch (Exception e)
            {
                this.Log().Error("Failed to deserialize {value} as {type} with {error}", value, type, e);
                throw;
            }
        }

        public string Serialize<T>(T value)
        {
            try
            {
                return JsonSerializer.Serialize(value, _options);
            }
            catch (Exception e)
            {
                this.Log().Error("Failed to serialize {value} as {type} with {error}", value?.ToString() ?? (object) "null", typeof(T), e);
                throw;
            }
        }

        public string Serialize(object value)
        {
            try
            {
                return JsonSerializer.Serialize(value, _options);
            }
            catch (Exception e)
            {
                this.Log().Error("Failed to serialize {value} as {type} with {error}", value, value?.GetType() ?? (object) "null", e);
                throw;
            }
        }
    }
}
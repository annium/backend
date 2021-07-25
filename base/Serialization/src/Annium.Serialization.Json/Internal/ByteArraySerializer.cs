using System;
using System.Text;
using System.Text.Json;
using Annium.Logging.Abstractions;
using Annium.Serialization.Abstractions;

namespace Annium.Serialization.Json.Internal
{
    internal class ByteArraySerializer : ISerializer<byte[]>, ILogSubject
    {
        public ILogger Logger { get; }
        private readonly JsonSerializerOptions _options;

        public ByteArraySerializer(
            ILogger<ByteArraySerializer> logger,
            OptionsContainer options
        )
        {
            Logger = logger;
            _options = options.Value;
        }

        public T Deserialize<T>(byte[] value)
        {
            try
            {
                return JsonSerializer.Deserialize<T>(value, _options)!;
            }
            catch (Exception e)
            {
                this.Log().Error("Failed to deserialize {value} as {type} with {error}", Encoding.UTF8.GetString(value), typeof(T), e);
                throw;
            }
        }

        public object Deserialize(Type type, byte[] value)
        {
            try
            {
                return JsonSerializer.Deserialize(value, type, _options)!;
            }
            catch (Exception e)
            {
                this.Log().Error("Failed to deserialize {value} as {type} with {error}", Encoding.UTF8.GetString(value), type, e);
                throw;
            }
        }

        public byte[] Serialize<T>(T value)
        {
            try
            {
                return JsonSerializer.SerializeToUtf8Bytes(value, _options);
            }
            catch (Exception e)
            {
                this.Log().Error("Failed to serialize {value} as {type} with {error}", value?.ToString() ?? (object) "null", typeof(T), e);
                throw;
            }
        }

        public byte[] Serialize(object value)
        {
            try
            {
                return JsonSerializer.SerializeToUtf8Bytes(value, _options);
            }
            catch (Exception e)
            {
                this.Log().Error("Failed to serialize {value} as {type} with {error}", value, value?.GetType() ?? (object) "null", e);
                throw;
            }
        }
    }
}
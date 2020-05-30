using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using Annium.Core.Entrypoint;
using Annium.Data.Operations;
using Annium.Extensions.Primitives;

namespace Demo.Data.Operations.Serialization
{
    public class Program
    {
        private static void Run(
            IServiceProvider provider,
            string[] args,
            CancellationToken token
        )
        {
            var result = Result.New()
                .Errors("plain", "other")
                .Error("labelA", "valueA")
                .Error("labelB", "valueB");

            var opts = new JsonSerializerOptions();
            opts.Converters.Add(new ResultConverter());

            var str = JsonSerializer.Serialize(result, opts);
            Console.WriteLine(str);
            var source = JsonSerializer.Deserialize<IResult>(str, opts);
        }

        public static int Main(string[] args) => new Entrypoint()
            .UseServicePack<ServicePack>()
            .Run(Run, args);
    }

    public class ResultConverter : ResultConverterBase<IResult>
    {
        protected override bool IsConvertibleInterface(Type type) => type == typeof(IResult);

        public override IResult Read(
            ref Utf8JsonReader reader,
            Type typeToConvert,
            JsonSerializerOptions options
        )
        {
            var value = Result.New();

            ReadErrors(ref reader, value, options);

            return value;
        }

        public override void Write(
            Utf8JsonWriter writer,
            IResult value,
            JsonSerializerOptions options
        )
        {
            writer.WriteStartObject();

            WriteErrors(writer, value, options);

            writer.WriteEndObject();
        }
    }

    public abstract class ResultConverterBase<T> : JsonConverter<T> where T : IResultBase<T>
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType.IsInterface ? IsConvertibleInterface(objectType) : objectType.GetInterfaces().Any(IsConvertibleInterface);
        }

        protected abstract bool IsConvertibleInterface(Type type);

        protected Type GetImplementation(Type type)
        {
            if (type.IsInterface)
                return type;

            return type.GetInterfaces()
                .First(IsConvertibleInterface);
        }

        protected void ReadErrors(
            ref Utf8JsonReader reader,
            T value,
            JsonSerializerOptions options
        )
        {
            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.PropertyName)
                {
                    var name = reader.GetString();

                    if (name.Equals(nameof(IResultBase.PlainErrors), StringComparison.InvariantCultureIgnoreCase))
                        value.Errors(JsonSerializer.Deserialize<IEnumerable<string>>(ref reader, options));
                    else if (name.Equals(nameof(IResultBase.LabeledErrors), StringComparison.InvariantCultureIgnoreCase))
                        value.Errors(JsonSerializer.Deserialize<IReadOnlyDictionary<string, IEnumerable<string>>>(ref reader, options));
                }
            }

            // var str = reader.GetString();

            // if (obj.Get(nameof(IResultBase.PlainErrors)) is JArray plainErrors)
            //     result.GetType()
            //     .GetMethod(nameof(IResultBase<object>.Errors), new [] { typeof(ICollection<string>) }) !
            //     .Invoke(result, new [] { plainErrors.ToObject<string[]>().ToList() });

            // if (obj.Get(nameof(IResultBase.LabeledErrors)) is JObject labeledErrors)
            //     result.GetType()
            //     .GetMethod(nameof(IResultBase<object>.Errors), new [] { typeof(IReadOnlyCollection<KeyValuePair<string, IEnumerable<string>>>) }) !
            //     .Invoke(result, new [] { labeledErrors.ToObject<Dictionary<string, string[]>>().ToDictionary(p => p.Key, p => p.Value as IEnumerable<string>) });

            // static JsonException getException() => new JsonException($"Can't deserialize {typeof(T)} from JSON");
        }

        protected void WriteErrors(
            Utf8JsonWriter writer,
            T value,
            JsonSerializerOptions options
        )
        {
            writer.WritePropertyName(nameof(IResultBase.PlainErrors).CamelCase());
            JsonSerializer.Serialize(writer, value.PlainErrors, options);

            writer.WritePropertyName(nameof(IResultBase.LabeledErrors).CamelCase());
            JsonSerializer.Serialize(writer, value.LabeledErrors, options);
        }
    }
}
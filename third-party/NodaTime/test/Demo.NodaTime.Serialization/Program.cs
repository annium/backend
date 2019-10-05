using System;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using Annium.Core.Entrypoint;
using Annium.NodaTime.Serialization.Json;
using NodaTime;

namespace Demo.NodaTime.Serialization
{
    public class Program
    {
        private static void Run(
            IServiceProvider provider,
            string[] args,
            CancellationToken token
        )
        {
            var opts = With(Converters.DateIntervalConverter, Converters.LocalDateConverter);

            string json = "{\"interval\":{\"start\":\"2012-01-02\",\"end\":\"2013-06-07\"}}";

            var testObject = JsonSerializer.Deserialize<TestObject>(json, opts);
        }

        public class TestObject
        {
            public DateInterval Interval { get; set; }
        }

        public static int Main(string[] args) => new Entrypoint()
            .UseServicePack<ServicePack>()
            .Run(Run, args);

        internal static JsonSerializerOptions With(params JsonConverter[] converters)
        {
            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                DictionaryKeyPolicy = JsonNamingPolicy.CamelCase,
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            };
            foreach (var converter in converters)
                options.Converters.Add(converter);

            return options;
        }
    }
}
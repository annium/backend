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
            CancellationToken ct
        )
        {
            var converters = new[] { Converters.IsoDateIntervalConverter, Converters.LocalDateConverter };
            var startLocalDate = new LocalDate(2012, 1, 2);
            var endLocalDate = new LocalDate(2013, 6, 7);
            var dateInterval = new DateInterval(startLocalDate, endLocalDate);

            var testObject = new TestObject { Interval = dateInterval };

            var json = JsonSerializer.Serialize(testObject, With(converters));
        }

        public class TestObject
        {
            public DateInterval Interval { get; set; } = null!;
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
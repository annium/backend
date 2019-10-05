using System.Collections.Generic;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using Annium.NodaTime.Serialization.Json;
using NodaTime;

namespace Annium.Core.DependencyInjection
{
    public static class JsonSerializerOptionsExtensions
    {
        public static JsonSerializerOptions ConfigureForNodaTime(
            this JsonSerializerOptions options,
            IDateTimeZoneProvider provider
        )
        {
            options.Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping;
            options.DictionaryKeyPolicy = JsonNamingPolicy.CamelCase;

            AddDefaultConverters(options.Converters, provider);

            return options;
        }

        /// <summary>
        /// Configures the given serializer settings to use <see cref="Converters.IsoIntervalConverter"/>.
        /// Any other converters which can convert <see cref="Interval"/> are removed from the serializer.
        /// </summary>
        /// <param name="options">The existing serializer settings to add Noda Time converters to.</param>
        /// <returns>The original <paramref name="options"/> value, for further chaining.</returns>
        public static JsonSerializerOptions WithIsoIntervalConverter(this JsonSerializerOptions options)
        {
            ReplaceExistingConverters<Interval>(options.Converters, Converters.IsoIntervalConverter);

            return options;
        }

        /// <summary>
        /// Configures the given serializer settings to use <see cref="Converters.IsoDateIntervalConverter"/>.
        /// Any other converters which can convert <see cref="DateInterval"/> are removed from the serializer.
        /// </summary>
        /// <param name="options">The existing serializer settings to add Noda Time converters to.</param>
        /// <returns>The original <paramref name="options"/> value, for further chaining.</returns>
        public static JsonSerializerOptions WithIsoDateIntervalConverter(this JsonSerializerOptions options)
        {
            ReplaceExistingConverters<DateInterval>(options.Converters, Converters.IsoDateIntervalConverter);

            return options;
        }

        private static void AddDefaultConverters(
            IList<JsonConverter> converters,
            IDateTimeZoneProvider provider
        )
        {
            converters.Add(Converters.InstantConverter);
            converters.Add(Converters.IntervalConverter);
            converters.Add(Converters.LocalDateConverter);
            converters.Add(Converters.LocalDateTimeConverter);
            converters.Add(Converters.LocalTimeConverter);
            converters.Add(Converters.DateIntervalConverter);
            converters.Add(Converters.OffsetConverter);
            converters.Add(Converters.CreateDateTimeZoneConverter(provider));
            converters.Add(Converters.DurationConverter);
            converters.Add(Converters.RoundtripPeriodConverter);
            converters.Add(Converters.OffsetDateTimeConverter);
            converters.Add(Converters.OffsetDateConverter);
            converters.Add(Converters.OffsetTimeConverter);
            converters.Add(Converters.CreateZonedDateTimeConverter(provider));
        }

        private static void ReplaceExistingConverters<T>(IList<JsonConverter> converters, JsonConverter newConverter)
        {
            for (int i = converters.Count - 1; i >= 0; i--)
                if (converters[i].CanConvert(typeof(T)))
                    converters.RemoveAt(i);

            converters.Add(newConverter);
        }
    }
}
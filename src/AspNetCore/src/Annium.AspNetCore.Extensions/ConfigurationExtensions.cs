using Annium.AspNetCore.Extensions;
using NodaTime;
using NodaTime.Serialization.JsonNet;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ConfigurationExtensions
    {
        public static IMvcBuilder WithDataAnnotationsLocalization<TAnnotationsResource>(this IMvcBuilder builder) =>
            builder.AddDataAnnotationsLocalization(opts =>
                opts.DataAnnotationLocalizerProvider = (_, factory) => factory.Create(typeof(TAnnotationsResource)));

        public static IMvcBuilder WithJsonOptions(
            this IMvcBuilder builder,
            bool nodaTime = false,
            string[] abstractTypesSources = null
        ) => builder.AddJsonOptions(opts =>
        {
            if (nodaTime)
                opts.SerializerSettings.ConfigureForNodaTime(DateTimeZoneProviders.Serialization);

            if (abstractTypesSources?.Length > 0)
                opts.SerializerSettings.Converters.Add(new AbstractJsonConverter(abstractTypesSources));
        });
    }
}
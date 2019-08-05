using Microsoft.Extensions.DependencyInjection;
using NodaTime;
using NodaTime.Serialization.JsonNet;

namespace Annium.Core.DependencyInjection
{
    public static class ConfigurationExtensions
    {
        public static IMvcBuilder WithDataAnnotationsLocalization<TAnnotationsResource>(this IMvcBuilder builder) =>
            builder.AddDataAnnotationsLocalization(opts =>
                opts.DataAnnotationLocalizerProvider = (_, factory) => factory.Create(typeof(TAnnotationsResource)));

        public static IMvcBuilder WithJsonOptions(
            this IMvcBuilder builder
        ) => builder.AddJsonOptions(opts =>
        {
            opts.SerializerSettings.ConfigureForNodaTime(DateTimeZoneProviders.Serialization);

            opts.SerializerSettings.ConfigureAbstractConverter();
        });
    }
}
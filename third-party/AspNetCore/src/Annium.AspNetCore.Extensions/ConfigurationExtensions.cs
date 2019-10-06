using Microsoft.Extensions.DependencyInjection;
using NodaTime;

namespace Annium.Core.DependencyInjection
{
    public static class ConfigurationExtensions
    {
        public static IMvcBuilder AddDefaultJsonOptions(
            this IMvcBuilder builder
        ) => builder.AddJsonOptions(opts => opts.JsonSerializerOptions
            .ConfigureAbstractConverter()
            .ConfigureForOperations()
            .ConfigureForNodaTime(DateTimeZoneProviders.Serialization)
        );
    }
}
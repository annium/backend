using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using NodaTime;

namespace Annium.Core.DependencyInjection
{
    public static class ConfigurationExtensions
    {
        public static IMvcBuilder AddDefaultJsonOptions(
            this IMvcBuilder builder,
            Action<JsonOptions> configure
        ) => builder.AddJsonOptions(opts =>
        {
            opts.JsonSerializerOptions.ConfigureDefault()
                .ConfigureForOperations()
                .ConfigureForNodaTime(DateTimeZoneProviders.Serialization);

            configure(opts);
        });

        public static IMvcBuilder AddDefaultJsonOptions(this IMvcBuilder builder) =>
            builder.AddDefaultJsonOptions(opts => { });
    }
}
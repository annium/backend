using System;
using System.Text.Json;
using System.Text.Json.Serialization;
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
            opts.JsonSerializerOptions
                .ConfigureAbstractConverter()
                .ConfigureForOperations()
                .ConfigureForNodaTime(DateTimeZoneProviders.Serialization);
            opts.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
            opts.JsonSerializerOptions.DictionaryKeyPolicy = JsonNamingPolicy.CamelCase;
            opts.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
            opts.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;

            configure(opts);
        });

        public static IMvcBuilder AddDefaultJsonOptions(this IMvcBuilder builder) =>
            builder.AddDefaultJsonOptions(opts => { });
    }
}
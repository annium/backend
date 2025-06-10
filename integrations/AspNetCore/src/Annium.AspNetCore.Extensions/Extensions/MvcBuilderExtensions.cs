using System;
using Annium.AspNetCore.Extensions.Internal.DynamicControllers;
using Annium.Core.DependencyInjection.Container;
using Annium.Core.Runtime;
using Annium.Data.Operations.Serialization.Json;
using Annium.NodaTime.Serialization.Json;
using Annium.Serialization.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace Annium.AspNetCore.Extensions.Extensions;

/// <summary>
/// Extension methods for configuring MVC builder with additional functionality
/// </summary>
public static class MvcBuilderExtensions
{
    /// <summary>
    /// Adds dynamic controller support to the MVC application
    /// </summary>
    /// <param name="builder">The MVC builder to configure</param>
    /// <param name="configure">Action to configure the dynamic controller models</param>
    /// <returns>The configured MVC builder</returns>
    public static IMvcBuilder AddDynamicControllers(
        this IMvcBuilder builder,
        Action<IDynamicControllerModelPack> configure
    )
    {
        // resolve models
        var pack = new DynamicControllerModelPack();
        configure(pack);
        var models = pack.Models;

        // add feature provider
        builder.ConfigureApplicationPartManager(apm =>
            apm.FeatureProviders.Add(new DynamicControllerFeatureProvider(models))
        );

        // add route convention
        builder.Services.Configure<MvcOptions>(opts =>
            opts.Conventions.Add(new DynamicControllerRouteConvention(models))
        );

        return builder;
    }

    /// <summary>
    /// Configures JSON serialization options with default settings for operations and NodaTime support
    /// </summary>
    /// <param name="builder">The MVC builder to configure</param>
    /// <param name="configure">Action to configure additional JSON options</param>
    /// <returns>The configured MVC builder</returns>
    public static IMvcBuilder AddDefaultJsonOptions(this IMvcBuilder builder, Action<JsonOptions> configure) =>
        builder.AddJsonOptions(opts =>
        {
            var typeManager = new ServiceContainer(builder.Services).GetTypeManager();

            opts.JsonSerializerOptions.ConfigureDefault(typeManager).ConfigureForOperations().ConfigureForNodaTime();

            configure(opts);
        });

    /// <summary>
    /// Configures JSON serialization options with default settings for operations and NodaTime support
    /// </summary>
    /// <param name="builder">The MVC builder to configure</param>
    /// <returns>The configured MVC builder</returns>
    public static IMvcBuilder AddDefaultJsonOptions(this IMvcBuilder builder) =>
        builder.AddDefaultJsonOptions(_ => { });
}

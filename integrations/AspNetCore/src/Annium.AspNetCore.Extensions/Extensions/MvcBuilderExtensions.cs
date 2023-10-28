using System;
using Annium.AspNetCore.Extensions.Internal.DynamicControllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

// ReSharper disable once CheckNamespace
namespace Annium.Core.DependencyInjection;

public static class MvcBuilderExtensions
{
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
        builder.ConfigureApplicationPartManager(
            apm => apm.FeatureProviders.Add(new DynamicControllerFeatureProvider(models))
        );

        // add route convention
        builder.Services.Configure<MvcOptions>(
            opts => opts.Conventions.Add(new DynamicControllerRouteConvention(models))
        );

        return builder;
    }

    public static IMvcBuilder AddDefaultJsonOptions(this IMvcBuilder builder, Action<JsonOptions> configure) =>
        builder.AddJsonOptions(opts =>
        {
            var typeManager = new ServiceContainer(builder.Services).GetTypeManager();

            opts.JsonSerializerOptions.ConfigureDefault(typeManager).ConfigureForOperations().ConfigureForNodaTime();

            configure(opts);
        });

    public static IMvcBuilder AddDefaultJsonOptions(this IMvcBuilder builder) =>
        builder.AddDefaultJsonOptions(_ => { });
}

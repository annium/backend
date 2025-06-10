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
        builder.ConfigureApplicationPartManager(apm =>
            apm.FeatureProviders.Add(new DynamicControllerFeatureProvider(models))
        );

        // add route convention
        builder.Services.Configure<MvcOptions>(opts =>
            opts.Conventions.Add(new DynamicControllerRouteConvention(models))
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

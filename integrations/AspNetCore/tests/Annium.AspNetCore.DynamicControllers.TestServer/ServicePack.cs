using System;
using System.Threading;
using System.Threading.Tasks;
using Annium.AspNetCore.DynamicControllers.TestServer.Controllers;
using Annium.AspNetCore.Extensions;
using Annium.Core.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

namespace Annium.AspNetCore.DynamicControllers.TestServer;

/// <summary>
/// Service pack for the dynamic-controllers test server. Registers two controllers that opt out of
/// conventional MVC discovery (via <see cref="Microsoft.AspNetCore.Mvc.NonControllerAttribute" />) and
/// are wired up exclusively through <see cref="MvcBuilderExtensions.AddDynamicControllers" />, so that a
/// successful request pins both the feature provider (controller discovery) and the route convention
/// (route composition and route-value wiring). The assembly also contains a conventional
/// <see cref="Controllers.ConventionalController" />, discovered through the default <c>AddControllers()</c>
/// scan below rather than the dynamic feature provider, so the route convention's <c>model is null</c> guard
/// (which leaves non-dynamic controllers untouched) is exercised alongside the dynamic ones.
/// </summary>
public class ServicePack : ServicePackBase
{
    /// <summary>
    /// Registers MVC with two dynamic controllers: one without an area (plain route) and one with an
    /// area (area-prefixed route).
    /// </summary>
    /// <param name="container">The service container to register services with</param>
    /// <param name="provider">The service provider for dependency resolution</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>A task that represents the asynchronous registration.</returns>
    public override Task RegisterAsync(IServiceContainer container, IServiceProvider provider, CancellationToken ct)
    {
        container.Collection.AddControllers();
        container
            .Collection.AddMvc()
            .AddDynamicControllers(pack =>
                pack.Setup(null, "plain-key")
                    .Add<PlainDynamicController>("Plain", "plain-dynamic")
                    .Setup("dynamic-area", "area-key")
                    .Add<AreaDynamicController>("Area", "area-dynamic")
            );

        return Task.CompletedTask;
    }
}

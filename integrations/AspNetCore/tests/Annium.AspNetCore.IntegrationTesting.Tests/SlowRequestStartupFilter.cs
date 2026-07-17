using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.DependencyInjection;

namespace Annium.AspNetCore.IntegrationTesting.Tests;

/// <summary>
/// Test-only <see cref="IStartupFilter" /> that maps a single additional endpoint, <c>/slow</c>, which blocks
/// on the request's <see cref="RequestGate" /> before completing. Registered via
/// <see cref="Microsoft.Extensions.Hosting.IHostBuilder.ConfigureServices(System.Action{Microsoft.Extensions.Hosting.HostBuilderContext,IServiceCollection})" />
/// by <see cref="SlowRequestTestHost" />, mirroring the additive pattern used by
/// <see cref="KeyedTestHost" /> / <see cref="ScopedTestHost" />: it does not modify the shared
/// <see cref="Annium.AspNetCore.TestServer" /> project's <c>Program</c> in any way, it only wraps the
/// pipeline that the started host builds from it.
/// </summary>
internal sealed class SlowRequestStartupFilter : IStartupFilter
{
    /// <summary>
    /// Maps the <c>/slow</c> endpoint, which blocks on <see cref="RequestGate" /> before completing, then
    /// delegates to <paramref name="next" /> to configure the rest of the pipeline.
    /// </summary>
    /// <param name="next">The next pipeline-configuration delegate in the <see cref="IStartupFilter" /> chain.</param>
    /// <returns>
    /// An application-configuration delegate that maps the <c>/slow</c> endpoint ahead of the pipeline configured
    /// by <paramref name="next" />.
    /// </returns>
    public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next) =>
        app =>
        {
            app.Map(
                "/slow",
                branch =>
                    branch.Run(async context =>
                    {
                        var gate = context.RequestServices.GetRequiredService<RequestGate>();
                        await gate.WaitForReleaseAsync();
                        await context.Response.WriteAsync("released");
                    })
            );
            next(app);
        };
}

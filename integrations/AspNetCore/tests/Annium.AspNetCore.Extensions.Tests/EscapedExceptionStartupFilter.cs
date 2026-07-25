using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace Annium.AspNetCore.Extensions.Tests;

/// <summary>
/// Test-only <see cref="IStartupFilter" /> that wraps the entire downstream pipeline (including
/// <c>ExceptionMiddleware</c>) with a try/catch, recording on <see cref="EscapedExceptionSink" /> any
/// exception that escapes it instead of letting it abort the connection. Registered via
/// <see cref="Microsoft.Extensions.Hosting.IHostBuilder.ConfigureServices" /> by <see cref="PartialWriteTestHost" />,
/// mirroring the additive <c>IStartupFilter</c> pattern used by
/// <c>Annium.AspNetCore.IntegrationTesting.Tests.KeyedTestHost</c> /
/// <c>Annium.AspNetCore.IntegrationTesting.Tests.SlowRequestTestHost</c>: it does not modify the shared
/// <see cref="Annium.AspNetCore.TestServer" /> project's <c>Program</c> in any way,
/// it only wraps the pipeline that the started host builds from it. Because <see cref="IStartupFilter" />
/// composition applies each filter's pre-<c>next</c> code before calling <c>next</c>, the middleware added
/// here — added before <c>next(app)</c> invokes the rest of <c>Program</c>'s configuration — ends up first in
/// the built pipeline, i.e. outermost, wrapping <c>ExceptionMiddleware</c> and everything after it.
/// </summary>
internal sealed class EscapedExceptionStartupFilter : IStartupFilter
{
    /// <summary>
    /// Wraps the pipeline built by <paramref name="next" /> in a try/catch that records on
    /// <see cref="EscapedExceptionSink" /> any exception escaping it, instead of letting it abort the connection.
    /// </summary>
    /// <param name="next">The next pipeline-configuration delegate in the <see cref="IStartupFilter" /> chain.</param>
    /// <returns>
    /// An application-configuration delegate that installs the exception-recording middleware ahead of the
    /// pipeline configured by <paramref name="next" />.
    /// </returns>
    public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next) =>
        app =>
        {
            app.Use(
                async (context, nextMiddleware) =>
                {
                    try
                    {
                        await nextMiddleware(context);
                    }
                    catch (Exception ex)
                    {
                        context.RequestServices.GetService<EscapedExceptionSink>()?.Record(ex);
                    }
                }
            );
            next(app);
        };
}

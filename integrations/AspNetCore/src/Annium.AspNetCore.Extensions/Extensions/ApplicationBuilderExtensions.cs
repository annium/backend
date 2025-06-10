using System;
using Annium.AspNetCore.Extensions.Internal.Middlewares;
using Microsoft.AspNetCore.Builder;

namespace Annium.AspNetCore.Extensions.Extensions;

public static class ApplicationBuilderExtensions
{
    public static IApplicationBuilder UseExceptionMiddleware(this IApplicationBuilder builder) =>
        builder.UseMiddleware<ExceptionMiddleware>();

    public static IApplicationBuilder UseCorsDefaults(this IApplicationBuilder builder) =>
        builder.UseCors(b =>
            b.SetIsOriginAllowed(_ => true)
                .AllowAnyMethod()
                .AllowAnyHeader()
                .AllowCredentials()
                .SetPreflightMaxAge(TimeSpan.FromDays(7))
        );
}

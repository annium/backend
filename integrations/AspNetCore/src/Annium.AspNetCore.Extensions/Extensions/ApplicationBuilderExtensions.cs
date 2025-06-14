using System;
using Annium.AspNetCore.Extensions.Internal.Middlewares;
using Microsoft.AspNetCore.Builder;

// ReSharper disable once CheckNamespace
namespace Annium.AspNetCore.Extensions;

/// <summary>
/// Extension methods for configuring the ASP.NET Core application builder
/// </summary>
public static class ApplicationBuilderExtensions
{
    /// <summary>
    /// Adds the exception middleware to the application pipeline
    /// </summary>
    /// <param name="builder">The application builder to configure</param>
    /// <returns>The configured application builder</returns>
    public static IApplicationBuilder UseExceptionMiddleware(this IApplicationBuilder builder) =>
        builder.UseMiddleware<ExceptionMiddleware>();

    /// <summary>
    /// Configures CORS with default permissive settings for development
    /// </summary>
    /// <param name="builder">The application builder to configure</param>
    /// <returns>The configured application builder</returns>
    public static IApplicationBuilder UseCorsDefaults(this IApplicationBuilder builder) =>
        builder.UseCors(b =>
            b.SetIsOriginAllowed(_ => true)
                .AllowAnyMethod()
                .AllowAnyHeader()
                .AllowCredentials()
                .SetPreflightMaxAge(TimeSpan.FromDays(7))
        );
}

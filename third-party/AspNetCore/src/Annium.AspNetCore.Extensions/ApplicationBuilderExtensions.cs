using Annium.Architecture.Http.Profiles;
using Annium.AspNetCore.Extensions.Internal.Middlewares;
using Annium.Core.Runtime;
using Microsoft.AspNetCore.Builder;

[assembly: ReferTypeAssembly(typeof(HttpStatusCodeProfile))]

namespace Annium.Core.DependencyInjection
{
    public static class ApplicationBuilderExtensions
    {
        public static IApplicationBuilder UseExceptionMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<ExceptionMiddleware>();
        }
    }
}
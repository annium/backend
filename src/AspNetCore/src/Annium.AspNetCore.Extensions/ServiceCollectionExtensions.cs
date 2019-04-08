using Microsoft.AspNetCore.Builder;

namespace Annium.AspNetCore.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IApplicationBuilder UseExceptionMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<ExceptionMiddleware>();
        }
    }
}
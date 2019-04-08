using System;
using System.Net;
using System.Net.Mime;
using System.Threading.Tasks;
using Annium.Logging.Abstractions;
using Microsoft.AspNetCore.Http;

namespace Annium.AspNetCore.Extensions
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate next;

        private readonly ILogger<ExceptionMiddleware> logger;

        public ExceptionMiddleware(
            RequestDelegate next,
            ILogger<ExceptionMiddleware> logger
        )
        {
            this.next = next;
            this.logger = logger;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await next(context);
            }
            catch (Exception exception)
            {
                logger.Error(exception);
                context.Response.StatusCode = (int) HttpStatusCode.InternalServerError;
                context.Response.ContentType = MediaTypeNames.Text.Plain;

                await context.Response.WriteAsync(exception.ToString());
            }
        }
    }
}
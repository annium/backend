using System;
using System.Net;
using System.Net.Mime;
using System.Text.Json;
using System.Threading.Tasks;
using Annium.Architecture.Base;
using Annium.Core.DependencyInjection;
using Annium.Data.Operations;
using Annium.Logging.Abstractions;
using Microsoft.AspNetCore.Http;
using NodaTime;

namespace Annium.AspNetCore.Extensions.Internal.Middlewares
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate next;
        private readonly ILogger<ExceptionMiddleware> logger;
        private readonly JsonSerializerOptions jsonSerializerOptions = new JsonSerializerOptions()
            .ConfigureAbstractConverter()
            .ConfigureForOperations()
            .ConfigureForNodaTime(DateTimeZoneProviders.Serialization);

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

                var result = Result.Status(OperationStatus.UncaughtException).Error(exception.ToString());

                await context.Response.WriteAsync(JsonSerializer.Serialize(result, jsonSerializerOptions));
            }
        }
    }
}
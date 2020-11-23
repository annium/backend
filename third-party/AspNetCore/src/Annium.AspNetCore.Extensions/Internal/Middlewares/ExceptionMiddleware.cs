using System;
using System.Net;
using System.Net.Mime;
using System.Threading.Tasks;
using Annium.Architecture.Base;
using Annium.Architecture.Http.Exceptions;
using Annium.Core.DependencyInjection;
using Annium.Data.Operations;
using Annium.Logging.Abstractions;
using Annium.Serialization.Abstractions;
using Microsoft.AspNetCore.Http;

namespace Annium.AspNetCore.Extensions.Internal.Middlewares
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ISerializer<string> _serializer;
        private readonly ILogger<ExceptionMiddleware> _logger;

        public ExceptionMiddleware(
            RequestDelegate next,
            IIndex<string, ISerializer<string>> serializers,
            ILogger<ExceptionMiddleware> logger
        )
        {
            _next = next;
            _serializer = serializers[MediaTypeNames.Application.Json];
            _logger = logger;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (ValidationException e)
            {
                await WriteResponse(context, HttpStatusCode.BadRequest, e.Result);
            }
            catch (ForbiddenException e)
            {
                await WriteResponse(context, HttpStatusCode.Forbidden, e.Result);
            }
            catch (NotFoundException e)
            {
                await WriteResponse(context, HttpStatusCode.NotFound, e.Result);
            }
            catch (ConflictException e)
            {
                await WriteResponse(context, HttpStatusCode.Conflict, e.Result);
            }
            catch (ServerException e)
            {
                _logger.Error(e);

                await WriteResponse(context, HttpStatusCode.InternalServerError, e.Result);
            }
            catch (Exception e)
            {
                _logger.Error(e);
                var result = Result.Status(OperationStatus.UncaughtException).Error(e.ToString());
                await WriteResponse(context, HttpStatusCode.InternalServerError, result);
            }
        }

        private Task WriteResponse(HttpContext context, HttpStatusCode status, IResultBase result)
        {
            context.Response.StatusCode = (int) status;
            context.Response.ContentType = MediaTypeNames.Application.Json;

            return context.Response.WriteAsync(_serializer.Serialize(result));
        }
    }
}
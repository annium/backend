using System;
using System.Net;
using System.Net.Mime;
using System.Threading.Tasks;
using Annium.Architecture.Base;
using Annium.Architecture.Http.Exceptions;
using Annium.Core.DependencyInjection.Extensions;
using Annium.Data.Operations;
using Annium.Logging;
using Annium.Serialization.Abstractions;
using Microsoft.AspNetCore.Http;

namespace Annium.AspNetCore.Extensions.Internal.Middlewares;

internal class ExceptionMiddleware : ILogSubject
{
    public ILogger Logger { get; }
    private readonly RequestDelegate _next;
    private readonly Helper _helper;

    public ExceptionMiddleware(RequestDelegate next, IServiceProvider sp, ILogger logger)
    {
        _next = next;
        var serializerKey = SerializerKey.CreateDefault(MediaTypeNames.Application.Json);
        var serializer = sp.ResolveKeyed<ISerializer<string>>(serializerKey);
        _helper = new Helper(serializer, MediaTypeNames.Application.Json);
        Logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (ValidationException e)
        {
            await _helper.WriteResponseAsync(context, HttpStatusCode.BadRequest, e.Result);
        }
        catch (ForbiddenException e)
        {
            await _helper.WriteResponseAsync(context, HttpStatusCode.Forbidden, e.Result);
        }
        catch (NotFoundException e)
        {
            await _helper.WriteResponseAsync(context, HttpStatusCode.NotFound, e.Result);
        }
        catch (ConflictException e)
        {
            await _helper.WriteResponseAsync(context, HttpStatusCode.Conflict, e.Result);
        }
        catch (ServerException e)
        {
            this.Error(e);

            await _helper.WriteResponseAsync(context, HttpStatusCode.InternalServerError, e.Result);
        }
        catch (Exception e)
        {
            this.Error(e);
            var result = Result.Status(OperationStatus.UncaughtError).Error(e.ToString());
            await _helper.WriteResponseAsync(context, HttpStatusCode.InternalServerError, result);
        }
    }
}

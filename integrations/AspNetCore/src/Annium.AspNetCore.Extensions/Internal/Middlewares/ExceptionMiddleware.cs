using System;
using System.Net;
using System.Net.Mime;
using System.Threading.Tasks;
using Annium.Architecture.Base;
using Annium.Architecture.Http.Exceptions;
using Annium.Core.DependencyInjection;
using Annium.Data.Operations;
using Annium.Logging;
using Annium.Serialization.Abstractions;
using Microsoft.AspNetCore.Http;

namespace Annium.AspNetCore.Extensions.Internal.Middlewares;

/// <summary>
/// Middleware that handles exceptions and converts them to appropriate HTTP responses
/// </summary>
internal class ExceptionMiddleware : ILogSubject
{
    /// <summary>
    /// Gets the logger for this middleware
    /// </summary>
    public ILogger Logger { get; }

    /// <summary>
    /// The next middleware in the pipeline
    /// </summary>
    private readonly RequestDelegate _next;

    /// <summary>
    /// The helper for writing HTTP responses
    /// </summary>
    private readonly Helper _helper;

    /// <summary>
    /// Initializes a new instance of the ExceptionMiddleware class
    /// </summary>
    /// <param name="next">The next middleware in the pipeline</param>
    /// <param name="sp">The service provider for dependency resolution</param>
    /// <param name="logger">The logger for error reporting</param>
    public ExceptionMiddleware(RequestDelegate next, IServiceProvider sp, ILogger logger)
    {
        _next = next;
        var serializerKey = SerializerKey.CreateDefault(MediaTypeNames.Application.Json);
        var serializer = sp.ResolveKeyed<ISerializer<string>>(serializerKey);
        _helper = new Helper(serializer, MediaTypeNames.Application.Json);
        Logger = logger;
    }

    /// <summary>
    /// Invokes the middleware to handle the HTTP request
    /// </summary>
    /// <param name="context">The HTTP context for the current request</param>
    /// <returns>A task that represents the asynchronous operation</returns>
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

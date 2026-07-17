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
        HttpStatusCode status;
        IResultBase result;

        try
        {
            await _next(context);
            return;
        }
        catch (ValidationException e)
        {
            status = HttpStatusCode.BadRequest;
            result = e.Result;

            if (WriteFailedSilently(context, e))
                return;
        }
        catch (ForbiddenException e)
        {
            status = HttpStatusCode.Forbidden;
            result = e.Result;

            if (WriteFailedSilently(context, e))
                return;
        }
        catch (NotFoundException e)
        {
            status = HttpStatusCode.NotFound;
            result = e.Result;

            if (WriteFailedSilently(context, e))
                return;
        }
        catch (ConflictException e)
        {
            status = HttpStatusCode.Conflict;
            result = e.Result;

            if (WriteFailedSilently(context, e))
                return;
        }
        catch (ServerException e)
        {
            this.Error(e);

            status = HttpStatusCode.InternalServerError;
            result = e.Result;

            if (context.Response.HasStarted)
                return;
        }
        catch (Exception e)
        {
            this.Error(e);

            status = HttpStatusCode.InternalServerError;
            result = Result.Status(OperationStatus.UncaughtError).Error(e.ToString());

            if (context.Response.HasStarted)
                return;
        }

        await _helper.WriteResponseAsync(context, status, result);
    }

    /// <summary>
    /// Checks whether the response has already started for the given exception; if so, logs the exception
    /// (since the typed branches above don't otherwise log their cause) and reports that the caller should
    /// return without attempting to write an HTTP status/body, which would throw a secondary
    /// <see cref="InvalidOperationException" /> masking this one.
    /// </summary>
    /// <param name="context">The HTTP context for the current request</param>
    /// <param name="e">The exception that would otherwise be written as a response</param>
    /// <returns><c>true</c> if the response has already started and the exception was logged instead</returns>
    private bool WriteFailedSilently(HttpContext context, Exception e)
    {
        if (!context.Response.HasStarted)
            return false;

        this.Error(e);

        return true;
    }
}

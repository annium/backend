using System;
using System.Threading;
using System.Threading.Tasks;
using Annium.Core.Mediator;
using Annium.Logging.Abstractions;

namespace Demo.Core.Mediator.Handlers;

internal class LoggingHandler<TRequest, TResponse> :
    IPipeRequestHandler<TRequest, TRequest, TResponse, TResponse>,
    ILogSubject
{
    public ILogger Logger { get; }

    public LoggingHandler(
        ILogger<LoggingHandler<TRequest, TResponse>> logger
    )
    {
        Logger = logger;
    }

    public async Task<TResponse> HandleAsync(
        TRequest request,
        CancellationToken ct,
        Func<TRequest, CancellationToken, Task<TResponse>> next
    )
    {
        this.Log().Trace($"Start {typeof(TRequest).Name} handle");
        var result = await next(request, ct);
        this.Log().Trace($"Complete {typeof(TRequest).Name} handle");

        return result;
    }
}
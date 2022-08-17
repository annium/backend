using System;
using System.Threading;
using System.Threading.Tasks;
using Annium.Architecture.Base;
using Annium.Data.Operations;
using Annium.Extensions.Composition;
using Annium.Logging.Abstractions;

namespace Annium.Architecture.Mediator.Internal.PipeHandlers;

internal abstract class CompositionPipeHandlerBase<TRequest, TResponse> : ILogSubject<CompositionPipeHandlerBase<TRequest, TResponse>>
    where TRequest : class
{
    public ILogger<CompositionPipeHandlerBase<TRequest, TResponse>> Logger { get; }
    private readonly IComposer<TRequest> _composer;

    public CompositionPipeHandlerBase(
        IComposer<TRequest> composer,
        ILogger<CompositionPipeHandlerBase<TRequest, TResponse>> logger
    )
    {
        _composer = composer;
        Logger = logger;
    }

    public async Task<TResponse> HandleAsync(
        TRequest request,
        CancellationToken ct,
        Func<TRequest, CancellationToken, Task<TResponse>> next
    )
    {
        this.Log().Trace($"Compose {typeof(TRequest)}");
        var result = await _composer.ComposeAsync(request);
        if (result.HasErrors)
        {
            this.Log().Trace($"Composition of {typeof(TRequest)} failed");

            return GetResponse(result);
        }

        return await next(request, ct);
    }

    protected abstract TResponse GetResponse(IStatusResult<OperationStatus> compositionResult);
}
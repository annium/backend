using System;
using System.Threading;
using System.Threading.Tasks;
using Annium.Core.Mediator;
using Annium.Data.Operations;
using Annium.Logging.Abstractions;

namespace Demo.Core.Mediator.Handlers;

internal class ExceptionHandler<TRequest, TResponse> :
    IPipeRequestHandler<TRequest, TRequest, IBooleanResult<TResponse>, IBooleanResult<TResponse>>,
    ILogSubject<ExceptionHandler<TRequest, TResponse>>
{
    public ILogger<ExceptionHandler<TRequest, TResponse>> Logger { get; }

    public ExceptionHandler(
        ILogger<ExceptionHandler<TRequest, TResponse>> logger
    )
    {
        Logger = logger;
    }

    public async Task<IBooleanResult<TResponse>> HandleAsync(
        TRequest request,
        CancellationToken ct,
        Func<TRequest, CancellationToken, Task<IBooleanResult<TResponse>>> next
    )
    {
        try
        {
            var result = await next(request, ct);
            this.Log().Trace($"Request {typeof(TRequest).Name} complete without errors");

            return result;
        }
        catch (Exception exception)
        {
            this.Log().Trace($"Request {typeof(TRequest).Name} failed with {exception}");
            return Result.Failure(default(TResponse)!).Error(exception.Message);
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Annium.Core.Mediator;
using Annium.Data.Operations;
using Annium.Extensions.Validation;
using Annium.Logging.Abstractions;

namespace Demo.Core.Mediator.Handlers;

internal class ValidationHandler<TRequest, TResponse> :
    IPipeRequestHandler<TRequest, TRequest, TResponse, IBooleanResult<TResponse>>,
    ILogSubject<ValidationHandler<TRequest, TResponse>>
{
    public ILogger<ValidationHandler<TRequest, TResponse>> Logger { get; }
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public ValidationHandler(
        IEnumerable<IValidator<TRequest>> validators,
        ILogger<ValidationHandler<TRequest, TResponse>> logger
    )
    {
        _validators = validators;
        Logger = logger;
    }

    public async Task<IBooleanResult<TResponse>> HandleAsync(
        TRequest request,
        CancellationToken ct,
        Func<TRequest, CancellationToken, Task<TResponse>> next
    )
    {
        this.Log().Trace($"Start {typeof(TRequest).Name} validation");
        var result = Result.Failure(default(TResponse)!)
            .Join(await Task.WhenAll(_validators.Select(v => v.ValidateAsync(request))));
        this.Log().Trace($"Status of {typeof(TRequest).Name} validation: {result.IsFailure}");
        if (result.HasErrors)
            return result;

        var response = await next(request, ct);

        return Result.Success(response);
    }
}
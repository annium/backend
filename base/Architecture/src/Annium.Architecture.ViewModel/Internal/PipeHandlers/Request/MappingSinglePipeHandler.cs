using System;
using System.Threading;
using System.Threading.Tasks;
using Annium.Core.Mapper;
using Annium.Core.Mediator;
using Annium.Logging.Abstractions;

namespace Annium.Architecture.ViewModel.Internal.PipeHandlers.Request;

internal class MappingSinglePipeHandler<TRequestIn, TRequestOut, TResponse> :
    IPipeRequestHandler<
        TRequestIn,
        TRequestOut,
        TResponse,
        TResponse
    >,
    ILogSubject<MappingSinglePipeHandler<TRequestIn, TRequestOut, TResponse>>
    where TRequestIn : IRequest<TRequestOut>
{
    public ILogger<MappingSinglePipeHandler<TRequestIn, TRequestOut, TResponse>> Logger { get; }
    private readonly IMapper _mapper;

    public MappingSinglePipeHandler(
        IMapper mapper,
        ILogger<MappingSinglePipeHandler<TRequestIn, TRequestOut, TResponse>> logger
    )
    {
        _mapper = mapper;
        Logger = logger;
    }

    public Task<TResponse> HandleAsync(
        TRequestIn request,
        CancellationToken ct,
        Func<TRequestOut, CancellationToken, Task<TResponse>> next
    )
    {
        this.Log().Trace($"Map request: {typeof(TRequestIn)} -> {typeof(TRequestOut)}");
        var mappedRequest = _mapper.Map<TRequestOut>(request);

        return next(mappedRequest, ct);
    }
}